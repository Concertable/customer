export interface TicketPurchasedPayload {
  success: boolean;
  requiresAction: boolean;
  message: string;
  amount: number;
  currency?: string;
  purchaseDate: string;
  transactionId?: string;
  clientSecret?: string;
  userEmail?: string;
  ticketIds: number[];
  concertId: number;
}
