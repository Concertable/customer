import type { Genre } from "@concertable/shared/types";

export interface Preference {
  id: number;
  userId: string;
  radiusKm: number;
  genres: Genre[];
}

export type PreferenceRequest = Omit<Preference, "id" | "userId">;
