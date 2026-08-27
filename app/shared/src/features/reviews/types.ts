import type { Review } from "@concertable/shared/features/reviews/types";

export type CreateReviewRequest = Pick<Review, "stars" | "details">;
