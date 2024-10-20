import { Alert, AlertColor, Box, Dialog, DialogActions, DialogContent, DialogContentText, Divider, Grid, Snackbar, Stack, Typography } from '@mui/material';
import { Button, Panel, PanelGroup, Tabs } from '@willowinc/ui';
import { useEffect, useState } from 'react';
import { ErrorBoundary, withErrorBoundary } from 'react-error-boundary';
import { useForm } from 'react-hook-form';
import { FaCircle } from 'react-icons/fa';
import { QueryErrorResetBoundary, useMutation, UseMutationResult, useQuery, useQueryClient } from 'react-query';
import { useNavigate, useParams } from 'react-router-dom';
import { VisibleIf } from '../components/auth/Can';
import ButtonList from '../components/ButtonList';
import { RuleErrorFallback } from '../components/error/errorBoundary';
import FlexTitle from '../components/FlexPageTitle';
import RuleDetails from '../components/formparts/RuleDetails';
import CalculatedPointsGrid from '../components/grids/CalculatedPointsGrid';
import CommandsGrid from '../components/grids/CommandsGrid';
import InsightsTable from '../components/grids/InsightsTable';
import SkillDeployments from '../components/grids/SkillDeployments';
import HeightAdjustableWrapper from '../components/HeightAdjustableWrapper';
import JsonEditor from '../components/JsonEditor';
import { RuleEnableSync } from '../components/RuleEnableSync';
import RuleExecutionDatePicker from '../components/RuleExecutionDatePicker';
import RuleFormDependencies from '../components/RuleFormDependencies';
import RuleFormParameters from '../components/RuleFormParameters';
import RuleFormTriggers from '../components/RuleFormTriggers';
import RuleGraph from '../components/RuleGraph';
import RuleMetadata from '../components/RuleMetadata';
import RuleSimulation from '../components/RuleSimulation';
import StyledLink from '../components/styled/StyledLink';
import useApi from '../hooks/useApi';
import { PageStateProvider } from '../providers/PageStateProvider';
import { BatchRequestDto, RuleDto, RuleInstanceDto, ValidationReponseDto } from '../Rules';

const maxDays: number = 365;
const defaultDays: number = 7;

interface SaveRuleProps {
  rule: RuleDto;
  formContext: any;
  onActioned: () => void;
  onSuccess: () => void;
  onError: (err: string) => void;
}

export function GetSaveRuleMutation(props: SaveRuleProps): UseMutationResult<void, unknown, any, any> {
  const { rule, onActioned, onError, onSuccess, formContext } = props;
  const { setError } = formContext;

  const apiclient = useApi();
  const queryClient = useQueryClient();

  return useMutation(async (data: RuleDto) => {
    try {
      //Set the JSON to empty for security reasons
      data!.json = '';
      await apiclient.upsertRule(rule.id, data);

      onSuccess();

      queryClient.invalidateQueries(['rule', rule.id], {
        exact: true
      });

      queryClient.invalidateQueries(['categories', 'models', 'modelsautocomplete', 'ruleInstances', 'calculatedPoints', 'ruleInstanceList', 'rulesForSimulation'], {
        exact: true
      });

      queryClient.invalidateQueries(['validateRuleParameters', rule], {
        exact: true
      });
    }
    catch (err) {
      onError(err);

      const validationResponse = err as ValidationReponseDto;
      if (validationResponse) {
        validationResponse.results?.forEach((x, _i) => {
          setError(x.field!, { type: 'manual', message: x.message! });
        });
      }
    }

    onActioned();
  });
}

const RuleSingle = withErrorBoundary(() => {
  const params = useParams<{ id: string }>();
  const queryClient = useQueryClient();
  const apiclient = useApi();
  const sharedForm = useForm();
  const { register, clearErrors, handleSubmit, formState: { errors, isSubmitting } } = useForm();

  //Need to keep the rule json seperate since we set the JSON to empty on requests for security reasons
  const [ruleJSON, setRuleJSON] = useState<string>();

  const ruleQuery = useQuery(["rule", params.id], async (_x: any) => {
    var data = await apiclient.getRule(params.id);
    setRuleJSON(data?.json!);
    return data;
  }, {
    useErrorBoundary: true //with the error boundary this is required for catching react-query related errors. To be optimised
  });

  const rule = ruleQuery.data;
  const navigate = useNavigate();

  const [daysAgo, setDaysAgo] = useState<number>(defaultDays);
  const [equipmentId, setEquipmentId] = useState("");
  const [rightTabValue, setRightTabValue] = useState("simulate");

  //Set enabled on CTAs
  const [requestingJob, setRequestingJob] = useState(false);
  const [jobRequested, setJobRequested] = useState(false);
  const handleJobRequestClose = () => { setJobRequested(false); };

  const [isSaving, setSaving] = useState(false);
  const [saveSuccess, setSaveSuccess] = useState(false);
  const [saveCompleted, setSaveCompleted] = useState(false);
  const handleSaveCloseAlert = () => { setSaveCompleted(false); };
  const [saveSeverity, setSaveSeverity] = useState<AlertColor>("error");

  const [isDeleting, setDeleting] = useState(false);
  const [deleteDialog, setDeleteDialog] = useState(false);
  const handleCloseDeleteDialog = () => { navigate('../rules'); };

  const [isExecuting, setExecuting] = useState(false);
  const [isRegenerating, setRegenerating] = useState(false);

  const saveRuleProps = {
    rule: rule!,
    formContext: sharedForm,
    onActioned: () => {
      setSaveCompleted(true);
    },
    onSuccess: () => {
      setSaveSuccess(true);
      setSaveSeverity("success");
    },
    onError: () => {
      setSaveSuccess(false);
      setSaveSeverity("error");
      setSaveCompleted(true);
    }
  };

  const mutation = GetSaveRuleMutation(saveRuleProps);

  const [hasDetailsErrors, setHasDetailsErrors] = useState(false);
  const requiredDetailsKeys = ['name', 'category', 'primaryModelId', 'recommendations'];

  //Form operations
  const onSubmit = async (_data: any, _e: any) => {
    setRequestingJob(true);
    setSaving(true);

    await mutation.mutateAsync(rule);

    setHasDetailsErrors(false);
    setRequestingJob(false);
    setSaving(false);
  }

  const onError = () => {
    if (requiredDetailsKeys.some(key => errors.hasOwnProperty(key))) {
      setHasDetailsErrors(true);
    }
    setSaveSuccess(false);
    setSaveSeverity("error");
    setSaveCompleted(true);
  }

  const onDelete = async (_e: any) => {
    setRequestingJob(true);
    setDeleting(true);
    await apiclient.deleteRule(rule!.id).then(() => {
      setRequestingJob(false);
      setDeleting(false);
      setDeleteDialog(true);
    });
  };

  const onRegenerate = async (_e: any) => {
    setRequestingJob(true);
    setRegenerating(true);
    await apiclient.rebuild_Rules(rule!.id, true).then(() => {
      setRequestingJob(false);
      setRegenerating(false);
      setJobRequested(true);
      queryClient.invalidateQueries(["rule", params.id, 'calculatedPoints', 'ruleInstances']);
    });
  };

  const onExecute = async (resetInsights: boolean) => {
    setRequestingJob(true);
    setExecuting(true);
    await apiclient.execute_single_rule(rule!.id, daysAgo, resetInsights).then(() => {
      setRequestingJob(false);
      setExecuting(false);
      setJobRequested(true);
    });
  };


  //Rule Validation - For now focused on Parameters
  const [hasParameterErrors, setHasParameterErrors] = useState(false);
  const [hasTriggerErrors, setHasTriggerErrors] = useState(false);
  const validateRule = async (rule: RuleDto) => {
    try {
      //Set the JSON to empty for security reasons
      rule!.json = '';
      await apiclient.validateRule(rule);
      setHasParameterErrors(false);
      setHasTriggerErrors(false);
    }
    catch (err) {
      const validationResponse = err as ValidationReponseDto;
      if (validationResponse && validationResponse.results) {
        let hasParamErrors = false;
        let hasTriggerErrors = false;
        validationResponse.results?.forEach((x) => {
          hasParamErrors = x.parentField == "Parameters" || x.parentField == "ImpactScores" || x.parentField == "Filters";
          hasTriggerErrors = x.parentField == "RuleTriggers";
          sharedForm.setError(x.field!, { type: 'manual', message: x.message! });
        });
        setHasParameterErrors(hasParamErrors);
        setHasTriggerErrors(hasTriggerErrors);
      }
    }
  }

  //Validate once at this level to indicate validation issues
  useEffect(() => {
    if (rule && ruleQuery.isFetched) {
      clearErrors();
      validateRule(rule);
    }
  }, [rule?.id]);

  const [adtEnabled, setAdtEnabled] = useState(false);
  const [adtEnabledAlert, setAdtEnabledAlert] = useState(false);
  const [adtEnabledSeverity, setAdtEnabledSeverity] = useState<AlertColor>("error");
  useEffect(() => {
    setAdtEnabled(rule?.adtEnabled === true);
  }, [rule?.adtEnabled]);

  const adtSyncMutation = useMutation(async (data: boolean) => {
    try {
      console.log('syncTwinsMutation...', data);
      await apiclient.enableADTSync(rule!.id, data);
      queryClient.invalidateQueries(["rule", params.id]);

      setAdtEnabledSeverity("success");
    } catch (err) {
      setAdtEnabledSeverity("error");
      console.log('syncTwinsMutation ERR');
    }

    setAdtEnabledAlert(true);
  });

  const enableAdtSync = async () => {
    await adtSyncMutation.mutateAsync(true);
    setAdtEnabled(true);
  }

  const disableAdtSync = async () => {
    await adtSyncMutation.mutateAsync(false);
    setAdtEnabled(false);
  }

  const handleCloseAdtEnabledAlert = () => { setAdtEnabledAlert(false); };

  const [isProcessingCalcPoints, setIsProcessingCalcPoints] = useState(false);
  const onProcessCalcPoints = async (_e: any) => {
    try {
      setIsProcessingCalcPoints(true);
      await apiclient.processCalcPoints(rule!.id);
      setIsProcessingCalcPoints(false);

      queryClient.invalidateQueries(["rule", params.id, 'calculatedPoints', 'ruleInstances']);
    } catch (err) {
      console.log('onProcessCalcPoints ERR');
      setIsProcessingCalcPoints(false);
    }
  };

  const [, setCommandEnabled] = useState(false);
  useEffect(() => {
    setCommandEnabled(rule?.commandEnabled === true);
  }, [rule?.commandEnabled]);

  const [dependenciesRevision, setDependenciesRevision] = useState(0);  
  const handleChange = (newValue: string) => {
    if (newValue == 'dependencies') {
      //force refreshes on the dependencies grid in case there were changes on the rule
      setDependenciesRevision(dependenciesRevision + 1);
    }
    // In case the name has changed, invalidate the all rules page grid
    queryClient.invalidateQueries(["ruleswithfilter"]);
  };

  const gridCalculatedPointsProps = {
    ruleId: params.id!,
    pageId: 'RuleSingle'
  };

  if (rule && ruleQuery.isFetched) {
    const commansGridQuery = {
      invokeQuery: (request: BatchRequestDto) => {
        return apiclient.getCommandsAfter(rule.id!, request);
      },
      downloadCsv: (request: BatchRequestDto) => {
        return apiclient.exportCommandsAfter(rule.id!, request);
      },
      key: rule.id!,
      pageId: 'RuleSingle'
    };

    const hasScanError = rule!.ruleMetadata?.scanError !== null && rule!.ruleMetadata!.scanError!.length > 0;

    const jsonEditorProps = {
      input: ruleJSON,
      saveJsonObject: (jsonObject: any) => {
        return mutation.mutateAsync(jsonObject);
      },
      updateJsonObject: (_jsonObject: any | undefined) => {
        //We could possibly use this to set a variable for page submit to evaluate
      }
    };

    return (
      <Box component="form" sx={{ flexGrow: 1 }} autoComplete="off" onSubmit={handleSubmit(onSubmit, onError)}>
        <QueryErrorResetBoundary>
          {({ reset }) => (
            <ErrorBoundary
              onReset={reset}
              fallbackRender={({ resetErrorBoundary }) => (
                <div>
                  There was an error! <Button onClick={() => resetErrorBoundary()}>Try again</Button>
                </div>
              )}>
              <Stack spacing={2}>
                <Grid container alignItems="center" justifyContent="space-between">
                  <Grid item>
                    <FlexTitle>
                      <StyledLink to={"/rules"}>Skills</StyledLink>
                      <>{rule.name}&nbsp;{rule.isDraft && <Typography variant="body1">(Draft)</Typography>}</>
                    </FlexTitle>
                  </Grid>

                  <VisibleIf canEditRules policies={rule.policies}>
                    <Grid item>
                      <Stack spacing={1} direction={'row'}>
                        <RuleExecutionDatePicker maxDays={maxDays} showLabel={false} days={daysAgo} onChange={(input: number) => { setDaysAgo(input); }} />
                        <ButtonList options={["Execute", "Execute & Reset Insights"]} onClick={(_, i) => {
                          onExecute(i == 1);
                        }} disabled={requestingJob} loading={isExecuting} />
                        <Divider orientation="vertical" flexItem />
                        <Button kind="secondary" onClick={onRegenerate} disabled={requestingJob} loading={isRegenerating}>
                          Regenerate
                        </Button>
                        <Divider orientation="vertical" flexItem />
                        <Button kind="negative" variant="outline" onClick={onDelete} disabled={requestingJob} loading={isDeleting} style={{ minWidth: '80px' }} >
                          Delete
                        </Button>
                        <Button kind="primary" type="submit" disabled={isSubmitting || requestingJob} loading={isSaving} style={{ minWidth: '80px' }}>Submit</Button>
                      </Stack>
                    </Grid>
                  </VisibleIf>
                </Grid>

                <PageStateProvider>
                  <HeightAdjustableWrapper marginBottom={20} minHeight={400}>
                    <PanelGroup resizable autoSaveId="panelGroup_Rules">
                      <Panel collapsible tabs={
                        <Tabs defaultValue="overview" onTabChange={(newValue: string | null) => { handleChange(newValue!); }}>
                          <Tabs.List>
                            {hasDetailsErrors ?
                              <Tabs.Tab value="overview" prefix={<FaCircle color="red" title="Overview have errors" />}>Overview</Tabs.Tab> : <Tabs.Tab value="overview">Overview</Tabs.Tab>}
                            {hasParameterErrors ?
                              <Tabs.Tab value="expressions" prefix={<FaCircle color="red" title="Expressions have errors" />}>Expressions</Tabs.Tab> : <Tabs.Tab value="expressions">Expressions</Tabs.Tab>}
                            <Tabs.Tab value="dependencies">
                              Dependencies
                            </Tabs.Tab>
                            {hasTriggerErrors ?
                              <Tabs.Tab value="triggers" prefix={<FaCircle color="red" title="Triggers have errors" />}>Triggers</Tabs.Tab> : <Tabs.Tab value="triggers">Triggers</Tabs.Tab>}
                            <Tabs.Tab value="export">Export</Tabs.Tab>
                          </Tabs.List>
                          <Tabs.Panel value="overview">
                            <Box sx={{ margin: '1rem' }}>
                              <RuleDetails rule={rule} register={register} errors={errors} />
                            </Box>
                          </Tabs.Panel>
                          <Tabs.Panel value="expressions">
                            <Box sx={{ margin: '1rem' }}>
                              <RuleFormParameters rule={rule} formContext={sharedForm} validateRule={validateRule} setHasParameterErrors={setHasParameterErrors} />
                            </Box>
                          </Tabs.Panel>
                          <Tabs.Panel value="dependencies">
                            <Box sx={{ margin: '1rem' }}>
                              <RuleFormDependencies rule={rule} formContext={sharedForm} revision={dependenciesRevision} />
                            </Box>
                          </Tabs.Panel>
                          <Tabs.Panel value="triggers">
                            <Box sx={{ margin: '1rem' }}>
                              <RuleFormTriggers rule={rule} formContext={sharedForm} validateRule={validateRule} />
                            </Box>
                          </Tabs.Panel>
                          <Tabs.Panel value="export">
                            <Box sx={{ margin: '1rem' }}>
                              <JsonEditor props={jsonEditorProps} />
                            </Box>
                          </Tabs.Panel>
                        </Tabs>} />
                      <Panel collapsible tabs={
                        <Tabs value={rightTabValue} onTabChange={(newValue: string | null) => setRightTabValue(newValue ?? "")}>
                          <Tabs.List>
                            <Tabs.Tab value="simulate">
                              Simulation
                            </Tabs.Tab>
                            <Tabs.Tab value="instances">
                              Instances
                            </Tabs.Tab>
                            {rule.isCalculatedPoint && <Tabs.Tab value="calculatedPoints">
                              Calculated Points
                            </Tabs.Tab>}
                            {!rule.isCalculatedPoint && <Tabs.Tab value="insights">
                              Insights
                            </Tabs.Tab>}
                            {!rule.isCalculatedPoint && <Tabs.Tab value="commands">
                              Commands
                            </Tabs.Tab>}
                            <Tabs.Tab value="graph">
                              Graph
                            </Tabs.Tab>
                            {hasScanError ?
                              <Tabs.Tab value="metaData" prefix={<FaCircle color="red" title={rule!.ruleMetadata!.scanError} />}>Metadata</Tabs.Tab> : <Tabs.Tab value="metaData">Metadata</Tabs.Tab>}
                          </Tabs.List>
                          <Tabs.Panel value="simulate">
                            <Box sx={{ margin: '1rem' }}>
                              <RuleSimulation ruleId={rule!.id!} equipmentId={equipmentId} showEquipmentInput={true} rule={rule} showOutputBindings={true} canAddSimulations={true} />
                            </Box>
                          </Tabs.Panel>
                          <Tabs.Panel value="instances">
                            <Box sx={{ margin: '1rem' }}>
                              <SkillDeployments ruleId={params.id!} pageId='RuleSingle' actions={(ri: RuleInstanceDto) => (
                                <Button onClick={() => {
                                  setRightTabValue("simulate");
                                  setEquipmentId(ri.equipmentId!);
                                }}>Simulate</Button>)
                              } />
                            </Box>
                          </Tabs.Panel>
                          <Tabs.Panel value="calculatedPoints">
                            <Box sx={{ margin: '1rem' }}>
                              <Stack spacing={2}>
                                {rule.isCalculatedPoint &&
                                  <Grid container spacing={1} >
                                    <Grid item>
                                      <Grid container spacing={2} alignItems="center">
                                        <Grid item>
                                          {adtEnabled === false && <Button onClick={() => enableAdtSync()}
                                            disabled={adtSyncMutation.isLoading || rule.isDraft} variant="outlined" color="secondary">
                                            Enable ADT Sync
                                          </Button>}
                                          {adtEnabled === true && <Button onClick={() => disableAdtSync()}
                                            disabled={adtSyncMutation.isLoading} variant="outlined" color="error">
                                            Disable ADT Sync
                                          </Button>}
                                        </Grid>
                                      </Grid>
                                    </Grid>
                                    <Grid item>
                                      <Button onClick={onProcessCalcPoints}
                                        disabled={isProcessingCalcPoints || rule.isDraft || !rule.adtEnabled} variant="outlined" color="secondary">
                                        Sync with ADT
                                      </Button>
                                    </Grid>
                                  </Grid>}
                                <CalculatedPointsGrid query={gridCalculatedPointsProps} />
                              </Stack>
                            </Box>
                          </Tabs.Panel>
                          <Tabs.Panel value="insights">
                            <Box sx={{ margin: '1rem' }}>
                              <Stack spacing={2}>
                                <RuleEnableSync rule={rule} />
                                <InsightsTable ruleId={params.id!} pageId='RuleSingle' />
                              </Stack>
                            </Box>
                          </Tabs.Panel>
                          <Tabs.Panel value="commands">
                            <Box sx={{ margin: '1rem' }}>
                              <Stack spacing={2}>
                                <RuleEnableSync rule={rule} />
                                <CommandsGrid query={commansGridQuery} />
                              </Stack>
                            </Box>
                          </Tabs.Panel>
                          <Tabs.Panel value="graph">
                            <Box sx={{ margin: '1rem' }}>
                              <RuleGraph rule={rule} />
                            </Box>
                          </Tabs.Panel>
                          <Tabs.Panel value="metaData">
                            <Box sx={{ margin: '1rem' }}>
                              <RuleMetadata rule={rule} />
                            </Box>
                          </Tabs.Panel>
                        </Tabs>} />
                    </PanelGroup>
                  </HeightAdjustableWrapper>
                </PageStateProvider>

                <Snackbar open={jobRequested} onClose={handleJobRequestClose} autoHideDuration={6000} >
                  <Alert onClose={handleJobRequestClose} sx={{ width: '100%' }} variant="filled">
                    <Typography>Request submitted</Typography>
                  </Alert>
                </Snackbar>
                <Snackbar open={saveCompleted} onClose={handleSaveCloseAlert} autoHideDuration={5000} >
                  <Alert onClose={handleSaveCloseAlert} variant="filled" severity={saveSeverity}>
                    {saveSuccess && <Typography variant="body1">Save successful</Typography>}
                    {!saveSuccess && <Typography variant="body1">Save failed</Typography>}
                  </Alert>
                </Snackbar>
                <Snackbar open={adtEnabledAlert} onClose={handleCloseAdtEnabledAlert} autoHideDuration={10000} >
                  <Alert onClose={handleCloseAdtEnabledAlert} variant="filled" severity={adtEnabledSeverity}>
                    <Typography>Calculated points scheduled for processing.</Typography>
                    {adtEnabled === true && <Typography><br />Once completed, refresh the ADT cache to fetch the twins.</Typography>}
                  </Alert>
                </Snackbar>

                {/*Dialog to inform user on deletion process*/}
                <Dialog open={deleteDialog} onClose={handleCloseDeleteDialog}>
                  <DialogContent>
                    <DialogContentText>
                      Skill '{rule.name}' queued for deletion.
                    </DialogContentText>
                  </DialogContent>
                  <DialogActions>
                    <Button onClick={handleCloseDeleteDialog} variant="contained" color="primary">
                      Back to skills
                    </Button>
                  </DialogActions>
                </Dialog>
              </Stack>
            </ErrorBoundary>
          )}
        </QueryErrorResetBoundary>
      </Box>
    )
  } else {
    return <div>Loading...</div>
  }
}, {
  FallbackComponent: RuleErrorFallback, //using general error view
  onError(error, info) {
    console.log('from error boundary in Rulesingle: ', error, info)
  }
})

export default RuleSingle;
