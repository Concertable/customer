import { createFileRoute } from "@tanstack/react-router";
import { TicketCheckoutPage } from "@/features/concerts";

export const Route = createFileRoute("/_customer/concert/checkout/$id")({
  params: {
    parse: (params) => ({ id: Number(params.id) }),
    stringify: (params) => ({ id: String(params.id) }),
  },
  component: TicketCheckoutPage,
});
