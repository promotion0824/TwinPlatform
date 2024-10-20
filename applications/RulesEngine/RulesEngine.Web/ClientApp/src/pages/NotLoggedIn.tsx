import { alpha, Button, Grid, Paper, styled, Typography } from '@mui/material';
import { useMsal } from '@azure/msal-react';
import { Helmet } from 'react-helmet';
import env from '../services/EnvService';
import logo from '../components/icons/Logo';
import FlexTitle from '../components/FlexPageTitle';

const NotLoggedIn = () => {

  const { instance } = useMsal();

  const login = async () => {
    await instance.handleRedirectPromise();
    await instance.loginRedirect();
  };

  const baseurl = env.baseurl();
  const environment = env.customerName();

  const LayoutContainer = {
    display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh',
    background: 'url(' + baseurl + 're_Background.webp)',
    backgroundPosition: '50%',
    backgroundSize: 'cover',
    bottom: 0,
    left: 0,
    right: 0,
    top: 0
  }

  const StyledPaper = styled(Paper)(({ theme }) => ({
    backgroundColor: alpha(theme.palette.background.default, 0.9),
    color: theme.palette.primary.contrastText,
    maxWidth: '600px',
    padding: theme.spacing(2)
  }));

  return (
    <>
      <Helmet>
        <title>{'Willow Activate'}</title>
        <meta name="description" content="The Willow Activate Technology creates insights in response to events in Willow Twins" />
      </Helmet>

      <div style={LayoutContainer}>
        <StyledPaper>
          <Grid container spacing={2} alignItems="center">
            <Grid item sm={12}>
              <Grid container spacing={2} alignItems="center">
                <Grid item sm={6}>
                  <FlexTitle>
                    Activate Technology
                  </FlexTitle>
                </Grid>
                <Grid item sm={6}>
                  <Typography variant="h1" align='right'>{logo}</Typography>
                </Grid>
              </Grid>
            </Grid>
            <Grid item sm={12}>
              <Typography variant="body1">Welcome to Willow’s Activate Technology, please log in.
              </Typography>
            </Grid>
            <Grid item xs={2} mt={1}>
              <Button variant="contained" color="primary" onClick={login}>Login</Button>
            </Grid>
            <Grid item xs={10} mt={1} textAlign="right">
              <Typography variant="h5">{environment}</Typography>
            </Grid>
          </Grid>
        </StyledPaper>
      </div>
    </>
  );
}

export default NotLoggedIn;
