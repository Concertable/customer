import { useState } from "react";
import { useParams, useRouter } from "@tanstack/react-router";
import dayjs from "dayjs";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import type { TicketPurchasedPayload } from "@/features/notifications";
import {
  useConcert,
  useTicketCheckoutQuery,
  type Concert,
} from "@/features/concerts";
import { useCheckoutFlow, type CheckoutFlowState } from "@/features/concerts/hooks/useCheckoutFlow";
import { CheckoutLayout } from "@/features/concerts/components/checkout/CheckoutLayout";
import { CheckoutSection } from "@/features/concerts/components/checkout/CheckoutSection";
import { CheckoutEventBanner } from "@/features/concerts/components/checkout/CheckoutEventBanner";
import { OrderSummaryCard } from "@/features/concerts/components/checkout/OrderSummaryCard";
import { QuantitySelector } from "@/features/concerts/components/checkout/QuantitySelector";
import { CheckoutSuccess } from "@/features/concerts/components/checkout/CheckoutSuccess";
import { CheckoutFlow } from "@/features/concerts/components/checkout/CheckoutFlow";
import { StripePaymentForm } from "@/features/concerts/components/checkout/StripePaymentForm";

export function TicketCheckoutPage() {
  const { id } = useParams({ from: "/_customer/concert/checkout/$id" });
  const { concert, isLoading, isError } = useConcert(id);

  if (isLoading) return <CheckoutSkeleton />;
  if (isError || !concert)
    return <div className="text-destructive p-6">Concert not found.</div>;

  return <TicketCheckoutForm concert={concert} />;
}

const config = {
  title: "Processing your payment",
  timeoutTitle: "Still confirming your payment",
  pendingHint: "Your tickets will appear in your profile",
  steps: { first: "Payment authorised", final: "Issuing your tickets" },
};

interface Props {
  concert: Concert;
  flow: CheckoutFlowState<TicketPurchasedPayload>;
}

export function TicketCheckoutFlow({ concert, flow }: Readonly<Props>) {
  const router = useRouter();

  return (
    <CheckoutFlow
      flow={flow}
      {...config}
      renderSuccess={(payload) => (
        <TicketCheckoutSuccess
          concert={concert}
          ticketCount={payload.ticketIds.length}
          onView={() => void router.navigate({ to: "/profile/tickets/upcoming" })}
        />
      )}
    />
  );
}

function TicketCheckoutForm({ concert }: { concert: Concert }) {
  const [quantity, setQuantity] = useState(1);
  const [submitted, setSubmitted] = useState(false);
  const flow = useCheckoutFlow<TicketPurchasedPayload>({ event: "TicketPurchased" });
  const {
    data: checkout,
    isLoading: isCheckoutLoading,
    isError: isCheckoutError,
    isFetching,
  } = useTicketCheckoutQuery(concert.id, quantity);

  if (submitted) return <TicketCheckoutFlow concert={concert} flow={flow} />;
  if (isCheckoutLoading) return <CheckoutSkeleton />;
  if (isCheckoutError || !checkout)
    return (
      <div className="text-destructive p-6">Could not start checkout.</div>
    );

  const total = concert.price * quantity;

  return (
    <CheckoutLayout
      banner={
        <CheckoutEventBanner
          title={concert.name}
          subtitle={`${concert.venue.name} · ${concert.venue.town}`}
          meta={dayjs(concert.startDate).format("dddd, D MMM YYYY · HH:mm")}
        />
      }
      summary={
        <OrderSummaryCard
          lines={[
            {
              label: "Price per ticket",
              value: `£${concert.price.toFixed(2)}`,
            },
            {
              label: "Quantity",
              value: (
                <QuantitySelector
                  value={quantity}
                  onChange={setQuantity}
                  max={concert.availableTickets}
                />
              ),
            },
          ]}
          total={{ label: "Total", value: `£${total.toFixed(2)}` }}
        />
      }
    >
      <CheckoutSection title="Payment Method">
        <StripePaymentForm
          session={checkout.session}
          submitLabel={`Pay £${total.toFixed(2)}`}
          onSuccess={() => setSubmitted(true)}
          disabled={isFetching}
        />
      </CheckoutSection>
    </CheckoutLayout>
  );
}

function TicketCheckoutSuccess({
  concert,
  ticketCount,
  onView,
}: {
  concert: Concert;
  ticketCount: number;
  onView: () => void;
}) {
  return (
    <CheckoutSuccess
      title="Tickets confirmed"
      description={
        <>
          Your {ticketCount > 1 ? `${ticketCount} tickets` : "ticket"} for{" "}
          <span className="text-foreground font-medium">{concert.name}</span>{" "}
          {ticketCount > 1 ? "are" : "is"} ready.
        </>
      }
      footer={
        <Button onClick={onView} className="mt-2" data-testid="view-tickets">
          View tickets
        </Button>
      }
    />
  );
}

function CheckoutSkeleton() {
  return (
    <div className="mx-auto max-w-6xl space-y-6 px-6 py-8 lg:px-10 lg:py-10">
      <Skeleton className="h-8 w-32" />
      <Skeleton className="h-16 w-full" />
      <div className="grid gap-10 lg:grid-cols-[minmax(0,1fr)_400px] lg:gap-16">
        <Skeleton className="h-44 w-full rounded-lg" />
        <Skeleton className="h-72 w-full rounded-lg" />
      </div>
    </div>
  );
}
