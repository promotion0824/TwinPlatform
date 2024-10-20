import { Alert, Box, Button, Stack, Typography } from '@mui/material';
import { useNavigate } from 'react-router-dom';
interface ErrorBoundaryProps {
  error?: any
  resetErrorBoundary?: any
  componentStack?: any,
  customText?: string
}

export function ErrorFallback({ error, resetErrorBoundary, componentStack, customText }: ErrorBoundaryProps) {

  const appHasChanged = (error && error.toString().indexOf('ChunkLoadError') > -1) ||
    (error && error.toString().indexOf('Failed to fetch dynamically imported module') > -1);

  const navigate = useNavigate();

  return (
    <Box sx={{
      display: 'flex',
      position: 'absolute',
      left: 0,
      width: "100%",
      height: 500,
    }} justifyContent="center" alignItems="center" alignContent="center"
    >
      <Stack direction="row" >
        <Box sx={{
          display: 'flex',
          width: 400,
          height: 400,
          backgroundImage: 'url(./favicon-test.svg)',
          backgroundAttachment: 'center',
          backgroundSize: '100% 80%',
          backgroundRepeat: 'no-repeat',
          backgroundBlendMode: 'darken'
        }} justifyContent="center" alignItems="center" alignContent="center"
        >
        </Box>

        <Stack>
          {appHasChanged ?
            <h1>App updated, please reload</h1> :
            (error.title) ? <h1>{error.title}</h1> : <h1>Oops!</h1>}
          
          {!error.title && <Typography sx={{ background: 'none' }} padding={'16px 5px'} left={0} right={0} fontSize={20} textTransform="uppercase" display={"inline-block"} component="h2">
            Something went wrong</Typography>}

          {
            customText && <Alert severity='error' sx={{ background: 'none' }}>
              {customText}
            </Alert>
          }
          {
            !customText && error && (error.status || error.title) &&
            <Alert severity='error' sx={{ background: 'none' }}>
              {error.status + ' ' + error.title}
            </Alert>
          }

          <Stack direction="row" spacing={2}>
            <Button variant="contained"
              onClick={resetErrorBoundary}
              type="button"
              color={"primary"} className="float-right"
            >Try again</Button>

            <Button variant="contained"
              onClick={() => {
                navigate(-1);
              }}
              type="button"
              color={"primary"} className="float-right"
            >Go back</Button>
          </Stack>
          
        </Stack>

        <Box sx={{
          width: 400,
          height: 400,
        }}></Box>

      </Stack>
    </Box>
  )
}

export function RuleErrorFallback({ error, resetErrorBoundary, componentStack }: ErrorBoundaryProps) {
  const text = error?.status == 403 ? "You do not have access to this skill" : undefined;
  return <ErrorFallback error={error} resetErrorBoundary={resetErrorBoundary} componentStack={componentStack} customText={text} />
}

export function GlobalErrorFallback({ error, resetErrorBoundary, componentStack }: ErrorBoundaryProps) {
  const text = error?.status == 403 ? "You do not have access to this macro" : undefined;
  return <ErrorFallback error={error} resetErrorBoundary={resetErrorBoundary} componentStack={componentStack} customText={text} />
}
