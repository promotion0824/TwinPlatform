import { Box, IconButton, Tooltip } from '@mui/material';
import useApi from '../../hooks/useApi';
import { ProgressDto } from '../../Rules';
import CancelIcon from '@mui/icons-material/HighlightOff';

export const ProgressButtons = (props: { progress: ProgressDto, onChange: () => void }) => {
  const apiclient = useApi();
  const progress = props.progress;

  const cancelJob = async (e: any) => {
    props.onChange();
    if (progress.canCancel) {
      await apiclient.cancelJob(progress);
    }
  };

  const cancelTitle = progress.queued === true ? "Remove" : "Stop";

  return (
    <Box>
      {progress.canCancel &&
        <Tooltip title={cancelTitle}>
          <span>
            <IconButton aria-label="stop" size="large" disabled={!progress.canCancel} onClick={cancelJob}>
              <CancelIcon color="error" />
            </IconButton>
          </span>
        </Tooltip>
      }
    </Box>);
};
