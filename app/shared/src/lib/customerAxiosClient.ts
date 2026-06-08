import axios from "axios";
import qs from "qs";

const customerApi = axios.create({
  paramsSerializer: (params) =>
    qs.stringify(params, {
      arrayFormat: "comma",
      encode: false,
      skipNulls: true,
    }),
});

export function configureCustomerApi(baseURL: string) {
  customerApi.defaults.baseURL = baseURL;
}

export default customerApi;
