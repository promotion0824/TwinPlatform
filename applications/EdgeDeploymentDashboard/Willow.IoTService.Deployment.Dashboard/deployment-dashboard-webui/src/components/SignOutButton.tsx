import {useMsal} from '@azure/msal-react';
import {Button} from '@mui/material';

function handleLogout(instance: any) {
  instance.logoutRedirect();
}

export const SignOutButton = () => {
  const {instance} = useMsal();

  return (
    <Button color="inherit" onClick={() => handleLogout(instance)}>Logout</Button>
  );
}
