import { createFileRoute } from "@tanstack/react-router";
import { CustomerHomePage } from "@/features/search";

export const Route = createFileRoute("/_customer/")({
  component: CustomerHomePage,
});
