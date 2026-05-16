import { createFileRoute } from "@tanstack/react-router";
import { TicketsPage } from "@/features/concerts";

export const Route = createFileRoute("/_customer/profile/tickets/")({
  component: TicketsPage,
});
