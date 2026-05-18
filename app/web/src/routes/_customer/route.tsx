import { createFileRoute } from "@tanstack/react-router";
import { useCustomerNotifications } from "@/features/notifications";
import { AppLayout } from "@/components/AppLayout";

const links = [
  { label: "Home", to: "/" },
  { label: "Find", to: "/find" },
  { label: "For Artists & Venues", href: import.meta.env.VITE_BUSINESS_URL as string },
];

function CustomerLayout() {
  useCustomerNotifications();
  return <AppLayout links={links} />;
}

export const Route = createFileRoute("/_customer")({
  component: CustomerLayout,
});
