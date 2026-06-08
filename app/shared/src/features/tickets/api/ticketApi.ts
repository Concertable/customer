import customerApi from "../../../lib/customerAxiosClient";
import type { Ticket, TicketCheckout } from "../types";

interface TicketPurchaseRequest {
  concertId: number;
  quantity: number;
  paymentMethodId: string;
}

export interface TicketPurchaseResponse {
  requiresAction: boolean;
  clientSecret?: string;
  transactionId?: string;
  ticketIds: string[];
  concertId: number;
  amount: number;
  currency?: string;
  purchaseDate: string;
  userEmail?: string;
}

const ticketApi = {
  purchase: async (
    request: TicketPurchaseRequest,
  ): Promise<TicketPurchaseResponse> => {
    const { data } = await customerApi.post<TicketPurchaseResponse>(
      "/ticket/purchase",
      request,
    );
    return data;
  },
  checkout: async (concertId: number, quantity: number): Promise<TicketCheckout> => {
    const { data } = await customerApi.post<TicketCheckout>("/ticket/checkout", {
      concertId,
      quantity,
    });
    return data;
  },
  getUpcoming: async (): Promise<Ticket[]> => {
    const { data } = await customerApi.get<Ticket[]>("/ticket/upcoming/user");
    return data;
  },
  getHistory: async (): Promise<Ticket[]> => {
    const { data } = await customerApi.get<Ticket[]>("/ticket/history/user");
    return data;
  },
};

export default ticketApi;
