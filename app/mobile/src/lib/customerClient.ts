import { customerClient } from "@concertable/customer/lib/customerClient";
import Config from "@concertable/mobile/lib/config";
import { configureMobileClient } from "@concertable/mobile/lib/configureMobileClient";

configureMobileClient(customerClient, `${Config.customerApiUrl}/api`);

export { customerClient };
