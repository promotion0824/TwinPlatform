import { useState } from 'react';
import { useMsal } from '@azure/msal-react';
import { Avatar, Menu, MenuItem, Tooltip, CircularProgress, useTheme, Stack, Typography, Grid } from '@mui/material';
import { SilentRequest } from '@azure/msal-browser';
import { useQuery } from 'react-query';
import useApi from '../hooks/useApi';
import { VisibleIf } from './auth/Can';
import env from '../services/EnvService';

const getInitials = (n: string | undefined) => {
  if (((n?.length ?? 0) === 0)) return '?';
  var names = n!.split(' '),
    initials = names[0].substring(0, 1).toUpperCase();
  if (names.length > 1) {
    initials += names[names.length - 1].substring(0, 1).toUpperCase();
  }
  return initials;
};

/**
 * UserIcon represents the logged in user with options to login or out or a logged out user
 * It should sit with a z-order above the app when open
 */
const UserIcon = () => {
  const { instance, accounts } = useMsal();

  const apiclient = useApi();

  const theme = useTheme();

  const logout = () => {
    const logoutRequest = {
      account: accounts[0]
    }
    instance.logoutRedirect(logoutRequest);
  };

  const login = async () => {
    await instance.handleRedirectPromise();
    await instance.loginRedirect();
  };

  const token = useQuery(['token', accounts], async (_x) => {
    const account = accounts[0];
    const silentRequest: SilentRequest = { account: account, scopes: ["https://graph.microsoft.com/.default"] };
    try {
      return await instance.acquireTokenSilent(silentRequest);
    }
    catch (e) {
      logout();
    }
    return null;
  });

  const user = useQuery(['user'], async (_x) => {
    const data = await apiclient.getUserInfo('me');
    console.log('Got user', data);
    return data;
  },
    {
      enabled: !!token
    }
  );

  const [anchorEl, setAnchorEl] = useState<HTMLDivElement | null>(null);

  const open = Boolean(anchorEl);

  const handleClick = (event: React.MouseEvent<HTMLDivElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  if (user.isFetched) {
    return (
      <>
        <Tooltip title={user?.data?.user?.email ?? "not logged in"}>
          <Avatar sx={{ backgroundColor: theme.palette.primary.main, color: theme.palette.primary.contrastText, width: '36px', height: '36px', fontSize: 'medium' }}
            onClick={handleClick}>{getInitials(user.data?.user?.displayName)}</Avatar>
        </Tooltip>
        <Menu open={open} onClose={handleClose} anchorEl={anchorEl} disableScrollLock
          anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
          transformOrigin={{ vertical: 'top', horizontal: 'right' }}>
          <MenuItem onClick={logout}>Logout</MenuItem>

          <VisibleIf canViewSwitcher>
            <Grid container>
              <Grid item xs={6}>
                <MenuItem key='wsup' component='a' href='https://wsup.willowinc.com'>Switcher</MenuItem>
              </Grid>
            </Grid>
          </VisibleIf>
        </Menu>
      </>
    );
  }
  else if (token.isError || user.isError) {
    return (<Avatar sx={{ bgcolor: 'red', color: 'white' }} onClick={login}>?</Avatar>)
  }
  else if (token.isLoading || user.isLoading) {
    return (<CircularProgress color="secondary" />);
  }
  else {
    return (<Avatar sx={{ bgcolor: 'red', color: 'white' }} onClick={login}>?</Avatar>)
  }
};

export default UserIcon;
