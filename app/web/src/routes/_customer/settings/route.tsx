import { createFileRoute } from "@tanstack/react-router";
import { requireAuth } from "@concertable/web/shared/features/auth";
import { SettingsLayout } from "@concertable/web/shared/components/SettingsLayout";

export const Route = createFileRoute("/_customer/settings")({
  beforeLoad: ({ location }) => requireAuth({ location }),
  component: SettingsLayout,
});
