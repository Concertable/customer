import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { reviewApi } from "@customer/shared/features/reviews";
import {
  createReviewRequestSchema,
  type CreateReviewRequest,
} from "../schemas/createReviewRequestSchema";
import { useCanReviewQuery } from "./useCanReviewQuery";

export interface ReviewBuffer {
  stars: number;
  details: string;
}

export function useAddReview(concertId: number) {
  const queryClient = useQueryClient();
  const { data: canReview, isLoading } = useCanReviewQuery(
    "concert",
    concertId,
  );

  const mutation = useMutation({
    mutationFn: (request: CreateReviewRequest) =>
      reviewApi.createReview({ concertId, ...request }),
    onSuccess: () => {
      toast.success("Review submitted");
      queryClient.invalidateQueries({ queryKey: ["reviews"] });
    },
  });

  const submit = (buffer: ReviewBuffer, onDone: () => void) => {
    const parsed = createReviewRequestSchema.safeParse({
      stars: buffer.stars,
      details: buffer.details || undefined,
    });
    if (parsed.success) mutation.mutate(parsed.data, { onSuccess: onDone });
    return parsed;
  };

  return { canReview, isLoading, submit, isPending: mutation.isPending };
}
