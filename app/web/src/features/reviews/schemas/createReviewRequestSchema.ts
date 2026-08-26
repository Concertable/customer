import type { CreateReviewRequest } from "@concertable/customer/features/reviews/types";
import { z } from "zod";

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
    .transform((details) => details || undefined)
    .optional(),
}) satisfies z.ZodType<CreateReviewRequest>;

export type CreateReviewFormValues = z.input<
  typeof createReviewRequestSchema
>;
