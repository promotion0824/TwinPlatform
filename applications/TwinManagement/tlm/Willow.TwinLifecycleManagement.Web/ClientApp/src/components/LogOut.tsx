import { useMsal } from '@azure/msal-react';
import { Button } from '@mui/material';
import { loginRequest } from '../authConfig';

const LogOut = () => {
  const { instance } = useMsal();

  const performLogout = () => {
    if (localStorage.getItem('token')) localStorage.removeItem('token');
    // @ts-ignore
    instance.logoutRedirect(loginRequest());
  };

  return <Button data-cy="log-out-button" onClick={performLogout}>Log out</Button>;
};

export { LogOut };
