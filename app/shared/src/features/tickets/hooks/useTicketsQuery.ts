import { useQuery } from "@tanstack/react-query";
import ticketApi from "../api/ticketApi";

export function useUpcomingTicketsQuery() {
  return useQuery({
    queryKey: ["tickets", "upcoming"],
    queryFn: ticketApi.getUpcoming,
  });
}

export function useTicketHistoryQuery() {
  return useQuery({
    queryKey: ["tickets", "history"],
    queryFn: ticketApi.getHistory,
  });
}

export function useTicketCheckoutQuery(concertId: number, quantity: number) {
  return useQuery({
    queryKey: ["tickets", "checkout", concertId, quantity],
    queryFn: () => ticketApi.checkout(concertId, quantity),
    staleTime: Infinity,
  });
}
