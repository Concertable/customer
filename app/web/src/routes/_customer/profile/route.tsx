import { createFileRoute, Outlet, Link } from "@tanstack/react-router";
import { requireRole } from "@/features/auth";
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarInset,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarProvider,
} from "@/components/ui/sidebar";

const links = [
  { label: "Profile", to: "/profile" },
  { label: "Location", to: "/profile/location" },
  { label: "Preferences", to: "/profile/preferences" },
  { label: "Upcoming Tickets", to: "/profile/tickets/upcoming" },
  { label: "Ticket History", to: "/profile/tickets/history" },
];

function ProfileLayout() {
  return (
    <SidebarProvider
      style={{ "--sidebar-width": "220px" } as React.CSSProperties}
      className="min-h-screen"
    >
      <Sidebar collapsible="none" className="bg-muted min-h-screen">
        <SidebarContent>
          <SidebarGroup>
            <SidebarGroupLabel>Profile</SidebarGroupLabel>
            <SidebarGroupContent>
              <SidebarMenu>
                {links.map(({ label, to }) => (
                  <SidebarMenuItem key={to}>
                    <SidebarMenuButton asChild>
                      <Link
                        to={to}
                        activeOptions={{ exact: to === "/profile" }}
                        activeProps={{
                          "data-active": true,
                          className:
                            "bg-primary! text-primary-foreground! hover:bg-primary! hover:text-primary-foreground!",
                        }}
                      >
                        {label}
                      </Link>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                ))}
              </SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>
        </SidebarContent>
      </Sidebar>

      <SidebarInset>
        <div className="p-6">
          <Outlet />
        </div>
      </SidebarInset>
    </SidebarProvider>
  );
}

export const Route = createFileRoute("/_customer/profile")({
  beforeLoad: ({ location }) => requireRole("Customer", { location }),
  component: ProfileLayout,
});
