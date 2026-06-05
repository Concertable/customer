import { useTicketPurchasedHandler } from "@customer/shared/features/notifications";
import type { TicketPurchasedPayload } from "@customer/shared/features/notifications";
import { notificationConnection } from "shared/lib/signalr";
import { logger } from "shared/lib/logger";
import { notify } from "shared/lib/toast";

export function useCustomerNotifications() {
  logger.log("[useCustomerNotifications] mount", {
    connectionState: notificationConnection.state,
  });
  useTicketPurchasedHandler(notificationConnection, onPurchased);
}

function onPurchased(payload: TicketPurchasedPayload) {
  const count = payload.ticketIds.length;
  logger.log("[useCustomerNotifications] onPurchased fired", { ticketCount: count });
  notify(
    count > 1 ? `You purchased ${count} tickets` : "You purchased a ticket",
    "success",
  );
}
