import { useCallback, useState, useEffect } from 'react';
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Typography,
  useMediaQuery,
  useTheme,
} from '@mui/material';
import { endpoints } from '../../config';
import { ApiException, ErrorResponse } from '../../services/Clients';
import useUserInfo from '../../hooks/useUserInfo';
import { configService } from '../../services/ConfigService';
import Check from '@mui/icons-material/Check';
import ContentCopyOutlined from '@mui/icons-material/ContentCopyOutlined';

interface IPopUpExceptionTemplate {
  isCurrentlyOpen: boolean;
  onOpenChanged: React.Dispatch<React.SetStateAction<boolean>>;
  errorObj?: ErrorResponse | ApiException | string | undefined;
}

export const PopUpExceptionTemplate = (props: IPopUpExceptionTemplate) => {
  const userInfo = useUserInfo();
  const theme = useTheme();
  const fullScreen = useMediaQuery(theme.breakpoints.down('md'));

  const [errorDetail, setErrorDetail] = useState('');
  const [isCopied, setCopied] = useState(false);

  const copyToClipboard = useCallback(() => {
    navigator.clipboard.writeText(errorDetail);
    setCopied(true);
  }, [errorDetail]);

  const handleClose = () => {
    props.onOpenChanged(false);
  };

  const getDetailsFromError = (err: ErrorResponse | ApiException | string): string => {
    let builder: Array<string>;

    if (typeof err == typeof ErrorResponse) {
      let errorResponse = err as ErrorResponse;

      builder = [
        `ErrorResponse`,
        `Response code: ${errorResponse.statusCode}`,
        `Message: ${errorResponse.message}`,
        `Data: ${errorResponse.data}`,
        `Call stack:<br/>${errorResponse.callStack}<br/>    --- END CALLSTACK ---`,
      ];
    } else if (typeof err == typeof ApiException) {
      let apiException = err as ApiException;

      builder = [
        `ApiException`,
        `Response code: ${apiException.status}`,
        `Message: ${apiException.message}`,
        `Result: ${apiException.result}`,
        `Call stack:<br/>${apiException.stack}<br/>    --- END CALLSTACK ---`,
      ];
    } else {
      builder = [`${String(err)}`];
    }

    builder.push(`Timestamp: ${new Date().toISOString()}`);
    builder.push(`User: ${userInfo.userEmail}`);
    builder.push(`TLM version: ${configService.config.tlmAssemblyVersion}`);

    return builder.join('\n');
  };

  useEffect(() => {
    setErrorDetail(getDetailsFromError(props.errorObj!));
  }, [props.errorObj]);

  if (props.errorObj) {
    return (
      <>
        <Dialog
          fullWidth
          maxWidth="sm"
          fullScreen={fullScreen}
          open={props.isCurrentlyOpen}
          onClose={handleClose}
          aria-labelledby="responsive-dialog-title"
        >
          <DialogTitle
            id="responsive-dialog-title"
            data-cy="problem-dialog"
            variant="h1"
            style={{ fontWeight: 'normal' }}
          >
            {`🙈 There's a problem`}
          </DialogTitle>
          <DialogContent sx={{ paddingBottom: 0 }}>
            <Typography variant="subtitle1">
              Try again and if the problem persists{' '}
              <a style={{ color: theme.palette.text.primary }} href={endpoints.supportLink}>
                let us know about it
              </a>
              .
            </Typography>
            <p></p>
            <DialogContent style={{ margin: 0, padding: 0, backgroundColor: theme.palette.secondary.dark }}>
              <Button
                autoFocus
                sx={{ maxWidth: '100%', backgroundColor: theme.palette.secondary.dark }}
                variant="contained"
                fullWidth={true}
                style={{ justifyContent: 'flex-start' }}
                size="large"
                data-cy="show-details-button"
              >
                {'DETAILS'}
              </Button>
              <>
                <DialogTitle variant="subtitle1" sx={{ position: 'relative' }}>
                  <div data-cy="error-message" style={{ whiteSpace: 'pre-wrap' }}>
                    {errorDetail}
                  </div>
                  <Button
                    startIcon={isCopied ? <Check /> : <ContentCopyOutlined />}
                    variant="contained"
                    onClick={copyToClipboard}
                    sx={{
                      maxWidth: '90%',
                      position: 'absolute',
                      right: '0',
                      bottom: '0',
                      margin: '1em',
                      backgroundColor: isCopied ? theme.palette.secondary.dark : undefined,
                    }}
                  >
                    {isCopied ? 'Copied' : 'Copy to clipboard'}
                  </Button>
                </DialogTitle>
              </>
            </DialogContent>
          </DialogContent>
          <DialogActions>
            <Button
              autoFocus
              onClick={handleClose}
              sx={{ maxWidth: '90%', backgroundColor: theme.palette.secondary.dark }}
              variant="contained"
              size="large"
              data-cy="close-button"
            >
              CLOSE
            </Button>
          </DialogActions>
        </Dialog>
      </>
    );
  }
  return <></>;
};
