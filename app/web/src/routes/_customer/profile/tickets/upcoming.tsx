import { createFileRoute } from "@tanstack/react-router";
import { UpcomingTicketsPage } from "@/features/concerts";

export const Route = createFileRoute("/_customer/profile/tickets/upcoming")({
  component: UpcomingTicketsPage,
});
