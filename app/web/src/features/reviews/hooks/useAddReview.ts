import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  reviewApi,
  type CreateReviewRequest,
} from "@concertable/customer/features/reviews";
import { useCanReviewQuery } from "./useCanReviewQuery";

export function useAddReview(concertId: number) {
  const queryClient = useQueryClient();
  const { data: canReview, isLoading } = useCanReviewQuery(
    "concert",
    concertId,
  );

  const mutation = useMutation({
    mutationFn: (request: CreateReviewRequest) =>
      reviewApi.createReview(concertId, request),
    onSuccess: () => {
      toast.success("Review submitted");
      queryClient.invalidateQueries({ queryKey: ["reviews"] });
    },
  });

  const submit = (request: CreateReviewRequest, onDone: () => void) =>
    mutation.mutate(request, { onSuccess: onDone });

  return { canReview, isLoading, submit, isPending: mutation.isPending };
}
