import { Box } from '@mui/material';
import ThemeSidenavMenu from './ThemeSidenavMenu';
import { mastheadHeight } from './Masthead';

const themeSidenavWidth = '240px';

const ThemeSidenav = () => {
  return (
    <Box
      id='Sidenav'
      component='nav'
      sx={{
        position: 'fixed',
        top: mastheadHeight,
        p: 2,
        zIndex: 'drawer',
        bgcolor: 'background.willow.panel',
        borderRight: '1px solid',
        borderColor: 'divider',
        width: themeSidenavWidth,
        height: '100%',
      }}
    >
      <ThemeSidenavMenu />
    </Box>
  );
};

export { ThemeSidenav, themeSidenavWidth };
