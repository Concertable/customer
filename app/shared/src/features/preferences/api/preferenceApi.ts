import api from "@concertable/shared/lib/axiosClient";
import type { Preference, CreatePreferenceRequest } from "../types";

const preferenceApi = {
  getMyPreference: async (): Promise<Preference> => {
    const { data } = await api.get<Preference>("/preference/user");
    return data;
  },

  createPreference: async (
    request: CreatePreferenceRequest,
  ): Promise<Preference> => {
    const { data } = await api.post<Preference>("/preference", request);
    return data;
  },

  updatePreference: async (
    id: number,
    preference: Preference,
  ): Promise<Preference> => {
    const { data } = await api.put<Preference>(`/preference/${id}`, preference);
    return data;
  },
};

export default preferenceApi;
