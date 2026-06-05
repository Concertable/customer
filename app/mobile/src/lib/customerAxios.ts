import customerApi, { configureCustomerApi } from "@customer/shared/lib/customerAxiosClient";
import { useAuthStore } from "@concertable/shared/features/auth";
import { tokenStorage } from "shared/auth/tokenStorage";
import Config from "shared/lib/config";

configureCustomerApi(`${Config.customerApiUrl}/api`);

customerApi.interceptors.request.use(async (config) => {
  const token = await tokenStorage.getAccessToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

customerApi.interceptors.response.use(
  (r) => r,
  async (error) => {
    if (error.response?.status === 401) {
      await tokenStorage.clear();
      useAuthStore.getState().setUser(null);
    }
    return Promise.reject(error);
  },
);

export default customerApi;
