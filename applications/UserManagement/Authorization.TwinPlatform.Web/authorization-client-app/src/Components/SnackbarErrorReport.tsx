import { SnackbarContent, SnackbarKey, SnackbarMessage } from 'notistack'
import { forwardRef, ReactNode, useCallback, useState } from 'react'
import { Alert, Paper, Typography, Button, Stack, Card, CardContent, CardActions } from '@mui/material';
import { Check, ContentCopyOutlined } from '@mui/icons-material';

export class SnackbarErrorReportContent {
  static getCustomContent = (key: SnackbarKey, message: SnackbarMessage, correlationId: string): ReactNode => {
    return <SnackbarErrorReport id={key} message={message} correlationId={correlationId}></SnackbarErrorReport>
  }
}

interface ISnackbarErrorReportProps {
  id: SnackbarKey,
  message: SnackbarMessage
  correlationId: string
}

const SnackbarErrorReport = forwardRef<HTMLDivElement, ISnackbarErrorReportProps>((props, ref) => {

  const [clipboardCopied, setClipboardCopied] = useState(false);
  const {
    // You have access to notistack props and options 👇🏼
    id,
    message,
    // as well as your own custom props 👇🏼
    correlationId,
    ...other
  } = props;
  const copyToClipBoard = useCallback(() => {
    setClipboardCopied(true);
    navigator.clipboard.writeText("CorrelationId : " + correlationId)
  },[correlationId]);

  return (

    <SnackbarContent ref={ref} role="alert" {...other}>
      <Stack spacing={0} sx={{ width: '25vw', maxWidth: '50vh' }}>
        <Alert severity="error" variant="filled">{message}</Alert>
        <Card >
          <CardContent>
            <Paper>
              <Typography>
                Please report this problem to admin for troubleshooting.
              </Typography>
              <Typography>
                Location : {window.location.href}
              </Typography>
              <Typography>
                Time : {(new Date()).toISOString()}
              </Typography>
              {correlationId !== '' &&
                <Typography>
                  CorrelationId :  {correlationId}
                </Typography>
              }
            </Paper>
          </CardContent>
          <CardActions>
            {clipboardCopied ?
              <Button color='inherit' variant="outlined" size="small" startIcon={<Check/>}
              >Copied</Button>
              :
              <Button color='primary' variant="contained" size="small" startIcon={<ContentCopyOutlined />}
                onClick={copyToClipBoard}
              >Copy to Clipboard</Button>
            }
          </CardActions>
        </Card>

      </Stack>

    </SnackbarContent>
  )
});

export default SnackbarErrorReport;

