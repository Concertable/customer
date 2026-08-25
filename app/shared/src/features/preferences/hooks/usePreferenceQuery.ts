import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import preferenceApi from "../api/preferenceApi";
import type { PreferenceRequest } from "../types";

export function useMyPreferenceQuery() {
  return useQuery({
    queryKey: ["preference", "my"],
    queryFn: preferenceApi.getMyPreference,
  });
}

export function useUpdateMyPreferenceMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: PreferenceRequest }) =>
      preferenceApi.updatePreference(id, data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["preference", "my"] }),
  });
}

export function useCreateMyPreferenceMutation() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: PreferenceRequest) =>
      preferenceApi.createPreference(request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["preference", "my"] }),
  });
}
