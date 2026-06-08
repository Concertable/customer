import { createFileRoute } from "@tanstack/react-router";
import { requireAuth } from "@/features/auth";
import { SettingsLayout } from "@/components/SettingsLayout";

export const Route = createFileRoute("/_customer/settings")({
  beforeLoad: ({ location }) => requireAuth({ location }),
  component: SettingsLayout,
});
