import { Sync } from '@mui/icons-material';
import { Alert, Box, Button, Card, CardContent, Grid, Snackbar, Stack, Tab, Tabs, Tooltip, Typography, useTheme } from '@mui/material';
import * as React from 'react';
import { useEffect, useState } from 'react';
import { ErrorBoundary, withErrorBoundary } from 'react-error-boundary';
import { QueryErrorResetBoundary, useQuery, useQueryClient } from 'react-query';
import { useParams } from 'react-router-dom';
import CommandGraph from '../components/commands/CommandGraph';
import CommandOccurrences from '../components/commands/CommandOccurrences';
import CommandSummary from '../components/commands/CommandSummary';
import CommandTimeSeries from '../components/commands/CommandTimeSeries';
import CopyToClipboardButton from '../components/CopyToClipboard';
import { ErrorFallback } from '../components/error/errorBoundary';
import FlexTitle from '../components/FlexPageTitle';
import { CommandSyncFormatter, IsTriggeredFormatter, IsValidTriggerFormatter } from '../components/LinkFormatters';
import StyledLink from '../components/styled/StyledLink';
import TabPanel from '../components/tabs/TabPanel';
import useApi from '../hooks/useApi';

const CommandPage = withErrorBoundary(() => {
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

  const commandQuery = useQuery(['commands', params.id], async () => {
    const commandDto = await apiclient.getCommand(params.id!);
    return commandDto;
  },
    {
      useErrorBoundary: true,
      refetchInterval: 10 * 60 * 1000
    });

  const command = commandQuery.data;

  const handleTabChange = (_event: React.ChangeEvent<{}>, newValue: number) => {
    setTabValue(newValue);
  };

  useEffect(() => {
    setCommandEnabled(command?.enabled === true);
  }, [command]);

  const handleSyncErrorClose = (_event?: React.SyntheticEvent | Event, _reason?: string) => {
    setSyncFinished(false);
  };

  const onPostCommand = async (_e: any) => {
    if (command) {
      setPostingToCommand(true);
      setSyncSucceeded(true);
      const toggled = !commandEnabled;

      try {
        await apiclient.postToCommand(params.id, toggled);
      }
      catch (e) {
        setSyncSucceeded(false);
        console.error(e);
      }

      setCommandEnabled(toggled);
      setSyncFinished(true);
      setPostingToCommand(false);

      // Invalidate all command queries and individual command queries
      queryClient.invalidateQueries('commands');
    }
  };

  return (
    <QueryErrorResetBoundary>
      {({ reset }: { reset: any }) => (
        <ErrorBoundary onReset={reset} FallbackComponent={ErrorFallback}>
          <Stack spacing={2}>
            <Box flexGrow={1}>
              <Grid container spacing={2}>
                <Grid item sm={10}>
                  <FlexTitle>
                    <StyledLink to={"/commands"}>Commands</StyledLink>
                    {(commandQuery.isFetched && commandQuery.data) && commandQuery.data.commandName}
                  </FlexTitle>
                </Grid>
                <Grid item sm={2}>
                  <Stack direction="row" spacing={1} justifyContent="right">
                    {commandEnabled &&
                      <Tooltip title="Synchronize this Command each time it changes" enterDelay={4000}>
                        <Button variant="contained" disabled={isPostingToCommand} color="success" onClick={onPostCommand}>
                          <Sync sx={{ mr: 1, fontSize: "medium" }} />Stop sync
                        </Button>
                      </Tooltip>
                    }
                    {!commandEnabled &&
                      <Tooltip title="Synchronize this Command each time it changes" enterDelay={4000} >
                        <Button variant="contained" disabled={isPostingToCommand} color="success" onClick={onPostCommand}>
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
                {(commandQuery.isFetched && commandQuery.data) && <Grid container spacing={2}>
                  {(!commandQuery.data.externalId || !commandQuery.data.connectorId) &&
                    <Grid item xs={12}>
                      <Alert severity="warning">
                        Warning: The command's point twin requires a Connector Id and External Id for it to be able to to sync
                      </Alert>
                    </Grid>}
                  <Grid item xs={6}>
                    <Typography variant="body1">Timezone: {commandQuery.data.timeZone}</Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body1">Valid: {IsValidTriggerFormatter(commandQuery.data!.isValid!)}</Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body1">Status: {IsTriggeredFormatter(commandQuery.data!.isTriggered!)}</Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body1">Enable Sync: {CommandSyncFormatter(commandQuery.data!.enabled!)}</Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body1">Point: <StyledLink to={"/equipment/" + encodeURIComponent(commandQuery.data.twinId!)}>{commandQuery.data!.twinName}</StyledLink><CopyToClipboardButton content={commandQuery.data!.twinName!} /></Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body1">Point Id: {commandQuery.data!.twinId}<CopyToClipboardButton content={commandQuery.data!.twinId!} /></Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body1">Equipment: <StyledLink to={"/equipment/" + encodeURIComponent(commandQuery.data.equipmentId!)}>{commandQuery.data!.equipmentName}</StyledLink><CopyToClipboardButton content={commandQuery.data!.equipmentName!} /></Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <Typography variant="body1">Equipment Id: {commandQuery.data!.equipmentId}<CopyToClipboardButton content={commandQuery.data!.equipmentId!} /></Typography>
                  </Grid>
                  <Grid item xs={12}>
                    <Stack spacing={2}>
                      <Typography variant="body1">Connector Id: {commandQuery.data.connectorId!}<CopyToClipboardButton content={commandQuery.data!.connectorId!} /></Typography>
                      <Typography variant="body1">External Id: {commandQuery.data.externalId!}<CopyToClipboardButton content={commandQuery.data!.externalId!} /></Typography>
                      <Typography variant="body1">Insight: <StyledLink to={"/insight/" + encodeURIComponent(commandQuery.data.ruleInstanceId!)}>{commandQuery.data.equipmentId}</StyledLink></Typography>
                      <Typography variant="body1">Skill: <StyledLink to={"/rule/" + encodeURIComponent(commandQuery.data.ruleId!)}>{commandQuery.data.ruleName}</StyledLink></Typography>
                    </Stack>
                  </Grid>
                </Grid>}
              </CardContent>
            </Card>
          </Stack>

          {!commandQuery.isFetched && <p>Loading...</p>}

          {(commandQuery.isFetched && commandQuery.data) &&
            <Box flexGrow={1}>
              <Tabs value={tabValue} onChange={handleTabChange} aria-label="simple tabs example">
                <Tab label="Summary" />
                <Tab label="Occurrences" />
                <Tab label="Timeseries" />
                <Tab label="Graph" />
              </Tabs>
              <TabPanel value={tabValue} index={0}>
                <CommandSummary single={commandQuery.data!} />
              </TabPanel>

              <TabPanel value={tabValue} index={1}>
                <CommandOccurrences single={commandQuery.data!} />
              </TabPanel>

              <TabPanel value={tabValue} index={2}>
                <CommandTimeSeries single={commandQuery.data!} />
              </TabPanel>

              <TabPanel value={tabValue} index={3}>
                <CommandGraph single={commandQuery.data!} />
              </TabPanel>
            </Box>
          }

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
},
  {
    FallbackComponent: ErrorFallback, //using general error view
    onError(error, info) {
      console.log('from error boundary in CommandPage: ', error, info)
    }
  })

export default CommandPage;
