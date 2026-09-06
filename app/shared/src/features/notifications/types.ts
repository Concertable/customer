import type {
  PaymentOperationReference,
  TicketPurchase,
} from "../tickets/types";

export type TicketPurchasedPayload = TicketPurchase;

export interface TicketPurchaseFailedPayload {
  reference: PaymentOperationReference;
  failureCode?: string;
  failureMessage?: string;
}
