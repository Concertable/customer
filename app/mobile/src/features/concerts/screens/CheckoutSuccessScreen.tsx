import { useRef } from "react";
import { Animated, View } from "react-native";
import { useMountEffect } from "@concertable/shared/hooks/useMountEffect";
import { SafeAreaView } from "react-native-safe-area-context";
import { useNavigation, useRoute } from "@react-navigation/native";
import type { RouteProp } from "@react-navigation/native";
import type { BottomTabNavigationProp } from "@react-navigation/bottom-tabs";
import { CheckCircle } from "lucide-react-native";
import { Button } from "@/components/ui/button";
import { Text } from "@/components/ui/text";
import { theme } from "shared/lib/theme";
import type { CustomerConcertNavParamList, CustomerTabParamList } from "../../../navigation/types";

type SuccessRoute = RouteProp<CustomerConcertNavParamList, "CheckoutSuccess">;
type TabNav = BottomTabNavigationProp<CustomerTabParamList>;

export function CheckoutSuccessScreen() {
  const route = useRoute<SuccessRoute>();
  const tabNav = useNavigation<TabNav>();
  const ticketCount = route.params?.ticketCount ?? 1;
  const scale = useRef(new Animated.Value(0)).current;

  useMountEffect(() => {
    Animated.spring(scale, {
      toValue: 1,
      tension: 60,
      friction: 6,
      useNativeDriver: true,
    }).start();
  });

  return (
    <SafeAreaView className="flex-1 bg-background" testID="checkout-success">
      <View className="flex-1 items-center justify-center gap-8 px-6">
        <Animated.View style={{ transform: [{ scale }] }}>
          <View className="w-24 h-24 rounded-full bg-success/15 items-center justify-center">
            <CheckCircle size={52} color={theme.success} strokeWidth={1.5} />
          </View>
        </Animated.View>

        <View className="items-center gap-2">
          <Text className="text-2xl font-bold text-foreground">You're going!</Text>
          <Text className="text-sm text-muted-foreground text-center leading-relaxed">
            {ticketCount > 1
              ? `Your ${ticketCount} tickets have been confirmed.`
              : "Your ticket has been confirmed."}{"\n"}See you there!
          </Text>
        </View>

        <View className="w-full gap-3">
          <Button
            size="lg"
            onPress={() => tabNav.navigate("TicketsTab", { screen: "TicketsMain" })}
            testID="view-tickets"
          >
            <Text>View My Tickets</Text>
          </Button>
          <Button
            variant="outline"
            size="lg"
            onPress={() => tabNav.navigate("HomeTab", { screen: "HomeMain" })}
          >
            <Text>Back to Home</Text>
          </Button>
        </View>
      </View>
    </SafeAreaView>
  );
}
