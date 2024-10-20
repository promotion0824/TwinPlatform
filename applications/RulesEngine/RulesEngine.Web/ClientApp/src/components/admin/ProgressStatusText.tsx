import { Box, CircularProgress, createTheme, Grid, ThemeProvider, Typography } from '@mui/material';
import { ProgressDto, ProgressStatus } from '../../Rules';
import ExclamationIcon from '@mui/icons-material/ErrorOutline';
import CompletedIcon from '@mui/icons-material/CheckCircleOutline';
import QueueIcon from '@mui/icons-material/Queue';

const theme = createTheme({
  typography: {
    subtitle1: {
      fontSize: 12,
      fontStyle: 'italic',
      color: 'grey'
    }
  },
});

const styles = {
  largeIcon: {
    width: 40,
    height: 40,
  },
};

export const ProgressStatusText = (props: { isCancelling: boolean, progress: ProgressDto }) => {
  return (<div>
    <ThemeProvider theme={theme}>
      <Typography component="h2" align="right" sx={{ marginTop: 1 }}>
        <Grid container direction="row" alignItems="center">
          <Grid item>
            {props.isCancelling && <>Cancelling</>}
            {(props.progress.status === ProgressStatus._1 && props.progress.timeout === false && props.isCancelling === false) && <>In Progress</>}
            {props.progress.timeout === true && <>Timeout</>}
            {props.progress.status === ProgressStatus._2 && <>Completed</>}
            {props.progress.status === ProgressStatus._3 && <>Failed</>}
            {(props.progress.queued === true && props.isCancelling === false) && <>Queued</>}
          </Grid>
          <Grid item sx={{ paddingLeft: 1 }}>
            {props.isCancelling && <CircularProgress sx={styles.largeIcon} />}
            {(props.progress.status === ProgressStatus._1 && props.progress.timeout === false && props.isCancelling === false) && <CircularProgress sx={styles.largeIcon} />}
            {props.progress.status === ProgressStatus._2 && <CompletedIcon color="success" sx={styles.largeIcon} />}
            {props.progress.status === ProgressStatus._3 && <ExclamationIcon color="error" sx={styles.largeIcon} />}
            {(props.progress.queued === true && props.isCancelling === false) && <QueueIcon sx={styles.largeIcon} />}
          </Grid>
        </Grid>
      </Typography>
      {(props.progress.status === ProgressStatus._3) && <Typography variant="subtitle1" align="right">{props.progress.failedReason}</Typography>}
      {(props.progress.timeout === true) && <Typography variant="subtitle1" align="right">Timeout</Typography>}
    </ThemeProvider>
  </div>
  );
};
