import { useNavigation } from "@react-navigation/native";
import type { NativeStackNavigationProp } from "@react-navigation/native-stack";
import { ProfileScreen } from "shared/features/user/screens/ProfileScreen";
import type { CustomerProfileStackParamList } from "../../../navigation/types";

type ProfileNav = NativeStackNavigationProp<CustomerProfileStackParamList>;

export function CustomerProfileScreen() {
  const nav = useNavigation<ProfileNav>();

  return (
    <ProfileScreen
      accountItems={[{ label: "Preferences", onPress: () => nav.navigate("Preferences") }]}
    />
  );
}
