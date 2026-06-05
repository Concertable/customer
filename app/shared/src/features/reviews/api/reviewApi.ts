import customerApi from "../../../lib/customerAxiosClient";
import type { Review, ReviewEntityType } from "@concertable/shared/features/reviews";

interface CreateReviewRequest {
  concertId: number;
  stars: number;
  details?: string;
}

const basePath = (type: ReviewEntityType, id: number) =>
  `/${type}s/${id}/reviews`;

const reviewApi = {
  canReview: async (type: ReviewEntityType, id: number): Promise<boolean> => {
    const { data } = await customerApi.get<boolean>(`${basePath(type, id)}/eligibility`);
    return data;
  },

  createReview: async (request: CreateReviewRequest): Promise<Review> => {
    const { concertId, ...body } = request;
    const { data } = await customerApi.post<Review>(`${basePath("concert", concertId)}`, body);
    return data;
  },
};

export default reviewApi;
