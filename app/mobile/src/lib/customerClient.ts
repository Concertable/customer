import { customerClient } from "@concertable/customer-shared/lib/customerClient";
import Config from "shared/lib/config";
import { configureMobileClient } from "shared/lib/configureMobileClient";

configureMobileClient(customerClient, `${Config.customerApiUrl}/api`);

export { customerClient };
