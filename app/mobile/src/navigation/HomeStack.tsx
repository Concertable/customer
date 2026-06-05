import { HomeScreen } from "../features/search/screens/HomeScreen";
import { CustomerConcertDetailScreen } from "../features/concerts/screens/CustomerConcertDetailScreen";
import { TicketCheckoutScreen } from "../features/concerts/screens/TicketCheckoutScreen";
import { CheckoutSuccessScreen } from "../features/concerts/screens/CheckoutSuccessScreen";
import { ArtistDetailScreen } from "shared/features/artists/screens/ArtistDetailScreen";
import { VenueDetailScreen } from "shared/features/venues/screens/VenueDetailScreen";
import { createAppStack } from "shared/navigation/createAppStack";
import type { HomeStackParamList } from "./types";

const Stack = createAppStack<HomeStackParamList>();

export function HomeStack() {
  return (
    <Stack.Navigator>
      <Stack.Screen name="HomeMain" component={HomeScreen} options={{ headerShown: false }} />
      <Stack.Screen name="ConcertDetail" component={CustomerConcertDetailScreen} options={{ title: "Concert" }} />
      <Stack.Screen name="TicketCheckout" component={TicketCheckoutScreen} options={{ title: "Buy Ticket" }} />
      <Stack.Screen name="CheckoutSuccess" component={CheckoutSuccessScreen} options={{ headerShown: false }} />
      <Stack.Screen name="ArtistDetail" component={ArtistDetailScreen} options={{ title: "Artist" }} />
      <Stack.Screen name="VenueDetail" component={VenueDetailScreen} options={{ title: "Venue" }} />
    </Stack.Navigator>
  );
}
