import { customerClient } from "@concertable/customer/lib/customerClient";
import { configureWebClient } from "@concertable/web/lib/configureWebClient";

configureWebClient(customerClient, import.meta.env.VITE_API_URL);

export { customerClient };
