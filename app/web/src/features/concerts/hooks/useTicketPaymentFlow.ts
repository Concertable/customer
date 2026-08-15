import { useCallback, useRef, useState } from "react";
import type {
  TicketPurchaseFailedPayload,
  TicketPurchasedPayload,
} from "@concertable/customer/features/notifications";
import { useCheckoutFlow } from "@concertable/web/features/concerts/hooks/useCheckoutFlow";

export function useTicketPaymentFlow(clientSecret?: string) {
  const [submitted, setSubmitted] = useState(false);
  const [paymentError, setPaymentError] = useState<string>();
  const [attempt, setAttempt] = useState(0);
  const failureReceived = useRef(false);
  const transactionId = clientSecret?.split("_secret_")[0];
  const matchesSuccess = useCallback(
    (payload: TicketPurchasedPayload) => payload.transactionId === transactionId,
    [transactionId],
  );
  const matchesFailure = useCallback(
    (failure: TicketPurchaseFailedPayload) =>
      failure.transactionId === transactionId,
    [transactionId],
  );
  const handleFailure = useCallback((failure: TicketPurchaseFailedPayload) => {
    failureReceived.current = true;
    setPaymentError(failure.failureMessage);
    setSubmitted(false);
  }, []);
  const flow = useCheckoutFlow<
    TicketPurchasedPayload,
    TicketPurchaseFailedPayload
  >({
    event: "TicketPurchased",
    failureEvent: "TicketPurchaseFailed",
    active: transactionId !== undefined,
    timeoutActive: submitted,
    matchesSuccess,
    matchesFailure,
    onFailure: handleFailure,
    resetKey: `${transactionId}:${attempt}`,
  });

  const paymentStarted = () => {
    failureReceived.current = false;
    setPaymentError(undefined);
    setAttempt((current) => current + 1);
  };

  const paymentConfirmed = () => {
    if (failureReceived.current) return;
    setSubmitted(true);
  };

  return {
    flow,
    submitted,
    paymentError,
    paymentStarted,
    paymentConfirmed,
  };
}
