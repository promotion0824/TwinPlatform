import { Link, LinkProps } from "react-router-dom";
import logo from "../icons/WillowTextLogo";
import { PageTitle, PageTitleItem, Tabs } from '@willowinc/ui';
import { styled } from '@mui/material/styles';
import styledComp from 'styled-components';
import * as React from 'react';
import { Route, Routes, useLocation } from 'react-router-dom';
import ApplicationTypes from './ApplicationTypes';
import Connectors from './Connectors';
import Deployer from './Deployer';
import Deployments from './Deployments';
import ErrorNotification from './ErrorNotification';
import UserIcon from './UserIcon';

const EXTENSIONNAME = "edge-deployment-dashboard";
const BASE_PAGE = "/" + EXTENSIONNAME;
export const CONNECTORS_PAGE = BASE_PAGE + "/";
export const DEPLOYMENTS_PAGE = BASE_PAGE + "/deployments";
export const DEPLOYER_PAGE = BASE_PAGE + "/deployer";
export const TYPES_PAGE = BASE_PAGE + "/types";

export default function Layout() {
  const location = useLocation();

  const [openError, setOpenError] = React.useState(false);
  const errorNotificationProps = {
    openError,
    setOpenError,
  };

  return (
    <>
      <CommandTitleBar>
        <Link to="/">
          {logo}
        </Link>
        <Spacer />
        <UserIcon />
      </CommandTitleBar>
      <Main>
        <PageTitleContainer>
          <PageTitle>
            <PageTitleItem href="#">Edge Connector Deployment Dashboard</PageTitleItem>
          </PageTitle>
        </PageTitleContainer>
        <Tabs value={location.pathname}>
          <TabsListPadded>
            <UnstyledLink to={CONNECTORS_PAGE}><Tabs.Tab value={CONNECTORS_PAGE}>Connectors</Tabs.Tab></UnstyledLink>
            <UnstyledLink to={DEPLOYMENTS_PAGE}><Tabs.Tab value={DEPLOYMENTS_PAGE}>Deployments</Tabs.Tab></UnstyledLink>
            <UnstyledLink to={DEPLOYER_PAGE}><Tabs.Tab value={DEPLOYER_PAGE}>Deployer</Tabs.Tab></UnstyledLink>
            <UnstyledLink to={TYPES_PAGE}><Tabs.Tab value={TYPES_PAGE}>Types</Tabs.Tab></UnstyledLink>
          </TabsListPadded>
        </Tabs>
        <Content>
          <Routes>
            <Route path={BASE_PAGE} element={<Connectors setOpenError={setOpenError} />}>
            </Route>
            <Route path={DEPLOYMENTS_PAGE} element={<Deployments setOpenError={setOpenError} />}>
            </Route>
            <Route path={DEPLOYER_PAGE} element={<Deployer setOpenError={setOpenError} />}>
            </Route>
            <Route path={TYPES_PAGE} element={<ApplicationTypes setOpenError={setOpenError} />}>
            </Route>
          </Routes>
        </Content>
      </Main>
      <ErrorNotification {...errorNotificationProps}></ErrorNotification>
    </>
  );
}

const TabsListPadded = styled(Tabs.List)({
  padding: "0 16px",
});

const Spacer = styled("div")({
  flexGrow: 1,
});

const Main = styled("main")({
  display: "flex",
  flexDirection: "column",
  flexGrow: 1,
});

const Content = styled("div")({
  margin: "16px",
  flexGrow: 1,
  overflow: "auto",
  display: "flex",
});

const PageTitleContainer = styled("div")({
  display: "flex",
  flexDirection: "row",
  justifyContent: "space-between",
  padding: "16px 16px 0 16px",
});

const CommandTitleBar = styledComp.div(({ theme }) => ({
  alignItems: 'center',
  backgroundColor: theme.color.neutral.bg.panel.default,
  borderBottom: `1px solid ${theme.color.neutral.border.default}`,
  display: 'flex',
  gap: theme.spacing.s16,
  height: '52px',
  padding: `0 ${theme.spacing.s16}`,
}));

const UnstyledLink = styled(Link)({
  textDecoration: "none",
});
