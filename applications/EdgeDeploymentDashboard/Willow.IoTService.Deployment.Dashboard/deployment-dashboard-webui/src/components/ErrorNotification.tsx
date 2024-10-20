import {Alert as MuiAlert, AlertProps, Slide, SlideProps, Snackbar} from '@mui/material';
import {forwardRef} from 'react';
import {ErrorNotificationProps} from '../types/ErrorNotificationProps';

export default function ErrorNotification(props: ErrorNotificationProps) {
  const {openError, setOpenError} = props;

  const Alert = forwardRef<HTMLDivElement, AlertProps>(function Alert(
    props,
    ref,
  ) {
    return <MuiAlert elevation={6} ref={ref} variant="filled" {...props} />;
  });

  const handleClose = (event?: React.SyntheticEvent | Event, reason?: string) => {
    if (reason === 'clickaway') {
      return;
    }

    setOpenError(false);
  };

  function TransitionUp(props: SlideProps) {
    return <Slide {...props} direction="up"/>;
  }

  return (
    <Snackbar open={openError} autoHideDuration={20000} onClose={handleClose} TransitionComponent={TransitionUp}>
      <Alert onClose={handleClose} severity="error" sx={{width: '100%'}}>
        An error occurred. Please see the console for details!
      </Alert>
    </Snackbar>
  );
}
