import { useMsal } from '@azure/msal-react';
import { Button, LinearProgress } from '@mui/material';
import { InteractionStatus } from '@azure/msal-browser';
import styled from '@emotion/styled';

const NotLoggedIn = () => {
  const { instance, inProgress } = useMsal();

  const login = async () => {
    await instance.loginRedirect();
  };

  if (inProgress !== InteractionStatus.None) {
    return (
      <Flex>
        <StyledH1>Twin Lifecycle Management</StyledH1>
        <StyledH5 data-cy="please-wait-loading-message">Please wait...</StyledH5>
        <LinearProgress sx={{ width: '75%' }} />
      </Flex>
    );
  } else {
    return (
      <Flex>
        <StyledH1>Twin Lifecycle Management</StyledH1>
        <StyledH5>Please log in or contact your environment administrator to add you to the list of users</StyledH5>
        <Button
          data-cy="login-button"
          variant="contained"
          size="large"
          onClick={() => {
            login();
          }}
        >
          Login
        </Button>
      </Flex>
    );
  }
};

export { NotLoggedIn };

const Flex = styled('div')({
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  height: '100vh',
  flexDirection: 'column',
});

const StyledH1 = styled('h1')({
  fontSize: '2.5rem',
  fontWeight: 500,
  lineHeight: 1.2,
  margin: '0 0 0.5rem 0',
});

const StyledH5 = styled('h5')({ fontSize: '1.25rem', fontWeight: 500, margin: '0 0 0.5rem 0' });
