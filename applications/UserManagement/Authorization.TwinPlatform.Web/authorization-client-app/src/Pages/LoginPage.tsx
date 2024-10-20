import { InteractionStatus } from '@azure/msal-browser';
import { useMsal } from '@azure/msal-react';
import { Box, Container, CssBaseline, LinearProgress, Paper, Typography } from '@mui/material';
import { SignInButton } from '../Components/SignInButton';


function LoginPage() {

  const { instance, inProgress } = useMsal();

  return (
    <>
      <CssBaseline />
      <Container>
        <Box sx={{
          height: '100vh', flexWrap: 'wrap'
        }}
          m={1}
          display="flex"
          height="100%"
          justifyContent="center"
          alignItems="center"
        >
          <Paper elevation={9} sx={{ p: 2 }}>
            {[InteractionStatus.Login,
              InteractionStatus.AcquireToken,
              InteractionStatus.HandleRedirect,
              InteractionStatus.SsoSilent].includes(inProgress) ?
              <>
                <Typography sx={{ minWidth: "30vw", pb: 2 }}>
                  Please wait ...
                </Typography>
                <LinearProgress />
              </> :
              <Box>
                {instance.getAllAccounts().length < 1 &&
                  <>
                  <Typography sx={{ minWidth: "30vw"}}>
                      You need to login to access User Management
                    </Typography>
                    <SignInButton /></>}
              </Box>
            }
          </Paper>
        </Box>

      </Container>
    </>
  );
}
export default LoginPage;
