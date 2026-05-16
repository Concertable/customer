import { createFileRoute } from "@tanstack/react-router";
import { TicketHistoryPage } from "@/features/concerts";

export const Route = createFileRoute("/_customer/profile/tickets/history")({
  component: TicketHistoryPage,
});
