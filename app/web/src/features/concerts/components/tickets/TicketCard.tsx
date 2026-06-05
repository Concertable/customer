import { CalendarDays, MapPin, Music } from "lucide-react";
import { Separator } from "@/components/ui/separator";
import type { Ticket } from "@customer/shared/features/tickets";
import { QrPopover } from "./QrPopover";

interface Props {
  ticket: Ticket;
}

export function TicketCard({ ticket }: Readonly<Props>) {
  const { concert, qrCode, purchaseDate } = ticket;
  const start = new Date(concert.startDate);

  return (
    <div className="flex flex-col gap-4 rounded-lg border p-4 sm:flex-row" data-testid="ticket-card">
      <div className="flex-1 space-y-2">
        <h3 className="font-medium">{concert.name}</h3>
        <div className="text-muted-foreground space-y-1 text-sm">
          <div className="flex items-center gap-2">
            <Music className="size-4" />
            {concert.artistName}
          </div>
          <div className="flex items-center gap-2">
            <MapPin className="size-4" />
            {concert.venueName}
          </div>
          <div className="flex items-center gap-2">
            <CalendarDays className="size-4" />
            {start.toLocaleString()}
          </div>
        </div>
        <Separator />
        <div className="text-muted-foreground text-xs">
          Purchased {new Date(purchaseDate).toLocaleDateString()} · £
          {concert.price.toFixed(2)}
        </div>
      </div>
      <QrPopover qrCode={qrCode} alt={`QR code for ticket ${ticket.id}`} />
    </div>
  );
}
