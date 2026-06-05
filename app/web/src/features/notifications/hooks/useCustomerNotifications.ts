import { useCallback, useEffect } from "react";
import { useRouter } from "@tanstack/react-router";
import { toast } from "sonner";
import { useTicketPurchasedHandler } from "@customer/shared/features/notifications";
import { notificationConnection } from "@/lib/signalr";
import type { TicketPurchasedPayload } from "@customer/shared/features/notifications";
import type { ConcertPostedPayload } from "@/features/notifications";

export function useCustomerNotifications() {
  const router = useRouter();

  const onPurchased = useCallback((payload: TicketPurchasedPayload) => {
    const count = payload.ticketIds.length;
    toast.success(
      count > 1 ? `You purchased ${count} tickets` : "You purchased a ticket",
      {
        description: "Click to view your ticket",
        action: {
          label: "View",
          onClick: () => void router.navigate({ to: "/profile/tickets/upcoming" }),
        },
      },
    );
  }, [router]);

  useTicketPurchasedHandler(notificationConnection, onPurchased);

  useEffect(() => {
    notificationConnection.on(
      "ConcertPosted",
      (payload: ConcertPostedPayload) => {
        console.log("[SignalR] ConcertPosted:", payload);
      },
    );

    return () => {
      notificationConnection.off("ConcertPosted");
    };
  }, []);
}
