import { Delete, Sync } from '@mui/icons-material';
import { Alert, Box, Button, Card, CardContent, Grid, Snackbar, Stack, Tab, Tabs, Tooltip, Typography, useTheme } from '@mui/material';
import * as React from 'react';
import { useEffect, useState } from 'react';
import { ErrorBoundary, withErrorBoundary } from 'react-error-boundary';
import { QueryErrorResetBoundary, useQuery, useQueryClient } from 'react-query';
import { useNavigate, useParams } from 'react-router-dom';
import CopyToClipboardButton from '../components/CopyToClipboard';
import { ErrorFallback } from '../components/error/errorBoundary';
import FlexTitle from '../components/FlexPageTitle';
import CommandsGrid from '../components/grids/CommandsGrid';
import InsightDependenciesTable from '../components/grids/InsightDependenciesTable';
import InsightGraph from '../components/insights/InsightGraph';
import InsightOccurrences from '../components/insights/InsightOccurrences';
import { InsightStatusFormatterWithLink } from '../components/insights/InsightStatusFormatter';
import InsightSummary from '../components/insights/InsightSummary';
import InsightTimeSeries from '../components/insights/InsightTimeSeries';
import { InsightFaultyFormatter, InsightValidFormatter } from '../components/LinkFormatters';
import { RuleInstanceStatusAlert } from '../components/RuleInstanceStatus';
import StyledLink from '../components/styled/StyledLink';
import TabPanel from '../components/tabs/TabPanel';
import TwinLocations from '../components/TwinLocations';
import ChipList from '../components/ChipList';
import useApi from '../hooks/useApi';
import { BatchRequestDto } from '../Rules';

const InsightPage = withErrorBoundary(() => {
  const theme = useTheme();
  const params = useParams<{ id: string }>();

  if (!params.id) return (<div>No id supplied</div>);

  const apiclient = useApi();
  const queryClient = useQueryClient();
  const [isPostingToCommand, setPostingToCommand] = useState(false);
  const [syncFinished, setSyncFinished] = useState(false);
  const [syncSucceeded, setSyncSucceeded] = useState(false);
  const [commandEnabled, setCommandEnabled] = useState(false);
  const [tabValue, setTabValue] = useState(0);

  const insightQuery = useQuery(['insights', params.id], async () => {
    const insightDto = await apiclient.getInsight(params.id!);
    return insightDto;
  },
    {
      useErrorBoundary: true,
      refetchInterval: 10 * 60 * 1000
    });

  const ruleInstanceQuery = useQuery(["ruleInstance", params.id], async () => {
    const ruleInstance = await apiclient.getRuleInstance(params.id);
    return ruleInstance;
  }, {
    useErrorBoundary: true
  });

  const insight = insightQuery.data;

  const handleTabChange = (_event: React.ChangeEvent<{}>, newValue: number) => {
    setTabValue(newValue);
  };

  useEffect(() => {
    setCommandEnabled(insight?.commandEnabled === true);
  }, [insight]);

  const handleSyncErrorClose = (_event?: React.SyntheticEvent | Event, _reason?: string) => {
    setSyncFinished(false);
  };

  const onPostInsight = async (_e: any) => {
    if (insight) {
      setPostingToCommand(true);
      setSyncSucceeded(true);
      const toggled = !commandEnabled;

      try {
        await apiclient.postInsightToCommand(params.id, toggled);
      }
      catch (e) {
        setSyncSucceeded(false);
        console.error(e);
      }

      setCommandEnabled(toggled);
      setSyncFinished(true);
      setPostingToCommand(false);

      // Invalidate all insight queries and individual insight queries
      queryClient.invalidateQueries('insights');
    }
  };

  const [isDeletingInsight, setDeletingInsight] = useState(false);

  const navigate = useNavigate();

  const onDeleteInsight = async (_e: any) => {
    if (insight) {
      setDeletingInsight(true);
      await apiclient.deleteInsight(params.id);
      setDeletingInsight(false);
      queryClient.invalidateQueries(['insight', params.id], {
        exact: true
      });

      //redirect back to all Insights page
      navigate('../insights/all');
    }
  };

  const onDeleteAllInsights = async (_e: any) => {
    if (insight) {
      setDeletingInsight(true);
      await apiclient.deleteInsightsForRule(insight.ruleId);
      setDeletingInsight(false);
      queryClient.invalidateQueries(['insights'], {
        exact: true
      });

      //redirect back to all Insights page
      navigate('../insights/all');
    }
  };

  const commansGridQuery = {
    invokeQuery: (request: BatchRequestDto) => {
      return apiclient.commandsForRuleInstance(params.id, request);
    },
    downloadCsv: (request: BatchRequestDto) => {
      return apiclient.exportCommandsForRuleInstance(params.id, request);
    },
    key: params.id,
    pageId: 'InsightSingle'
  };

  if (insightQuery.isFetched && insightQuery.data && insight) {
    return (
      <QueryErrorResetBoundary>
        {({ reset }: { reset: any }) => (
          <ErrorBoundary onReset={reset} FallbackComponent={ErrorFallback}>
            <Stack spacing={2}>
              <Box sx={{ flex: 1 }}>
                <Grid container spacing={2}>
                  <Grid item sm={9}>
                    <FlexTitle>
                      <StyledLink to={"/insights/all"}>Insights</StyledLink>
                      {(insightQuery.isFetched && insightQuery.data) && insightQuery.data.id}
                    </FlexTitle>
                  </Grid>
                  <Grid item sm={3}>
                    <Stack direction="row" spacing={1} justifyContent="right">
                      {insight &&
                        <Tooltip title="Delete all Insights for this skill and purge them everywhere" enterDelay={4000} >
                          <Button variant="contained" disabled={isDeletingInsight} color="error" onClick={onDeleteAllInsights}>
                            <Delete sx={{ mr: 1, fontSize: "medium" }} />Delete all
                          </Button>
                        </Tooltip>
                      }
                      {insight &&
                        <Tooltip title="Delete this Insight and purge it everywhere" enterDelay={4000} >
                          <Button variant="contained" color="error" disabled={isDeletingInsight} onClick={onDeleteInsight}>
                            <Delete sx={{ mr: 1, fontSize: "medium" }} />Delete
                          </Button>
                        </Tooltip>
                      }
                      {commandEnabled &&
                        <Tooltip title="Synchronize this Insight each time it changes" enterDelay={4000}>
                          <Button variant="contained" disabled={isPostingToCommand} color="success" onClick={onPostInsight}>
                            <Sync sx={{ mr: 1, fontSize: "medium" }} />Stop sync
                          </Button>
                        </Tooltip>
                      }
                      {!commandEnabled &&
                        <Tooltip title="Synchronize this Insight each time it changes" enterDelay={4000} >
                          <Button variant="contained" disabled={isPostingToCommand} color="success" onClick={onPostInsight}>
                            <Sync sx={{ mr: 1, fontSize: "medium" }} />Sync
                          </Button>
                        </Tooltip>
                      }
                    </Stack>
                  </Grid>
                </Grid>
              </Box>

              <Card sx={{ backgroundColor: theme.palette.background.paper }} >
                <CardContent>
                  <Box flexGrow={1}>
                    <Stack spacing={2}>
                      {ruleInstanceQuery.data && <RuleInstanceStatusAlert ruleInstance={ruleInstanceQuery.data!} /> }
                      {insight.text && <Typography variant="body1">{insight.text!}</Typography>}
                      <Stack direction="row" alignItems="center" spacing={1}>
                        <Typography variant="body1"> Faulty: {InsightFaultyFormatter(insight)}</Typography>
                        <Typography variant="body1"> Valid: {InsightValidFormatter(insight)}</Typography>
                      </Stack>
                      <Typography variant="body1">Skill: <StyledLink to={"/rule/" + encodeURIComponent(insight.ruleId!)}>{insight.ruleName}</StyledLink></Typography>
                      <Typography variant="body1">Skill Instance: <StyledLink to={"/ruleinstance/" + encodeURIComponent(insight.ruleInstanceId!)}>{insight.ruleInstanceId!}</StyledLink ></Typography>
                      {insight.ruleTags &&
                      <Stack direction="row" alignItems="center" spacing={1}>
                        <Typography variant="body1">Tags: </Typography>
                        <ChipList values={ insight.ruleTags } />
                      </Stack>}
                      <Typography variant="body1">
                        Equipment: <StyledLink to={"/equipment/" + encodeURIComponent(insight.equipmentId!)}>{insight.equipmentName}</StyledLink><CopyToClipboardButton content={insight.equipmentId!} />
                      </Typography>
                      <Typography variant="body1">Timezone: {insight.timeZone}</Typography>
                      <TwinLocations locations={insight.locations} />
                      {insight.commandEnabled &&
                        <><Typography variant="body1">Command Id: {insight.commandInsightId!}<CopyToClipboardButton content={insight.commandInsightId!} /></Typography>
                          <Typography variant="body1">Status: {InsightStatusFormatterWithLink(insight)}</Typography></>}
                    </Stack>
                  </Box>
                </CardContent>
              </Card>

              <Box flexGrow={1}>
                <Tabs value={tabValue} onChange={handleTabChange} aria-label="simple tabs example">
                  <Tab label="Summary" />
                  <Tab label="Occurrences" />
                  <Tab label="Timeseries" />
                  <Tab label="Graph" />
                  <Tab label="Dependencies" />
                  <Tab label="Commands" />
                </Tabs>
                <TabPanel value={tabValue} index={0}>
                  <InsightSummary single={insightQuery.data!} />
                </TabPanel>

                <TabPanel value={tabValue} index={1}>
                  <InsightOccurrences single={insightQuery.data!} />
                </TabPanel>

                <TabPanel value={tabValue} index={2}>
                  <InsightTimeSeries single={insightQuery.data!} />
                </TabPanel>

                <TabPanel value={tabValue} index={3}>
                  <InsightGraph single={insightQuery.data!} />
                </TabPanel>

                <TabPanel value={tabValue} index={4}>
                  <InsightDependenciesTable props={{ insight: insightQuery.data!, pageId: 'Insight', key: insightQuery.data!.id! }} />
                </TabPanel>

                <TabPanel value={tabValue} index={5}>
                  <CommandsGrid query={commansGridQuery} />
                </TabPanel>
              </Box>
            </Stack>
            <Snackbar open={syncFinished && commandEnabled} onClose={handleSyncErrorClose} autoHideDuration={6000} >
              <Alert onClose={handleSyncErrorClose} severity={syncSucceeded === true ? "success" : "error"} sx={{ width: '100%' }}>
                {(syncSucceeded !== true) && <>Command sync failed</>}
                {(syncSucceeded === true) && <>Command sync succeeded</>}
              </Alert>
            </Snackbar>
          </ErrorBoundary>
        )}
      </QueryErrorResetBoundary>
    )
  } else {
    return <div>Loading...</div>
  }
},
  {
    FallbackComponent: ErrorFallback, //using general error view
    onError(error, info) {
      console.log('from error boundary in InsightPage: ', error, info)
    }
  })

export default InsightPage;
