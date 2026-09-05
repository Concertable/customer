import type { PaymentOperationReference } from "./types";

export function paymentOperationReferencesMatch(
  left: PaymentOperationReference,
  right: PaymentOperationReference,
): boolean {
  return (
    left.operationType === right.operationType &&
    left.clientReference === right.clientReference
  );
}
