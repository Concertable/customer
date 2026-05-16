import { createFileRoute } from "@tanstack/react-router";
import { PreferencesPage } from "@/features/customer";

export const Route = createFileRoute("/_customer/profile/preferences/")({
  component: PreferencesPage,
});
