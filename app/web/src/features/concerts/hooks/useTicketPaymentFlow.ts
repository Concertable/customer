import { useEffect, useRef, useState } from "react";
import type {
  TicketPurchaseFailedPayload,
  TicketPurchasedPayload,
} from "@concertable/customer/features/notifications/types";
import { paymentOperationReferencesMatch } from "@concertable/customer/features/tickets";
import type { PaymentOperationReference } from "@concertable/customer/features/tickets/types";
import type { CheckoutFlowState } from "@concertable/web/features/concerts/hooks/useCheckoutFlow";
import { notificationConnection } from "@concertable/web/lib/signalr";

export function useTicketPaymentFlow(reference?: PaymentOperationReference) {
  const [submitted, setSubmitted] = useState(false);
  const [paymentError, setPaymentError] = useState<string>();
  const [flow, setFlow] = useState<CheckoutFlowState<TicketPurchasedPayload>>({
    phase: "awaiting",
  });
  const failureReceived = useRef(false);

  useEffect(() => {
    failureReceived.current = false;
    setSubmitted(false);
    setPaymentError(undefined);
    setFlow({ phase: "awaiting" });
  }, [reference]);

  useEffect(() => {
    if (!reference) return;

    const successHandler = (payload: TicketPurchasedPayload) => {
      if (!paymentOperationReferencesMatch(payload.reference, reference)) return;
      setFlow({ phase: "success", result: payload });
    };
    const failureHandler = (failure: TicketPurchaseFailedPayload) => {
      if (!paymentOperationReferencesMatch(failure.reference, reference)) return;
      failureReceived.current = true;
      setPaymentError(failure.failureMessage ?? "Payment failed.");
      setSubmitted(false);
    };

    notificationConnection.on("TicketPurchased", successHandler);
    notificationConnection.on("TicketPurchaseFailed", failureHandler);
    return () => {
      notificationConnection.off("TicketPurchased", successHandler);
      notificationConnection.off("TicketPurchaseFailed", failureHandler);
    };
  }, [reference]);

  useEffect(() => {
    if (!submitted || flow.phase !== "awaiting") return;
    const timeoutId = setTimeout(() => setFlow({ phase: "timeout" }), 30_000);
    return () => clearTimeout(timeoutId);
  }, [flow.phase, submitted]);

  const paymentConfirmed = () => {
    if (!failureReceived.current) setSubmitted(true);
  };

  const retryPayment = () => {
    failureReceived.current = false;
    setPaymentError(undefined);
    setFlow({ phase: "awaiting" });
  };

  return { flow, submitted, paymentError, paymentConfirmed, retryPayment };
}
