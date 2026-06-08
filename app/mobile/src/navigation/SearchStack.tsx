import { SearchScreen } from "../features/search/screens/SearchScreen";
import { CustomerConcertDetailScreen } from "../features/concerts/screens/CustomerConcertDetailScreen";
import { TicketCheckoutScreen } from "../features/concerts/screens/TicketCheckoutScreen";
import { CheckoutSuccessScreen } from "../features/concerts/screens/CheckoutSuccessScreen";
import { ArtistDetailScreen } from "shared/features/artists/screens/ArtistDetailScreen";
import { VenueDetailScreen } from "shared/features/venues/screens/VenueDetailScreen";
import { createAppStack } from "shared/navigation/createAppStack";
import type { SearchStackParamList } from "./types";

const Stack = createAppStack<SearchStackParamList>();

export function SearchStack() {
  return (
    <Stack.Navigator>
      <Stack.Screen name="SearchMain" component={SearchScreen} options={{ headerShown: false }} />
      <Stack.Screen name="ConcertDetail" component={CustomerConcertDetailScreen} options={{ title: "Concert" }} />
      <Stack.Screen name="TicketCheckout" component={TicketCheckoutScreen} options={{ title: "Buy Ticket" }} />
      <Stack.Screen name="CheckoutSuccess" component={CheckoutSuccessScreen} options={{ headerShown: false }} />
      <Stack.Screen name="ArtistDetail" component={ArtistDetailScreen} options={{ title: "Artist" }} />
      <Stack.Screen name="VenueDetail" component={VenueDetailScreen} options={{ title: "Venue" }} />
    </Stack.Navigator>
  );
}
