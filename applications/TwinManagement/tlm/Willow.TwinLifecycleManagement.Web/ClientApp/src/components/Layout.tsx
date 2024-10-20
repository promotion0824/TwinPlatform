import * as React from 'react';
import { useState } from 'react';
import NavBar from './NavBar';
import PermissionProvider from '../providers/PermissionProvider';
import styled from '@emotion/styled';

export const AppContext = React.createContext([] as any);

export const Layout = ({ children }: { children: React.ReactNode }) => {
  const [appContext, setAppContext] = useState({ inProgress: false } as IAppContext);

  return (
    <AppContext.Provider value={[appContext, setAppContext]}>
      <PermissionProvider>
        <RootContainer>
          <NavBar />
          <ContentContainer>{children}</ContentContainer>
        </RootContainer>
      </PermissionProvider>
    </AppContext.Provider>
  );
};

export interface IAppContext {
  inProgress: boolean;
}

const ContentContainer = styled('div')({
  width: '100%',
  padding: 16,
  flexGrow: 1,
  overflowY: 'auto',
  display: 'flex',
  flexDirection: 'column',
});

const RootContainer = styled('div')({
  height: '100%',
  display: 'flex',
  flexDirection: 'column',
});
