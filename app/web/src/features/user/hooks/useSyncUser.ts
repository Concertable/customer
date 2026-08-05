import { useSyncUser as useSyncSharedUser } from "@concertable/web/features/user";
import { customerClient } from "@concertable/customer/lib/customerClient";
import type { User } from "@concertable/web/features/auth/types";

async function getMe(): Promise<User> {
  const { data } = await customerClient.get<User>("/user/me");
  return data;
}

export function useSyncUser() {
  useSyncSharedUser(getMe);
}
