import { GENRE_VALUES } from "@concertable/shared/types";
import { z } from "zod";
import type { PreferenceRequest } from "../types";

export const preferenceRequestSchema = z.object({
  radiusKm: z.number().min(1, "Choose a search radius"),
  genres: z.array(z.enum(GENRE_VALUES)),
}) satisfies z.ZodType<PreferenceRequest>;
