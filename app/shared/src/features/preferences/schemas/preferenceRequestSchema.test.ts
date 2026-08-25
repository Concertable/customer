import { describe, expect, it } from "vitest";
import { preferenceRequestSchema } from "./preferenceRequestSchema";

describe("preferenceRequestSchema", () => {
  it("preserves every selected genre", () => {
    expect(
      preferenceRequestSchema.parse({
        radiusKm: 25,
        genres: ["rock", "jazz"],
      }),
    ).toEqual({ radiusKm: 25, genres: ["rock", "jazz"] });
  });
});
