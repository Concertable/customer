import type { CheckoutSession } from "@concertable/shared/features/concerts";
import type { PaymentOutcome } from "@concertable/shared/features/payments";

export interface TicketPurchaseRequest {
  concertId: number;
  quantity: number;
  paymentMethodId: string;
}

export interface TicketPurchaseResponse extends PaymentOutcome {
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
  userEmail: string;
  concert: TicketConcert;
}

export interface TicketCheckout {
  session: CheckoutSession;
  price: number;
  concertId: number;
  quantity: number;
}
