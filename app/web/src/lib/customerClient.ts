import { customerClient } from "@concertable/customer/shared/lib/customerClient";
import { configureWebClient } from "@concertable/web/shared/lib/configureWebClient";

configureWebClient(customerClient, import.meta.env.VITE_CUSTOMER_API_URL);

export { customerClient };
