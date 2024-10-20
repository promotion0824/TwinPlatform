import { Outlet } from 'react-router-dom';
import { Box } from '@mui/material';
import { Masthead, mastheadHeight } from './components/Masthead';
import { ThemeSidenav } from './components/ThemeSidenav';
import { ThemeContent } from './components/ThemeContent';

function SidenavLayout() {
  return (
    <Box
      id='Layout'
      sx={{
        width: '100%',
        height: '100%',
      }}
    >
      <Masthead />
      <Box
        id='ContentWrapper'
        component='div'
        sx={{
          height: '100vh',
          width: '100%',
          display: 'flex',
          flexDirection: 'row',
          position: 'relative',
          paddingTop: mastheadHeight,
        }}
      >
        <ThemeSidenav />
        <ThemeContent>
          <Outlet />
        </ThemeContent>
      </Box>
    </Box>
  );
}

export default SidenavLayout;
