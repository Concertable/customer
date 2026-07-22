export interface CreateReviewRequest {
  concertId: number;
  stars: number;
  details?: string;
}
