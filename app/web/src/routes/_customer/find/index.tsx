import { createFileRoute } from "@tanstack/react-router";
import { CustomerFindPage, SearchSchema } from "@/features/search";

export const Route = createFileRoute("/_customer/find/")({
  component: CustomerFindPage,
  validateSearch: SearchSchema(),
});
