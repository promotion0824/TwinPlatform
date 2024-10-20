import { Box, Divider, Link, Stack, Typography } from '@mui/material';
import { memo } from 'react';
import styledComp from 'styled-components';
import { endpoints } from '../Config';
import logo from "../icons/WillowTextLogo";
import UserProfileBar from './UserProfileBar';

function MenuAppBar() {
  return (
    <CommandTitleBar>
      <Box sx={{ display: 'flex', alignItems: 'center' }}>
        <Link href={endpoints.baseName}>{logo}</Link>
        <Divider orientation="vertical" flexItem variant="fullWidth" sx={{marginLeft:'0.5rem', marginRight:'0.5rem'} } />
        <Link variant="h2" href={endpoints.baseName} sx={{ flexGrow: 1, fontWeight:'400', color:'white', textDecoration:'none' }}>
          User Management
        </Link>
      </Box>
      <UserProfileBar />
    </CommandTitleBar>
  );
}

const CommandTitleBar = styledComp.div(({ theme }) => ({
  alignItems: 'center',
  backgroundColor: theme.color.neutral.bg.panel.default,
  borderBottom: `1px solid ${theme.color.neutral.border.default}`,
  display: 'flex',
  gap: theme.spacing.s16,
  height: '52px',
  position: "static",
  top: 0,
  left: 0,
  flexShrink: 0,
  justifyContent: 'space-between',
  padding: `0 ${theme.spacing.s16}`,
}));
export default memo(MenuAppBar);
