import { createFileRoute } from "@tanstack/react-router";
import { LocationPage } from "@/features/user";

export const Route = createFileRoute("/_customer/profile/location")({
  component: LocationPage,
});
