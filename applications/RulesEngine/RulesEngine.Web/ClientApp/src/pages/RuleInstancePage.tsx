import { ExpandMore } from '@mui/icons-material';
import { Box, Button, Card, CardContent, Divider, FormControl, FormControlLabel, Grid, InputLabel, MenuItem, Select, SelectChangeEvent, Stack, Switch, Tab, Tabs, Typography, useTheme } from '@mui/material';
import { ChangeEvent, Suspense, useEffect, useState } from 'react';
import { ErrorBoundary, withErrorBoundary } from 'react-error-boundary';
import { QueryErrorResetBoundary, useQuery } from 'react-query';
import { useParams } from 'react-router-dom';
import { VisibleIf } from '../components/auth/Can';
import InsightTrends from '../components/chart/InsightTrends';
import Comments from '../components/Comments';
import CopyToClipboardButton from '../components/CopyToClipboard';
import { DownloadDebugInfo } from '../components/DownloadDebugInfo';
import { RuleErrorFallback } from '../components/error/errorBoundary';
import FlexTitle from '../components/FlexPageTitle';
import TwinGraph from '../components/graphs/TwinGraph';
import EquipmentCapabilityTable from '../components/grids/EquipmentCapabilityTable';
import RuleInstanceDependenciesTable from '../components/grids/RuleInstanceDependenciesTable';
import { RuleInstanceReviewStatusLookup } from '../components/Lookups';
import RuleInstanceBindings from '../components/RuleInstanceBindings';
import { RuleInstanceStatusAlert } from '../components/RuleInstanceStatus';
import RuleSimulation from '../components/RuleSimulation';
import { NoSpecialCharacters } from '../components/StringOptions';
import StyledLink from '../components/styled/StyledLink';
import TabPanel from '../components/tabs/TabPanel';
import TwinLocations from '../components/TwinLocations';
//import TagsEditor from '../components/TagsEditor';
import useApi from '../hooks/useApi';

const RuleInstancePage = withErrorBoundary(() => {
  const theme = useTheme();
  //Verify rule instance param
  const params = useParams<{ id: string }>();
  if (!params.id) return (<div>No id supplied</div>);

  //Get the data
  const apiclient = useApi();

  const ruleInstanceQuery = useQuery(["ruleInstance", params.id], async () => {
    const ruleInstance = await apiclient.getRuleInstance(params.id);
    return ruleInstance;
  }, {
    useErrorBoundary: true
  });

  const [reviewStatus, setReviewStatus] = useState("");
  const [enabled, setEnabled] = useState(false);
  //Manage tab navigation
  const [tabValue, setTabValue] = useState(0);
  const handleTabChange = (_event: React.ChangeEvent<{}>, newValue: number) => {
    setTabValue(newValue);
  };
  const [showSimulation, setShowSimulation] = useState<boolean>(false);

  const handleEnabledChange = async (event: ChangeEvent<HTMLInputElement>) => {
    setEnabled(event.target.checked);
    await apiclient.enableRuleInstance(params.id, event.target.checked == false);
  };

  const handleReviewStatusChange = async (event: SelectChangeEvent) => {
    setReviewStatus(event.target.value);
    await apiclient.updateRuleInstanceReviewStatus(params.id, parseInt(event.target.value));
  };

  useEffect(() => {
    setReviewStatus((ruleInstanceQuery.data?.reviewStatus ?? 0).toString());
    setEnabled((ruleInstanceQuery.data?.disabled ?? true) == false);
  }, [ruleInstanceQuery.isFetched])

  if (ruleInstanceQuery.isFetched &&
    !ruleInstanceQuery.isFetching &&//for refreshes
    ruleInstanceQuery.data) {
    const ruleInstance = ruleInstanceQuery.data;

    const tagsEditorProps = {
      id: "ri_single_Tags",
      key: "ri_single_TagsKey",
      queryKey: "ri_single_TagsQuery",
      defaultValue: ruleInstance.tags,
      allowFreeText: false,
      queryFn: async (_: any): Promise<string[]> => {
        try {
          const tags = await apiclient.ruleInstanceTags();
          return tags;
        } catch (error) {
          return [];
        }
      },
      valueChanged: async (newValue: string[]) => {
        if (ruleInstance.tags != newValue) {
          await apiclient.updateRuleInstanceTags(ruleInstance.id, newValue);
        }
      }
    };

    return (
      <QueryErrorResetBoundary>
        {({ reset }) => (
          <ErrorBoundary onReset={reset} FallbackComponent={RuleErrorFallback}>
            <Stack spacing={2}>
              <FlexTitle>
                <StyledLink to={"/rule/" + encodeURIComponent(ruleInstance.ruleId!)}>Skill</StyledLink>
                {ruleInstance.id}
              </FlexTitle>
              <Card sx={{ backgroundColor: theme.palette.background.paper }} >
                <CardContent>
                  <Box flexGrow={1}>
                    <Stack spacing={2}>
                      <RuleInstanceStatusAlert ruleInstance={ruleInstance} />

                      {ruleInstance.description && <Typography variant="body1">Description: {ruleInstance.description}</Typography>}
                      {ruleInstance.recommendations && <Typography variant="body1">Recommendations: {ruleInstance.recommendations}</Typography>}
                      <Typography variant="body1">Skill: <StyledLink to={"/rule/" + encodeURIComponent(ruleInstance.ruleId!)}>{ruleInstance.ruleName}</StyledLink></Typography>
                      <Typography variant="body1">
                        Equipment: <StyledLink to={"/equipment/" + encodeURIComponent(ruleInstance.equipmentId!)}>{ruleInstance.equipmentName}</StyledLink><CopyToClipboardButton content={ruleInstance.equipmentId!} />
                      </Typography>
                      {!ruleInstance.isCalculatedPointTwin && <Typography variant="body1">Insight: <StyledLink to={"/insight/" + encodeURIComponent(ruleInstance.id!)}>{ruleInstance.equipmentId}</StyledLink></Typography>}
                      {ruleInstance.isCalculatedPointTwin && < Typography variant="body1">Calculated Point: <StyledLink to={"/calculatedpoint/" + encodeURIComponent(ruleInstance.id!)}>{ruleInstance.id!}</StyledLink></Typography>}
                      <Typography variant="body1">Invocations: {ruleInstance.triggerCount}</Typography>
                      <Typography variant="body1">Timezone: {ruleInstance.timeZone}</Typography>
                      <TwinLocations locations={ruleInstance.locations} />
                      <VisibleIf canExportRules>
                        <Box><DownloadDebugInfo ruleInstanceId={params.id!} /></Box>
                      </VisibleIf>

                      <Grid container>
                        <Grid item xs={6}>
                          <Stack spacing={1}>
                            <FormControlLabel control={<Switch checked={enabled} onChange={handleEnabledChange} />} labelPlacement="start" label="Skill Deployment Enabled" />
                            <FormControl>
                              <InputLabel id="review-status-select-label">Review Status</InputLabel>
                              <Select
                                labelId="review-status-select-label"
                                id="review-status-select"
                                value={reviewStatus}
                                label="Review Status"
                                onChange={handleReviewStatusChange}>
                                {RuleInstanceReviewStatusLookup.GetStatusFilter().map((v, i) => (<MenuItem key={i} value={v.value.toString()}>{v.label}</MenuItem>))}
                              </Select>
                            </FormControl>
                            {/*<TagsEditor {...tagsEditorProps} />*/}
                            <Comments id={params.id!} comments={ruleInstance.comments!} />
                          </Stack>
                        </Grid>
                      </Grid>
                    </Stack>
                  </Box>
                </CardContent>
              </Card>
              <Box flexGrow={1}>
                <Tabs value={tabValue} onChange={handleTabChange} aria-label="simple tabs example">
                  <Tab label="Parameters and Bindings" />
                  <Tab label="Dependencies" sx={{ display: ruleInstance.isCalculatedPointTwin ? 'none' : 'inline-flex' }} />
                  <Tab label="Available bindings" />
                  <Tab label="Timeseries" />
                  <Tab label="Graph" />
                </Tabs>
                <TabPanel value={tabValue} index={0} >
                  <RuleInstanceBindings ruleInstance={ruleInstance} pageId={"RuleInstance"} />
                </TabPanel>

                <TabPanel value={tabValue} index={1} >
                  <RuleInstanceDependenciesTable props={{
                    dependencies: ruleInstance.ruleDependenciesBound, key: NoSpecialCharacters(ruleInstance.ruleName!), pageId: 'RuleInstance'
                  }} />
                </TabPanel>

                <TabPanel value={tabValue} index={2} >
                  <EquipmentCapabilityTable props={{ twinId: ruleInstance.equipmentId, pageId: 'RuleInstance' }} />
                </TabPanel>

                <TabPanel value={tabValue} index={3} >
                  <Stack spacing={2}>
                    <InsightTrends ruleInstance={ruleInstance} showSimulation={true} showDebugOptions={true} />
                    <Box>
                      <Button onClick={() => setShowSimulation(!showSimulation)} variant="outlined" color="secondary">
                        Simulation Test <ExpandMore sx={{ fontSize: 20 }} />
                      </Button>
                    </Box>
                    {showSimulation &&
                      <Stack spacing={2}>
                        <Divider textAlign="left">Execution Simulation</Divider>
                        <RuleSimulation ruleId={ruleInstance.ruleId!} equipmentId={ruleInstance.equipmentId!} showEquipmentInput={false} />
                      </Stack>
                    }
                  </Stack>
                </TabPanel>

                <TabPanel value={tabValue} index={4} >
                  <Suspense fallback={<div>Loading...</div>}>
                    <TwinGraph twinIds={[ruleInstance.equipmentId!]} isCollapsed={false} highlightedIds={ruleInstance.pointEntityIds?.map(v => v.id!)} />
                  </Suspense>
                </TabPanel>
              </Box>
            </Stack>
          </ErrorBoundary>
        )}
      </QueryErrorResetBoundary>
    )
  } else {
    return <div>Loading...</div>
  }
}, {
  FallbackComponent: RuleErrorFallback, //using general error view
  onError(error, info) {
    console.log('from error boundary in Rule Instance Page: ', error, info)
  },
})

export default RuleInstancePage;
