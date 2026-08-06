import { createFileRoute } from "@tanstack/react-router";
import { requireAuth } from "@concertable/web/features/auth";
import { SettingsLayout } from "@concertable/web/components/SettingsLayout";

export const Route = createFileRoute("/_customer/settings")({
  beforeLoad: ({ location }) => requireAuth({ location }),
  component: SettingsLayout,
});
