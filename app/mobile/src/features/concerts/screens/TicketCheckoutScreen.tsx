import { useEffect, useState } from "react";
import { ActivityIndicator, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";
import { useNavigation, useRoute } from "@react-navigation/native";
import type { RouteProp } from "@react-navigation/native";
import type { NativeStackNavigationProp } from "@react-navigation/native-stack";
import { useStripe } from "@stripe/stripe-react-native";
import { useConcert, useCheckoutFlow } from "@concertable/shared/features/concerts";
import { useTicketCheckoutQuery } from "@customer/shared/features/tickets";
import type { TicketPurchasedPayload } from "@customer/shared/features/notifications";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { Text } from "@/components/ui/text";
import { ErrorState } from "@/components/ui/ErrorState";
import { QuantitySelector } from "@/components/ui/QuantitySelector";
import { CheckoutAwaiting } from "../components/CheckoutAwaiting";
import { notify } from "shared/lib/toast";
import { logger } from "shared/lib/logger";
import { notificationConnection } from "shared/lib/signalr";
import { theme, stripeColors } from "shared/lib/theme";
import dayjs from "dayjs";
import type { CustomerConcertNavParamList } from "../../../navigation/types";

type CheckoutRoute = RouteProp<CustomerConcertNavParamList, "TicketCheckout">;
type CheckoutNav = NativeStackNavigationProp<CustomerConcertNavParamList>;

function TicketCheckoutAwaitingFlow() {
  const nav = useNavigation<CheckoutNav>();
  const flow = useCheckoutFlow<TicketPurchasedPayload>({
    connection: notificationConnection,
    event: "TicketPurchased",
  });

  useEffect(() => {
    logger.log("[TicketCheckoutScreen] flow", {
      phase: flow.phase,
      hasResult: "result" in flow,
    });
    if (flow.phase === "success") {
      logger.log("[TicketCheckoutScreen] phase=success → nav.replace");
      nav.replace("CheckoutSuccess", { ticketCount: flow.result.ticketIds.length });
    }
  }, [flow, nav]);

  logger.log("[TicketCheckoutScreen] rendering CheckoutAwaiting", { phase: flow.phase });
  return <CheckoutAwaiting timed_out={flow.phase === "timeout"} />;
}

export function TicketCheckoutScreen() {
  const route = useRoute<CheckoutRoute>();
  const { concertId } = route.params;

  const { initPaymentSheet, presentPaymentSheet } = useStripe();

  const { concert } = useConcert(concertId);

  const [qty, setQty] = useState(1);
  const [ready, setReady] = useState(false);
  const [paying, setPaying] = useState(false);
  const [submitted, setSubmitted] = useState(false);

  const { data: checkout, isLoading, isError, isFetching } = useTicketCheckoutQuery(concertId, qty);

  useEffect(() => {
    if (!checkout) return;
    setReady(false);
    initPaymentSheet({
      merchantDisplayName: "Concertable",
      paymentIntentClientSecret: checkout.session.clientSecret,
      returnURL: "concertable://checkout",
      allowsDelayedPaymentMethods: false,
      appearance: {
        colors: {
          primary: stripeColors.primary,
          background: stripeColors.background,
        },
      },
      ...(checkout.session.customerId ? { customerId: checkout.session.customerId } : {}),
      ...(checkout.session.customerSession ? { customerSessionClientSecret: checkout.session.customerSession } : {}),
    }).then(({ error }) => {
      if (error) notify(error.message, "error");
      else setReady(true);
    });
  }, [checkout]);

  async function handlePay() {
    setPaying(true);
    const { error } = await presentPaymentSheet();
    setPaying(false);
    logger.log("[TicketCheckoutScreen] presentPaymentSheet resolved", {
      stripeError: error?.code,
      connectionState: notificationConnection.state,
    });
    if (error) {
      if (error.code !== "Canceled") notify(error.message, "error");
      return;
    }
    logger.log("[TicketCheckoutScreen] submitted — waiting for TicketPurchased", {
      connectionState: notificationConnection.state,
    });
    setSubmitted(true);
  }

  if (submitted)
    return <TicketCheckoutAwaitingFlow />;

  if (isLoading) {
    return (
      <View className="flex-1 bg-background p-4 gap-4">
        <Skeleton className="w-full h-20 rounded-2xl" />
        <Skeleton className="w-full h-[140px] rounded-2xl" />
        <Skeleton className="w-full h-[52px] rounded-2xl mt-auto" />
      </View>
    );
  }

  if (isError || !checkout) {
    return (
      <View className="flex-1 bg-background">
        <ErrorState message="Failed to start checkout." />
      </View>
    );
  }

  const total = (checkout.price * qty).toFixed(2);
  const maxQty = concert?.availableTickets ?? 10;

  return (
    <SafeAreaView className="flex-1 bg-background" edges={["bottom"]}>
      <View className="flex-1 p-4 gap-4">
        <View className="bg-card rounded-2xl border border-border px-4 py-3 gap-0.5">
          <Text className="text-base font-semibold text-foreground" numberOfLines={1}>
            {concert?.name ?? "Loading…"}
          </Text>
          <Text className="text-xs text-muted-foreground">
            {concert
              ? `${concert.venue.name} · ${dayjs(concert.startDate).format("D MMM YYYY")}`
              : ""}
          </Text>
        </View>

        <View className="bg-muted/40 rounded-2xl border border-border p-4 gap-3">
          <View className="flex-row items-center justify-between">
            <Text className="text-sm text-muted-foreground">Price per ticket</Text>
            <Text className="text-sm font-medium text-foreground">£{checkout.price.toFixed(2)}</Text>
          </View>
          <View className="flex-row items-center justify-between">
            <Text className="text-sm text-muted-foreground">Quantity</Text>
            <QuantitySelector value={qty} onChange={setQty} min={1} max={maxQty} />
          </View>
          <View className="h-px bg-border" />
          <View className="flex-row items-center justify-between">
            <Text className="text-sm font-semibold text-foreground">Total</Text>
            <Text className="text-2xl font-bold text-foreground">£{total}</Text>
          </View>
        </View>

        <View className="mt-auto">
          <Button disabled={paying || !ready || isFetching} onPress={handlePay} size="lg" testID="checkout-pay">
            {paying || isFetching
              ? <ActivityIndicator size="small" color={theme.primaryForeground} />
              : <Text>{ready ? `Pay £${total}` : "Loading…"}</Text>}
          </Button>
        </View>
      </View>
    </SafeAreaView>
  );
}
