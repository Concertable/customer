import { customerClient } from "../../../lib/customerClient";
import type { Review, ReviewEntityType } from "@concertable/shared/features/reviews";
import type { CreateReviewRequest } from "../types";

const basePath = (type: ReviewEntityType, id: number) =>
  `/${type}s/${id}/reviews`;

const reviewApi = {
  canReview: async (type: ReviewEntityType, id: number): Promise<boolean> => {
    const { data } = await customerClient.get<boolean>(`${basePath(type, id)}/eligibility`);
    return data;
  },

  createReview: async (request: CreateReviewRequest): Promise<Review> => {
    const { concertId, ...body } = request;
    const { data } = await customerClient.post<Review>(`${basePath("concert", concertId)}`, body);
    return data;
  },
};

export default reviewApi;
