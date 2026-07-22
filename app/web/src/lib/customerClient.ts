import { customerClient } from "@customer/shared/lib/customerClient";
import { configureWebClient } from "shared/lib/configureWebClient";

configureWebClient(customerClient, import.meta.env.VITE_CUSTOMER_API_URL);

export { customerClient };
