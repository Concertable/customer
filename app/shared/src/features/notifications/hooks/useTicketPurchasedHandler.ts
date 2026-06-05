import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";
import type { HubConnection } from "@microsoft/signalr";
import type { TicketPurchasedPayload } from "../types";

export function useTicketPurchasedHandler(
  connection: HubConnection,
  onPurchased?: (payload: TicketPurchasedPayload) => void,
) {
  const queryClient = useQueryClient();

  useEffect(() => {
    console.log("[useTicketPurchasedHandler] effect — subscribing", {
      connectionState: connection.state,
    });
    const handler = (payload: TicketPurchasedPayload) => {
      console.log("[useTicketPurchasedHandler] event fired", payload);
      void queryClient.invalidateQueries({ queryKey: ["tickets", "upcoming"] });
      void queryClient.invalidateQueries({ queryKey: ["tickets", "history"] });
      onPurchased?.(payload);
    };

    connection.on("TicketPurchased", handler);
    return () => {
      console.log("[useTicketPurchasedHandler] cleanup — unsubscribing");
      connection.off("TicketPurchased", handler);
    };
  }, [queryClient, connection, onPurchased]);
}
