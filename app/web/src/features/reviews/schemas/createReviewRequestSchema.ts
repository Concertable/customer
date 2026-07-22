import { z } from "zod";

// Bounds mirror the backend CreateReviewRequestValidator — keep them in sync.
export const createReviewRequestSchema = z.object({
  stars: z
    .number()
    .int()
    .min(1, "Please select a star rating")
    .max(5, "Rating must be between 1 and 5 stars"),
  details: z
    .string()
    .trim()
    .max(1000, "Review must be 1000 characters or fewer")
    .optional(),
});

export type CreateReviewRequest = z.infer<typeof createReviewRequestSchema>;
