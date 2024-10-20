import { Alert, Box, Button, Stack, Typography } from "@mui/material";
interface ErrorBoundaryProps {
  error?: any;
  resetErrorBoundary?: any;
  componentStack?: any;
}

export function ErrorFallback({
  error,
  resetErrorBoundary,
  componentStack,
}: ErrorBoundaryProps) {
  console.log(error, componentStack);
  const appHasChanged =
    error && error.toString().indexOf("ChunkLoadError") > -1;

  return (
    <Box
      sx={{
        display: "flex",
        position: "absolute",
        left: 0,
        width: "100%",
        height: 500,
      }}
      justifyContent="center"
      alignItems="center"
      alignContent="center"
    >
      <Stack direction="row">
        <Box
          sx={{
            display: "flex",
            width: 400,
            height: 400,
            backgroundAttachment: "center",
            backgroundSize: "100% 80%",
            backgroundRepeat: "no-repeat",
            backgroundBlendMode: "darken",
          }}
          justifyContent="center"
          alignItems="center"
          alignContent="center"
        ></Box>

        <Stack>
          {appHasChanged ? <h1>App updated, please reload</h1> : <h1>Oops!</h1>}
          {error && (error.status || error.title) && (
            <Alert severity="error" sx={{ background: "none" }}>
              {error.status + " " + error.title}
            </Alert>
          )}
          <Typography
            sx={{ background: "none" }}
            padding={"16px 5px"}
            left={0}
            right={0}
            fontSize={20}
            textTransform="uppercase"
            display={"inline-block"}
            component="h2"
          >
            Something went wrong
          </Typography>
        </Stack>

        <Box
          sx={{
            width: 400,
            height: 400,
          }}
        ></Box>
      </Stack>
    </Box>
  );
}
