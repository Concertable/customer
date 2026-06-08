import { useQuery } from "@tanstack/react-query";
import { useAuth } from "react-oidc-context";
import { reviewApi } from "@customer/shared/features/reviews";
import type { ReviewEntityType } from "@/features/reviews";

export function useCanReviewQuery(type: ReviewEntityType, id: number) {
  const { isAuthenticated } = useAuth();
  return useQuery({
    queryKey: ["reviews", type, id, "can-review"],
    queryFn: () => reviewApi.canReview(type, id),
    enabled: isAuthenticated && !!id,
  });
}
