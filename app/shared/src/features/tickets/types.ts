import type { CheckoutSession } from "@concertable/shared/features/concerts/types";
import type { PaymentOutcome } from "@concertable/shared/features/payments/types";

export interface TicketPurchaseRequest {
  concertId: number;
  quantity: number;
  paymentMethodId: string;
}

export interface TicketPurchase extends PaymentOutcome {
  ticketIds: string[];
  concertId: number;
  amount: number;
  currency?: string;
  purchaseDate: string;
  userEmail?: string;
}

export interface TicketConcert {
  id: number;
  name: string;
  price: number;
  startDate: string;
  endDate: string;
  venueName: string;
  artistName: string;
}

export interface Ticket {
  id: string;
  purchaseDate: string;
  qrCode: string;
  userId: string;
  concert: TicketConcert;
}

export interface TicketCheckout {
  session: CheckoutSession;
  price: number;
  concertId: number;
  quantity: number;
}
