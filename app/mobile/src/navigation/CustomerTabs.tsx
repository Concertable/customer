import { createBottomTabNavigator } from "@react-navigation/bottom-tabs";
import { Home, MessageCircle, Search, Ticket, User } from "lucide-react-native";
import { HomeStack } from "./HomeStack";
import { SearchStack } from "./SearchStack";
import { TicketsStack } from "./TicketsStack";
import { ProfileStack } from "./ProfileStack";
import { MessagesScreen } from "shared/features/user/screens/MessagesScreen";
import { useCustomerNotifications } from "../features/notifications";
import { logger } from "shared/lib/logger";
import { theme } from "shared/lib/theme";
import type { CustomerTabParamList } from "./types";

const Tab = createBottomTabNavigator<CustomerTabParamList>();

export function CustomerTabs() {
  logger.log("[CustomerTabs] render");
  useCustomerNotifications();
  return (
    <Tab.Navigator
      screenOptions={{
        headerShown: false,
        tabBarActiveTintColor: theme.primary,
        tabBarInactiveTintColor: theme.mutedForeground,
        tabBarStyle: { borderTopColor: theme.border },
      }}
    >
      <Tab.Screen
        name="HomeTab"
        component={HomeStack}
        options={{ title: "Home", tabBarIcon: ({ color, size }) => <Home size={size} color={color} /> }}
      />
      <Tab.Screen
        name="SearchTab"
        component={SearchStack}
        options={{ title: "Search", tabBarIcon: ({ color, size }) => <Search size={size} color={color} /> }}
      />
      <Tab.Screen
        name="TicketsTab"
        component={TicketsStack}
        options={{ title: "Tickets", tabBarIcon: ({ color, size }) => <Ticket size={size} color={color} /> }}
      />
      <Tab.Screen
        name="Messages"
        component={MessagesScreen}
        options={{ title: "Messages", tabBarIcon: ({ color, size }) => <MessageCircle size={size} color={color} /> }}
      />
      <Tab.Screen
        name="ProfileTab"
        component={ProfileStack}
        options={{ title: "Profile", tabBarIcon: ({ color, size }) => <User size={size} color={color} /> }}
      />
    </Tab.Navigator>
  );
}
