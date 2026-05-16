import { createFileRoute } from "@tanstack/react-router";
import { CreatePreferencePage } from "@/features/customer";

export const Route = createFileRoute("/_customer/profile/preferences/create")({
  component: CreatePreferencePage,
});
