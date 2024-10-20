import { IPublicClientApplication } from "@azure/msal-browser";
import { useMsal } from "@azure/msal-react";
import { Container } from "@mui/material";


function signInClickHandler(instance: IPublicClientApplication) {
  instance.loginRedirect();
}

// SignInButton Component returns a button that invokes a popup login when clicked
const SignInButton = () => {
  // useMsal hook will return the PublicClientApplication instance you provided to MsalProvider
  const { instance } = useMsal();

  return <button onClick={() => signInClickHandler(instance)}>Login</button>;
}

const NotLoggedInPage = () => {

  return (
    <Container>
      <div>Not logged in</div>
      <SignInButton />
    </Container>
  );

}

export default NotLoggedInPage;
