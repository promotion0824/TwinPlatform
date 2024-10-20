import { Tabs, Badge } from "@willowinc/ui";
import { useLocation, Link } from "react-router-dom";
import styled from "@emotion/styled";
import { useAppContext } from "../../providers/AppContextProvider";
import { useMemo } from "react";
import useAuthorization from "../../hooks/useAuthorization";
import { useRequestsCountContext } from "../../providers/RequestsCountProvider";

export default function NavMenu() {
  const location = useLocation();
  const { requestsCountState } = useRequestsCountContext();

  const {
    hasCanViewRequestsCommandsPermission,
    hasCanApproveExecutePermission,
    isLoading: isAuthorizationLoading,
  } = useAuthorization();

  const hasPermission =
    hasCanViewRequestsCommandsPermission || hasCanApproveExecutePermission || isAuthorizationLoading;

  const routePages: RoutePages = useMemo(
    () => ({
      Overview: {
        path: "/",
        disabled: !hasPermission,
      },
      Requests: {
        path: "/requests",
        showRecordCount: true,
        disabled: !hasPermission,
      },
      Commands: {
        path: "/commands",
        disabled: !hasPermission,
      },
      "Activity Logs": {
        path: "/activity-logs",
        disabled: !hasPermission,
      },
    }),
    [hasPermission]
  );

  const invertedRoutePages = useMemo(
    () => invertRoutePages(routePages),
    [routePages]
  );

  const path = "/" + location.pathname.split("/")[1];

  const hideNavTabs = location.pathname.split("/").length > 2;

  return (
    <>
      {!hideNavTabs && (
        <Tabs value={invertedRoutePages[path]}>
          <StyledTabsList>
            {Object.entries(routePages).map(([key, value]) => (
              <div key={key}>
                <Link to={value.path}>
                  <StyledTabsTab value={key} disabled={value.disabled}>
                    <Container>
                      {key}
                      {value.showRecordCount && (
                        <Badge>{requestsCountState[0]}</Badge>
                      )}
                    </Container>
                  </StyledTabsTab>
                </Link>
              </div>
            ))}
          </StyledTabsList>
        </Tabs>
      )}
    </>
  );
}

/**
 * Return a map of path to page name
 * key is the route, value is page name
 */
function invertRoutePages(routePages: RoutePages) {
  return Object.keys(routePages).reduce(
    (inverted: Record<string, string>, key) => {
      inverted[routePages[key].path] = key;
      return inverted;
    },
    {}
  );
}

const StyledTabsList = styled(Tabs.List)({
  borderTop: "none !important",
  borderRight: "none !important",
  borderLeft: "none !important",
  padding: "0 16px",
});

const StyledTabsTab = styled(Tabs.Tab)({
  "&:disabled": {
    borderColor: "transparent !important",
  },
});

const Container = styled("div")({
  display: "flex",
  flexDirection: "row",
  gap: 4,
  alignItems: "center",
});

type RoutePages = Record<string, RoutePage>;

type RoutePage = {
  path: string;

  disabled?: boolean;

  showRecordCount?: boolean;
};
