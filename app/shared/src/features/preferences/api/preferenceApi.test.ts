import { beforeEach, describe, expect, it, vi } from "vitest";
import preferenceApi from "./preferenceApi";

const mocks = vi.hoisted(() => ({
  post: vi.fn(),
  put: vi.fn(),
}));

vi.mock("@concertable/shared/lib/apiClient", () => ({
  apiClient: {
    post: mocks.post,
    put: mocks.put,
  },
}));

describe("preferenceApi", () => {
  beforeEach(() => vi.clearAllMocks());

  it("uses the same slim request for create and update", async () => {
    const request = { radiusKm: 25, genres: ["rock", "jazz"] as const };
    mocks.post.mockResolvedValue({ data: { id: 1 } });
    mocks.put.mockResolvedValue({ data: { id: 1 } });

    await preferenceApi.createPreference(request);
    await preferenceApi.updatePreference(1, request);

    expect(mocks.post).toHaveBeenCalledWith("/preference", request);
    expect(mocks.put).toHaveBeenCalledWith("/preference/1", request);
  });
});
