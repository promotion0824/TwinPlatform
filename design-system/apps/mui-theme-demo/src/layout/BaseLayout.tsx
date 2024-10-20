import { Outlet } from 'react-router-dom';
import { Box } from '@mui/material';
import { Masthead, mastheadHeight } from './components/Masthead';
import { BaseContent } from './components/BaseContent';

function BaseLayout() {
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
        <BaseContent>
          <Outlet />
        </BaseContent>
      </Box>
    </Box>
  );
}

export default BaseLayout;
