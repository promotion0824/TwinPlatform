import { Alert, AlertTitle, Box, Button, Checkbox, CircularProgress, Fade, FormControlLabel, Grid, Snackbar, Stack, Tooltip, Typography } from '@mui/material';
import { useQuery } from 'react-query';
import useApi from '../hooks/useApi';
import env from '../services/EnvService';
import { useState } from 'react';
import InsightsTable from '../components/grids/InsightsTable';
import { VisibleIf } from '../components/auth/Can';
import { Delete, Sync, ExpandMore } from '@mui/icons-material';
import FlexTitle from '../components/FlexPageTitle';

const InsightsPage = () => {
  const baseApi = env.baseapi();
  const apiclient = useApi();

  const tokenQuery = useQuery('insightdownload', async (_c) => {
    try {
      return await apiclient.getTokenForInsightsDownload();
    }
    catch (e: any) {
      return "";
    }
  });
  const [requestingJob, setRequestingJob] = useState(false);
  const [jobRequested, setJobRequested] = useState(false);
  const [removeCommandId, setRemoveCommandId] = useState(false);
  const [deleteCommandInsights, setDeleteCommandInsights] = useState(false);
  const [deleteActors, setDeleteActors] = useState(false);
  const [deleteTimeSeries, setDeleteTimeSeries] = useState(false);
  const [manage, setManage] = useState(false);

  const summaryQuery = useQuery(["insights-summary"], async (_x: any) => {
    const total = await apiclient.getInsightsSummary();
    return total;
  },
    {
      enabled: manage
    });

  const handleDeleteFinishedClose = (_event?: React.SyntheticEvent | Event, _reason?: string) => {
    setJobRequested(false);
  };

  const deleteAllInsights = async (_e: any) => {
    setRequestingJob(true);
    await apiclient.deleteAllInsights(deleteCommandInsights, deleteActors, deleteTimeSeries);
    setRequestingJob(false);
    setJobRequested(true);
  };

  const disableAllInsights = async (_e: any) => {
    setRequestingJob(true);
    await apiclient.disableInsights();
    setRequestingJob(false);
    setJobRequested(true);
  };

  const deleteCommandInsightsNotFlaggedToSynced = async (_e: any) => {
    setRequestingJob(true);
    await apiclient.deleteNotSyncdInsightsFromCommand(removeCommandId);
    setRequestingJob(false);
    setJobRequested(true);
  };

  const reverseSyncInsightsFromCommand = async (_e: any) => {
    setRequestingJob(true);
    await apiclient.reverseSyncInsightsFromCommand();
    setRequestingJob(false);
    setJobRequested(true);
  };

  const handleRemoveCommandId = (event: React.ChangeEvent<{ checked: boolean }>) => {
    setRemoveCommandId(event.target.checked);
  };

  const handleDeleteCommandInsights = (event: React.ChangeEvent<{ checked: boolean }>) => {
    setDeleteCommandInsights(event.target.checked);
  };

  const handleDeleteActors = (event: React.ChangeEvent<{ checked: boolean }>) => {
    setDeleteActors(event.target.checked);
  };

  const handleDeleteTimeSeries = (event: React.ChangeEvent<{ checked: boolean }>) => {
    setDeleteTimeSeries(event.target.checked);
  };

  const handleManage = () => {
    setManage((prev) => !prev);
  };

  return (
    <Stack spacing={2}>
      <Grid container>
        <Grid item xs={12} md={4}>
          <FlexTitle>
            Insights
          </FlexTitle>
        </Grid>
        <Grid item xs={12} md={8}>
          <VisibleIf canViewAdminPage>
            <div style={{ float: 'right', paddingLeft: 5 }}>
              <Button variant="outlined" color="secondary" onClick={handleManage}>
                Manage <ExpandMore sx={{ fontSize: 20 }} />
              </Button>
            </div>
          </VisibleIf>
          <VisibleIf canExportRules>
            {tokenQuery.isFetched &&
              <div style={{ float: 'right' }}>
                <a href={baseApi + "api/File/download-insights?token=" + encodeURIComponent(tokenQuery.data!.token!)} target="_blank">
                  <Button variant="contained">Download</Button>
                </a>
              </div>
            }
          </VisibleIf>         
        </Grid>
      </Grid>
      <VisibleIf canViewAdminPage>
        {manage && summaryQuery.isFetched === false && <CircularProgress />}
        {manage && summaryQuery.isFetched === true && <>
          <Typography variant="body1">{summaryQuery.data?.total} Insights, {summaryQuery.data?.totalEnabled} Insights syncing to Command and {summaryQuery.data?.totalLinked} synced at least once.</Typography>
          <Typography variant="body1">{summaryQuery.data?.totalNotSynced} Insights marked not to sync but already pushed to Command.</Typography>
          <Box sx={{ display: 'flex' }}>
            <Fade in={manage}>
              <Grid container spacing={2}>
                <Grid item xs={12} mb={.5}>
                  <Button variant="contained" disabled={requestingJob} color="success" onClick={reverseSyncInsightsFromCommand}>
                    <Sync sx={{ mr: 1, fontSize: "medium" }} />Reverse sync insights
                  </Button>
                </Grid>
                <Grid item xs={12}>
                  <Grid container alignItems="center">
                    <Grid item xs={12} md={3}>
                      <Tooltip title="Delete insights in Command that have been sync'd to Command but are now marked to no longer sync">
                        <Button variant="contained" disabled={requestingJob} color="error" onClick={deleteCommandInsightsNotFlaggedToSynced}>
                          <Delete sx={{ mr: 1, fontSize: "medium" }} />
                          Delete Command Insights not synced
                        </Button>
                      </Tooltip>
                    </Grid>
                    <Grid item xs={12} md={9}>
                      <FormControlLabel
                        control={<Checkbox color="primary" checked={removeCommandId} onChange={handleRemoveCommandId} />}
                        label="Clear Command Id after delete/not found" />
                    </Grid>
                  </Grid>
                </Grid>
                <Grid item xs={12}>
                  <Grid container alignItems="center">
                    <Grid item xs={12} md={3}>
                      <Button variant="contained" color="error" disabled={requestingJob} onClick={deleteAllInsights}>
                        <Delete sx={{ mr: 1, fontSize: "medium" }} />
                        Delete ALL Insights</Button>
                    </Grid>
                    <Grid item xs={12} md={3}>
                      <Button variant="contained" color="error" disabled={requestingJob} onClick={disableAllInsights}>
                        <Delete sx={{ mr: 1, fontSize: "medium" }} />
                        Disable ALL Insights</Button>
                    </Grid>
                    <Grid item xs={12} md={9}>
                      <FormControlLabel control={<Checkbox
                        checked={deleteCommandInsights}
                        onChange={handleDeleteCommandInsights}
                        color="primary"
                        inputProps={{ 'aria-label': 'primary checkbox' }}
                      />} label="Delete Insights from Command" />
                      <FormControlLabel control={<Checkbox
                        checked={deleteActors}
                        onChange={handleDeleteActors}
                        color="primary"
                        inputProps={{ 'aria-label': 'primary checkbox' }}
                      />} label="Delete all Actor State" />
                      <FormControlLabel control={<Checkbox
                        checked={deleteTimeSeries}
                        onChange={handleDeleteTimeSeries}
                        color="primary"
                        inputProps={{ 'aria-label': 'primary checkbox' }}
                      />} label="Delete all Time Series" />
                    </Grid>
                  </Grid>
                </Grid>
              </Grid>
            </Fade>
          </Box></>
        }
        <Snackbar open={jobRequested} onClose={handleDeleteFinishedClose} autoHideDuration={6000} >
          <Alert onClose={handleDeleteFinishedClose} sx={{ width: '100%' }} variant="filled">
            <AlertTitle>Request submitted</AlertTitle>
            Please go to the admin page for progress.
          </Alert>
        </Snackbar>
      </VisibleIf>
      <InsightsTable ruleId={"none"} pageId='Insights' />
    </Stack>
  );
}

export default InsightsPage;
