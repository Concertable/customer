import { useNavigation, useRoute } from "@react-navigation/native";
import type { RouteProp } from "@react-navigation/native";
import type { NativeStackNavigationProp } from "@react-navigation/native-stack";
import { ConcertDetailScreen } from "shared/features/concerts/screens/ConcertDetailScreen";
import type { CustomerConcertNavParamList } from "../../../navigation/types";

type ConcertNav = NativeStackNavigationProp<CustomerConcertNavParamList>;
type ConcertDetailRoute = RouteProp<CustomerConcertNavParamList, "ConcertDetail">;

export function CustomerConcertDetailScreen() {
  const nav = useNavigation<ConcertNav>();
  const route = useRoute<ConcertDetailRoute>();
  const { concertId } = route.params;

  return <ConcertDetailScreen onBuyTickets={() => nav.navigate("TicketCheckout", { concertId })} />;
}
