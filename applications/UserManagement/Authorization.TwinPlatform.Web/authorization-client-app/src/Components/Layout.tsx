import MenuAppBar from './MenuAppBar';
import LeftDrawer from './LeftDrawer';
import { styled } from '@mui/material/styles';
import { Box, Stack } from '@willowinc/ui';
import { Outlet } from 'react-router';
import { AppContext } from '../Providers/AppContext';
import { ProgressWithBackDrop } from './ProgressWithBackDrop';
import { FunctionComponent } from 'react';
import { SnackbarProvider } from 'notistack';
import PermissionProvider from '../Providers/PermissionProvider';
import { GridRowSelectionModel, Group, Panel, PanelGroup, PanelHeader } from '@willowinc/ui';

const Layout: FunctionComponent = () => {

  return (
    <AppContext>
      <SnackbarProvider maxSnack={3}>
        <PermissionProvider>
          <MenuAppBar />
          <Main>
            <LeftDrawer />
            <Stack p="s16" flex={1}>
              <Outlet />
            </Stack>
            <ProgressWithBackDrop />
          </Main>
        </PermissionProvider>
      </SnackbarProvider>
    </AppContext>
  );
};

const Main = styled("main")({
  display: "flex",
  flexDirection: "row",
  flexGrow: 1,
  width: "100%",
  height: "100%",
  overflowY: "auto",
});

export default Layout;
