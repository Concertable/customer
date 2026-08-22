import { beforeEach, describe, expect, it, vi } from "vitest";
import reviewApi from "./reviewApi";

const mocks = vi.hoisted(() => ({ post: vi.fn() }));

vi.mock("../../../lib/customerClient", () => ({
  customerClient: { post: mocks.post },
}));

describe("reviewApi", () => {
  beforeEach(() => vi.clearAllMocks());

  it("keeps the concert id in the route and out of the request body", async () => {
    const review = { id: 7 };
    const request = { stars: 5, details: "Excellent" };
    mocks.post.mockResolvedValue({ data: review });

    await expect(reviewApi.createReview(42, request)).resolves.toBe(review);
    expect(mocks.post).toHaveBeenCalledWith("/concerts/42/reviews", request);
  });
});
