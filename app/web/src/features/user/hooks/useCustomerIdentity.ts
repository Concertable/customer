import { customerClient } from "@concertable/customer/lib/customerClient";
import { useMeQuery } from "@concertable/web/features/user";
import type { User } from "@concertable/web/features/auth/types";

async function getMe(): Promise<User> {
  const { data } = await customerClient.get<User>("/user/me");
  return data;
}

export function useCustomerIdentity() {
  return useMeQuery(getMe);
}
