import { Navigate } from "@tanstack/react-router";

export function TicketsPage() {
  return <Navigate to="/profile/tickets/upcoming" replace />;
}
