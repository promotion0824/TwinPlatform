import { alpha, Grid, Paper, styled, Typography } from "@mui/material";
import logo from "../components/icons/WillowTextLogo";
import { useMsal } from "@azure/msal-react";
import { Button, Loader } from "@willowinc/ui";
import { InteractionStatus } from "@azure/msal-browser";
import { AppName } from "../utils/appName";

const LayoutContainer = styled("div")({
  display: "flex",
  justifyContent: "center",
  alignItems: "center",
  height: "100vh",
  backgroundColor: "rgb(23, 23, 23)",
});

const StyledPaper = styled(Paper)(({ theme }) => ({
  backgroundColor: alpha(theme.palette.background.default, 0.9),
  color: theme.palette.primary.contrastText,
  maxWidth: "600px",
  padding: theme.spacing(3),
  marginBottom: "7vh",
}));

const NotLoggedIn = () => {
  const { instance, inProgress } = useMsal();

  const login = async () => {
    await instance.loginRedirect();
  };

  return (
    <LayoutContainer>
      {inProgress !== InteractionStatus.None ? (
        <Loader size="xl" variant="dots" />
      ) : (
        <>
          <StyledPaper>
            <Grid container spacing={2} alignItems="center">
              <Grid item sm>
                {logo}
              </Grid>
              <Grid item sm={12} style={{ paddingTop: 23 }}>
                <Typography variant="h2">{AppName}</Typography>
              </Grid>
              <Grid item sm={12}>
                <Typography variant="body1">
                  The Willow {AppName} app is an internal tool that
                  provides centralize control to optimize building performance.
                </Typography>
                <br />
                <Typography variant="body1">
                  Please direct all feedback to the {AppName} channel
                  in Teams.
                </Typography>
              </Grid>
              <Grid item xs={2} mt={1}>
                <Button size="large" kind="primary" onClick={login}>
                  Login
                </Button>
              </Grid>
            </Grid>
          </StyledPaper>
        </>
      )}
    </LayoutContainer>
  );
};

export default NotLoggedIn;
