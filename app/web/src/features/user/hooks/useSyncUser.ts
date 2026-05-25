import { useEffect } from "react";
import { useAuth } from "react-oidc-context";
import { useQuery } from "@tanstack/react-query";
import { useAuthStore } from "@/features/auth";
import customerApi from "@concertable/shared/lib/customerAxiosClient";
import type { User } from "@/features/auth/types";

async function getMe(): Promise<User> {
  const { data } = await customerApi.get<User>("/user/me");
  return data;
}

export function useSyncUser() {
  const { isAuthenticated, isLoading } = useAuth();
  const setUser = useAuthStore((s) => s.setUser);

  const { data, isError } = useQuery({
    queryKey: ["auth", "me"] as const,
    queryFn: getMe,
    enabled: !isLoading && isAuthenticated,
    meta: { expectedErrors: [404] },
  });

  useEffect(() => {
    if (isLoading) return;
    if (!isAuthenticated) {
      setUser(null);
      return;
    }
    if (isError) setUser(null);
    else if (data) setUser(data);
  }, [isAuthenticated, isLoading, data, isError, setUser]);
}
