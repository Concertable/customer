import { useSyncUser as useSyncSharedUser } from "@/features/user";
import { customerClient } from "@concertable/customer/shared/lib/customerClient";
import type { User } from "@/features/auth/types";

async function getMe(): Promise<User> {
  const { data } = await customerClient.get<User>("/user/me");
  return data;
}

export function useSyncUser() {
  useSyncSharedUser(getMe);
}
