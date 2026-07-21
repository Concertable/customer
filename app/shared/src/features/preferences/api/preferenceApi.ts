import { apiClient } from "@concertable/shared/lib/apiClient";
import type { Preference, CreatePreferenceRequest } from "../types";

const preferenceApi = {
  getMyPreference: async (): Promise<Preference> => {
    const { data } = await apiClient.get<Preference>("/preference/user");
    return data;
  },

  createPreference: async (
    request: CreatePreferenceRequest,
  ): Promise<Preference> => {
    const { data } = await apiClient.post<Preference>("/preference", request);
    return data;
  },

  updatePreference: async (
    id: number,
    preference: Preference,
  ): Promise<Preference> => {
    const { data } = await apiClient.put<Preference>(`/preference/${id}`, preference);
    return data;
  },
};

export default preferenceApi;
