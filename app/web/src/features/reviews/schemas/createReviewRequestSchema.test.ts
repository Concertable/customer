import { describe, expect, it } from "vitest";
import { createReviewRequestSchema } from "./createReviewRequestSchema";

describe("createReviewRequestSchema", () => {
  it("trims details and omits an empty value", () => {
    expect(
      createReviewRequestSchema.parse({ stars: 5, details: "  Excellent  " }),
    ).toEqual({ stars: 5, details: "Excellent" });
    expect(
      createReviewRequestSchema.parse({ stars: 5, details: "   " }),
    ).toEqual({ stars: 5, details: undefined });
  });
});
