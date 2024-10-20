import { useCallback } from "react";
import { useMsal } from "@azure/msal-react";
import { Button } from "@mui/material";
import { IPublicClientApplication } from "@azure/msal-browser";

export const SignInButton = () => {

  const { instance } = useMsal();

  const signInClickHandler = (instance: IPublicClientApplication) => {
    instance.loginRedirect();
  }

  const signInOnClick = useCallback(() => {
    signInClickHandler(instance);
  },[]);

  return (
    <Button sx={{ mt: 2 }} type='button' onClick={signInOnClick} color='primary' variant="contained">Sign In</Button>
  );
}
