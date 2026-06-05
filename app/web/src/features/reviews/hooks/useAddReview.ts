import { useMutation, useQueryClient } from "@tanstack/react-query";
import { reviewApi } from "@customer/shared/features/reviews";
import { useCanReviewQuery } from "./useCanReviewQuery";

export function useAddReview(concertId: number) {
  const queryClient = useQueryClient();
  const { data: canReview, isLoading } = useCanReviewQuery(
    "concert",
    concertId,
  );

  const mutation = useMutation({
    mutationFn: ({ stars, details }: { stars: number; details?: string }) =>
      reviewApi.createReview({ concertId, stars, details }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["reviews"] });
    },
  });

  return { canReview, isLoading, mutation };
}
