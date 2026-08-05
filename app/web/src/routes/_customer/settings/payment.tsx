import { createFileRoute } from "@tanstack/react-router";
import { PaymentPage } from "@concertable/web/shared/features/payments";

export const Route = createFileRoute("/_customer/settings/payment")({
  component: PaymentPage,
});
