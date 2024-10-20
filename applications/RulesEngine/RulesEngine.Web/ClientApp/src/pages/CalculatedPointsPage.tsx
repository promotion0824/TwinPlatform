import { Alert, AlertTitle, Button, Grid, Snackbar, Stack } from '@mui/material';
import { useState } from 'react';
import { VisibleIf } from '../components/auth/Can';
import CalculatedPointsGrid from '../components/grids/CalculatedPointsGrid';
import useApi from '../hooks/useApi';
import { Link } from 'react-router-dom';
import FlexTitle from '../components/FlexPageTitle';

const apiclient = useApi();

const CalculatedPointsPage = () => {
  const [requestingJob, setRequestingJob] = useState(false);
  const [jobRequested, setJobRequested] = useState(false);

  const rebuildPoints = async () => {
    try {
      setRequestingJob(true);
      await apiclient.rebuild_Calculated_Points();
    }
    finally {
      setRequestingJob(false);
      setJobRequested(true);
    }
  };

  const handleClose = (_event?: React.SyntheticEvent | Event, _reason?: string) => {
    setJobRequested(false);
  };

  const gridProps = {
    ruleId: "all",
    pageId: 'CalculatedPoints'
  };

  return (
    <Stack spacing={2}>
      <Grid container>
        <Grid item xs={12} md={4}>
          <FlexTitle>
            Calculated Points
          </FlexTitle>
        </Grid>
        <Grid item xs={12} md={8}>
          <Grid container spacing={1} alignContent="center" justifyContent="right">
            <Grid item>
              <Link to="/rulecreate/calculated-point">
                <Button variant="contained" color="primary">Create</Button>
              </Link>
            </Grid>
            <Grid item>
              <VisibleIf canEditRules>
                <Button variant="outlined" color="secondary" disabled={requestingJob} onClick={() => { rebuildPoints(); }}>
                  Rebuild points
                </Button>
              </VisibleIf>
            </Grid>
          </Grid>
        </Grid>
      </Grid>
      <CalculatedPointsGrid query={gridProps} />

      <Snackbar open={jobRequested} onClose={handleClose} autoHideDuration={6000} >
        <Alert onClose={handleClose} sx={{ width: '100%' }} variant="filled">
          <AlertTitle>Request submitted</AlertTitle>
          Please go to the admin page for progress.
        </Alert>
      </Snackbar>
    </Stack>
  );
}

export default CalculatedPointsPage;
