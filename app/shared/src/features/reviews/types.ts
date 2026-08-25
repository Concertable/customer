import type { Review } from "@concertable/shared/features/reviews";

export type CreateReviewRequest = Pick<Review, "stars" | "details"> & {
  concertId?: number;
};
