import { customerClient } from "../../../lib/customerClient";
import type { Review, ReviewEntityType } from "@concertable/shared/features/reviews/types";
import type { CreateReviewRequest } from "../types";

const basePath = (type: ReviewEntityType, id: number) =>
  `/${type}s/${id}/reviews`;

type LegacyCreateReviewRequest = CreateReviewRequest & {
  concertId: number;
};

async function createReview(
  request: LegacyCreateReviewRequest,
): Promise<Review>;
async function createReview(
  concertId: number,
  request: CreateReviewRequest,
): Promise<Review>;
async function createReview(
  concertIdOrRequest: number | LegacyCreateReviewRequest,
  request?: CreateReviewRequest,
): Promise<Review> {
  const concertId =
    typeof concertIdOrRequest === "number"
      ? concertIdOrRequest
      : concertIdOrRequest.concertId;
  const source =
    typeof concertIdOrRequest === "number" ? request : concertIdOrRequest;
  if (!source) throw new TypeError("Review request is required");
  const body: CreateReviewRequest = {
    stars: source.stars,
    details: source.details,
  };
  const { data } = await customerClient.post<Review>(
    basePath("concert", concertId),
    body,
  );
  return data;
}

const reviewApi = {
  canReview: async (type: ReviewEntityType, id: number): Promise<boolean> => {
    const { data } = await customerClient.get<boolean>(`${basePath(type, id)}/eligibility`);
    return data;
  },

  createReview,
};

export default reviewApi;
