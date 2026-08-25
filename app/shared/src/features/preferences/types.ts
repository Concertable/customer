import type { Genre } from "@concertable/shared/types";
import type { User } from "@concertable/shared/features/auth";

export interface Preference {
  id: number;
  userId?: string;
  user?: User;
  radiusKm: number;
  genres: Genre[];
}

export type PreferenceRequest = Omit<
  Preference,
  "id" | "userId" | "user"
>;
export type CreatePreferenceRequest = PreferenceRequest;
