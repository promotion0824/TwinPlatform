import { useMsal } from '@azure/msal-react';
import { Avatar, Menu, MenuItem, useTheme } from '@mui/material';
import { SilentRequest } from '@azure/msal-browser';
import { useQuery } from 'react-query';
import { loginRequest } from '../authConfig';
import { useState } from 'react';

const getInitials = (n: string | undefined) => {
  if (((n?.length ?? 0) === 0)) return '?';
  let names = n!.split(' '),
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
    const silentRequest: SilentRequest = { account: account, scopes: loginRequest.scopes };
    try {
      return await instance.acquireTokenSilent(silentRequest);
    }
    catch (e) {
      console.log('Failed to acquire token, logging out and back in', e);
      logout();
      login();   // ignore, remove
    }
    return null;
  });

  const [anchorEl, setAnchorEl] = useState<HTMLDivElement | null>(null);

  const open = Boolean(anchorEl);

  const handleClick = (event: React.MouseEvent<HTMLDivElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };


  return (
    <>
      <Avatar
        onClick={handleClick}
        sx={{ backgroundColor: theme.palette.primary.main, color: theme.palette.primary.contrastText, width: '36px', height: '36px', fontSize: 'medium' }}>
        {getInitials(token.data?.account?.name)}
      </Avatar>
      <Menu open={open} onClose={handleClose} anchorEl={anchorEl} disableScrollLock
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        transformOrigin={{ vertical: 'top', horizontal: 'right' }}>
        <MenuItem onClick={logout}>Logout</MenuItem>

      </Menu>
    </>

  );
}

export default UserIcon;
