import { TicketsScreen } from "../features/concerts/screens/TicketsScreen";
import { TicketDetailScreen } from "../features/concerts/screens/TicketDetailScreen";
import { createAppStack } from "shared/navigation/createAppStack";
import type { TicketsStackParamList } from "./types";

const Stack = createAppStack<TicketsStackParamList>();

export function TicketsStack() {
  return (
    <Stack.Navigator>
      <Stack.Screen name="TicketsMain" component={TicketsScreen} options={{ headerShown: false }} />
      <Stack.Screen name="TicketDetail" component={TicketDetailScreen} options={{ title: "Ticket" }} />
    </Stack.Navigator>
  );
}
