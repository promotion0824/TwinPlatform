import { lazy } from "react";

// route-based code splitting to reduce bundle sizes and load faster
const OverviewPage = lazy(() => import("./pages/OverviewPage/OverviewPage"));
const CommandsPage = lazy(() => import("./pages/CommandsPage/CommandsPage"));
const RequestsPage = lazy(() => import("./pages/RequestsPage/RequestsPage"));
const ActivityLogsPage = lazy(() => import("./pages/ActivityLogsPage/ActivityLogsPage"));

export const routes = [
  { path: "/", element: <OverviewPage /> },
  { path: "/requests", element: <RequestsPage /> },
  { path: "/requests/:connectorId/:twinId", element: <RequestsPage /> },
  { path: "/commands", element: <CommandsPage /> },
  { path: "/commands/:id", element: <CommandsPage /> },
  { path: "/activity-logs", element: <ActivityLogsPage />}
];
