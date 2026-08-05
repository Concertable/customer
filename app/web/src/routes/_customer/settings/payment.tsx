import { createFileRoute } from "@tanstack/react-router";
import { PaymentPage } from "@concertable/web/features/payments";

export const Route = createFileRoute("/_customer/settings/payment")({
  component: PaymentPage,
});
