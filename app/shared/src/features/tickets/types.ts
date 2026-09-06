export interface TicketPurchaseRequest {
  concertId: number;
  quantity: number;
}

export interface PaymentOperationReference {
  operationType: string;
  clientReference: string;
}

export interface CheckoutSession {
  clientSecret: string;
  customerSession?: string;
  customerId?: string;
}

export interface TicketPurchase {
  reference: PaymentOperationReference;
  clientSecret?: string;
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
  reference: PaymentOperationReference;
  session: CheckoutSession;
  price: number;
  concertId: number;
  quantity: number;
}
