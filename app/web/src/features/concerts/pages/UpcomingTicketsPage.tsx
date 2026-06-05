import { Skeleton } from "@/components/ui/skeleton";
import { useUpcomingTicketsQuery } from "@customer/shared/features/tickets";
import { TicketCard } from "../components/tickets/TicketCard";

export function UpcomingTicketsPage() {
  const { data, isLoading, isError } = useUpcomingTicketsQuery();

  return (
    <div className="max-w-2xl space-y-6">
      <div>
        <h2 className="text-lg font-semibold">Upcoming Tickets</h2>
        <p className="text-muted-foreground text-sm">
          Tickets for concerts you have yet to attend
        </p>
      </div>

      {isLoading ? (
        <div className="space-y-4">
          <Skeleton className="h-44 w-full rounded-lg" />
          <Skeleton className="h-44 w-full rounded-lg" />
        </div>
      ) : isError ? (
        <p className="text-destructive text-sm">Failed to load tickets.</p>
      ) : !data?.length ? (
        <p className="text-muted-foreground text-sm">
          You don't have any upcoming tickets yet.
        </p>
      ) : (
        <div className="space-y-4" data-testid="upcoming-tickets-list">
          {data.map((t) => (
            <TicketCard key={t.id} ticket={t} />
          ))}
        </div>
      )}
    </div>
  );
}
