import { Skeleton } from "@/components/ui/skeleton";
import { useTicketHistoryQuery } from "@customer/shared/features/tickets";
import { TicketCard } from "../components/tickets/TicketCard";

export function TicketHistoryPage() {
  const { data, isLoading, isError } = useTicketHistoryQuery();

  return (
    <div className="max-w-2xl space-y-6">
      <div>
        <h2 className="text-lg font-semibold">Ticket History</h2>
        <p className="text-muted-foreground text-sm">
          Tickets for concerts you have already attended
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
          No past tickets to show.
        </p>
      ) : (
        <div className="space-y-4">
          {data.map((t) => (
            <TicketCard key={t.id} ticket={t} />
          ))}
        </div>
      )}
    </div>
  );
}
