import { createFileRoute } from "@tanstack/react-router";
import { useCustomerNotifications } from "../../features/notifications";
import { AppLayout } from "@/components/AppLayout";
import type { ProfileMenuItem } from "@/components/ProfileMenu";

const links = [
  { label: "Home", to: "/" },
  { label: "Find", to: "/find" },
  { label: "For Artists & Venues", href: import.meta.env.VITE_BUSINESS_URL as string },
];

const profileItems: ProfileMenuItem[] = [
  { label: "Profile", to: "/profile" },
  {
    label: "My Tickets",
    items: [
      { label: "Upcoming", to: "/profile/tickets/upcoming" },
      { label: "History", to: "/profile/tickets/history" },
    ],
  },
  { label: "Preferences", to: "/profile/preferences" },
];

function CustomerLayout() {
  useCustomerNotifications();
  return <AppLayout links={links} profileItems={profileItems} />;
}

export const Route = createFileRoute("/_customer")({
  component: CustomerLayout,
});
