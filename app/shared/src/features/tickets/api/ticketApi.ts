import { customerClient } from "../../../lib/customerClient";
import type {
  Ticket,
  TicketCheckout,
  TicketPurchaseRequest,
  TicketPurchase,
} from "../types";

const ticketApi = {
  purchase: async (
    request: TicketPurchaseRequest,
  ): Promise<TicketPurchase> => {
    const { data } = await customerClient.post<TicketPurchase>(
      "/ticket/purchase",
      request,
    );
    return data;
  },
  checkout: async (concertId: number, quantity: number): Promise<TicketCheckout> => {
    const { data } = await customerClient.post<TicketCheckout>("/ticket/checkout", {
      concertId,
      quantity,
    });
    return data;
  },
  getUpcoming: async (): Promise<Ticket[]> => {
    const { data } = await customerClient.get<Ticket[]>("/ticket/upcoming/user");
    return data;
  },
  getHistory: async (): Promise<Ticket[]> => {
    const { data } = await customerClient.get<Ticket[]>("/ticket/history/user");
    return data;
  },
};

export default ticketApi;
