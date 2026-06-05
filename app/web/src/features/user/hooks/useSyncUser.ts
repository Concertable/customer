import { useSyncUser as useSyncSharedUser } from "@/features/user";
import customerApi from "@customer/shared/lib/customerAxiosClient";
import type { User } from "@/features/auth/types";

async function getMe(): Promise<User> {
  const { data } = await customerApi.get<User>("/user/me");
  return data;
}

export function useSyncUser() {
  useSyncSharedUser(getMe);
}
