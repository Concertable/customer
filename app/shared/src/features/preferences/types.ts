import type { Genre } from "@concertable/shared/types";
import type { User } from "@concertable/shared/features/auth";

export interface Preference {
  id: number;
  user: User;
  radiusKm: number;
  genres: Genre[];
}

export interface CreatePreferenceRequest {
  radiusKm: number;
  genres: Genre[];
}
