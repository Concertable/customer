import { createFileRoute, useNavigate } from "@tanstack/react-router";
import { ConcertDetailsPage } from "@/features/concerts";
import { AddReview } from "../../../features/reviews";

export const Route = createFileRoute("/_customer/find/concert/$id")({
  params: {
    parse: (params) => ({ id: Number(params.id) }),
    stringify: (params) => ({ id: String(params.id) }),
  },
  component: function () {
    const { id } = Route.useParams();
    const navigate = useNavigate();
    return (
      <ConcertDetailsPage
        id={id}
        addReviewSlot={<AddReview concertId={id} />}
        onBuyTickets={() =>
          void navigate({ to: "/concert/checkout/$id", params: { id } })
        }
      />
    );
  },
});
