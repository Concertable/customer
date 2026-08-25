import { apiClient } from "@concertable/shared/lib/apiClient";
import type { Preference, PreferenceRequest } from "../types";

const preferenceApi = {
  getMyPreference: async (): Promise<Preference> => {
    const { data } = await apiClient.get<Preference>("/preference/user");
    return data;
  },

  createPreference: async (
    request: PreferenceRequest,
  ): Promise<Preference> => {
    const { data } = await apiClient.post<Preference>("/preference", request);
    return data;
  },

  updatePreference: async (
    id: number,
    request: PreferenceRequest | Preference,
  ): Promise<Preference> => {
    const body: PreferenceRequest = {
      radiusKm: request.radiusKm,
      genres: request.genres,
    };
    const { data } = await apiClient.put<Preference>(
      `/preference/${id}`,
      body,
    );
    return data;
  },
};

export default preferenceApi;
