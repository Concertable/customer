import { createFileRoute } from "@tanstack/react-router";
import { CustomerFindPage } from "../../../features/search";
import { SearchSchema } from "@/features/search";

export const Route = createFileRoute("/_customer/find/")({
  component: CustomerFindPage,
  validateSearch: SearchSchema(),
});
