import { useEffect, useRef, useState } from "react";
import type { TicketPurchasedPayload } from "@concertable/customer/features/notifications";
import type { CheckoutFlowState } from "@concertable/web/features/concerts/hooks/useCheckoutFlow";
import { notificationConnection } from "@concertable/web/lib/signalr";

interface TicketPurchaseFailedPayload {
  transactionId: string;
  failureMessage?: string;
}

export function useTicketPaymentFlow(clientSecret?: string) {
  const [submitted, setSubmitted] = useState(false);
  const [paymentError, setPaymentError] = useState<string>();
  const [flow, setFlow] = useState<CheckoutFlowState<TicketPurchasedPayload>>({
    phase: "awaiting",
  });
  const failureReceived = useRef(false);
  const transactionId = clientSecret?.split("_secret_")[0];

  useEffect(() => {
    failureReceived.current = false;
    setSubmitted(false);
    setPaymentError(undefined);
    setFlow({ phase: "awaiting" });
  }, [transactionId]);

  useEffect(() => {
    if (!transactionId) return;

    const successHandler = (payload: TicketPurchasedPayload) => {
      if (payload.transactionId !== transactionId) return;
      setFlow({ phase: "success", result: payload });
    };
    const failureHandler = (failure: TicketPurchaseFailedPayload) => {
      if (failure.transactionId !== transactionId) return;
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
  }, [transactionId]);

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
