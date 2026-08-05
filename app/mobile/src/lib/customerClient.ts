import { customerClient } from "@concertable/customer/shared/lib/customerClient";
import Config from "@concertable/mobile/shared/lib/config";
import { configureMobileClient } from "@concertable/mobile/shared/lib/configureMobileClient";

configureMobileClient(customerClient, `${Config.customerApiUrl}/api`);

export { customerClient };
