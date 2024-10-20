import Header from "./header/Header";
import NavMenu from "./header/NavMenu";
import { useMsalAuthentication } from "@azure/msal-react";
import { InteractionType } from "@azure/msal-browser";
import { styled } from "@mui/material";
import Breadcrumbs from "./Breadcrumbs/Breadcrumbs";
import { AccessRestricted } from "./Auth/Auth";
import useAuthorization from "../hooks/useAuthorization";
import { Outlet } from "react-router-dom";
import { Suspense } from "react";
import { Loader } from "@willowinc/ui";

function Layout() {
  useMsalAuthentication(InteractionType.Redirect);

  const {
    hasCanViewRequestsCommandsPermission,
    hasCanApproveExecutePermission,
    isLoading: isAuthorizationLoading,
  } = useAuthorization();

  const hasPermission = hasCanViewRequestsCommandsPermission || hasCanApproveExecutePermission;

  return (
    <RootContainer>
      <Header />

      <SubContainer>
        <Breadcrumbs />
        <div id="date-range-picker-portal" />
      </SubContainer>

      <NavMenu />

      {isAuthorizationLoading ? (
        <FlexContentContainer>
          <Loader size="lg" />
        </FlexContentContainer>
      ) : !hasPermission ? (
        <FlexContentContainer>
          <AccessRestricted />
        </FlexContentContainer>
      ) : (
        <ContentContainer>
          <Suspense fallback={<div />}>
            <Outlet />
          </Suspense>
        </ContentContainer>
      )}
    </RootContainer>
  );
}

export default Layout;

const ContentContainer = styled("div")({
  width: "100%",
  padding: 16,
  flexGrow: 1,
  overflowY: "auto",
  display: "flex",
  flexDirection: "column",
});

const FlexContentContainer = styled("div")({
  display: "flex",
  justifyContent: "center",
  alignItems: "center",
  height: "100%",
 });

const SubContainer = styled("div")({
  display: "flex",
  flexDirection: "row",
  justifyContent: "space-between",
  padding: "16px 16px 0 16px",
});

const RootContainer = styled("div", {

})({
  height: "100%",
  display: "flex",
  flexDirection: "column",
});
