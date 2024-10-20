import React, { ReactNode } from 'react';
import NavBar from './NavBar';
import { Container, CssBaseline, Stack } from '@mui/material';

interface PageNavigationProps {
  // You can define any additional props you need for your navigation component
  // For example, you might want to add navigation-related props like activePage, onClick, etc.
  children: ReactNode;
}

const PageWrapper: React.FC<PageNavigationProps> = ({ children }) => {

  return (
    <>
      <CssBaseline />
      <NavBar />
      <Container maxWidth={false} disableGutters sx={{ marginTop: 0, width: "100%" }} >
        <Stack direction="column" sx={{ marginTop: "40px" }} alignItems="flex-start" minHeight="100%">
          {children}
        </Stack>
      </Container>
    </>
  );
};

export default PageWrapper;
