import { createFileRoute } from "@tanstack/react-router";
import { ProfilePage } from "@concertable/web/features/user";

export const Route = createFileRoute("/_customer/profile/")({
  component: ProfilePage,
});
