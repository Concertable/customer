import { CustomerProfileScreen } from "../features/user/screens/CustomerProfileScreen";
import { EditProfileScreen } from "shared/features/user/screens/EditProfileScreen";
import { LocationScreen } from "shared/features/user/screens/LocationScreen";
import { PreferencesScreen } from "../features/user/screens/PreferencesScreen";
import { createAppStack } from "shared/navigation/createAppStack";
import type { CustomerProfileStackParamList } from "./types";

const Stack = createAppStack<CustomerProfileStackParamList>();

export function ProfileStack() {
  return (
    <Stack.Navigator>
      <Stack.Screen name="ProfileMain" component={CustomerProfileScreen} options={{ headerShown: false }} />
      <Stack.Screen name="EditProfile" component={EditProfileScreen} options={{ title: "Edit Profile" }} />
      <Stack.Screen name="Location" component={LocationScreen} options={{ title: "Location" }} />
      <Stack.Screen name="Preferences" component={PreferencesScreen} options={{ title: "Preferences" }} />
    </Stack.Navigator>
  );
}
