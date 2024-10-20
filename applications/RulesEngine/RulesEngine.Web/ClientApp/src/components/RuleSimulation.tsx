import ArrowForwardIosSharpIcon from '@mui/icons-material/ArrowForwardIosSharp';
import { Accordion, AccordionDetails, Alert, Autocomplete, AutocompleteChangeDetails, AutocompleteChangeReason, Box, Button, Checkbox, CircularProgress, darken, Fade, FormControlLabel, Grid, Link, Stack, styled, TextField, Tooltip, Typography, useTheme } from '@mui/material';
import MuiAccordionSummary, {
  AccordionSummaryProps
} from '@mui/material/AccordionSummary';
import { DatePicker, LocalizationProvider } from '@mui/x-date-pickers-pro';
import { AdapterDateFns } from '@mui/x-date-pickers-pro/AdapterDateFns';
import { Icon } from '@willowinc/ui';
import moment from 'moment';
import { useEffect, useMemo, useState } from 'react';
import { useQuery } from 'react-query';
import useApi from '../hooks/useApi';
import { useStateContext } from '../providers/PageStateProvider';
import { BatchRequestDto, CommandDto, GlobalVariableDto, InsightDto, RuleDto, RuleInstanceDto, RuleSimulationRequest, SortSpecificationDto, TrendlineDto } from '../Rules';
import { VisibleIf } from './auth/Can';
import InsightTrends from './chart/InsightTrends';
import CommandOccurrences from './commands/CommandOccurrences';
import ModelPickerField from './fields/ModelPickerField';
import { ValidFormatterStatusSimple } from './LinkFormatters';
import RuleInstanceBindings from './RuleInstanceBindings';

const AccordionSummary = styled((props: AccordionSummaryProps) => (
  <MuiAccordionSummary
    expandIcon={<ArrowForwardIosSharpIcon sx={{ color: "white", fontSize: "0.9rem" }} />}
    {...props}
  />
))(({ theme }) => ({
  width: "400px",
  flexDirection: 'row-reverse',
  '& .MuiAccordionSummary-expandIconWrapper.Mui-expanded': {
    transform: 'rotate(90deg)',
  },
  '& .MuiAccordionSummary-content': {
    marginLeft: theme.spacing(1),
  },
}));

const SingleCommandOccurrences = (params: { command: CommandDto }) => {
  const command = params.command;
  const [expanded, setExpanded] = useState(false);
  return <Accordion disableGutters={true} sx={{ backgroundColor: 'transparent', backgroundImage: 'none', boxShadow: 'none' }} expanded={expanded} onChange={() => setExpanded(!expanded)}>
    <AccordionSummary>
      <Typography variant="h5">{command.commandName}</Typography>
    </AccordionSummary>
    <AccordionDetails>
      <CommandOccurrences single={command} />
    </AccordionDetails>
  </Accordion>
}

const VariableList = (params: { group: string, variables: ISimulationRequestVariable[] }) => {
  const group = params.group;
  const [variables, setVariables] = useState(params.variables);
  const [all, setAll] = useState(false);
  const [some, setSome] = useState(false);

  const updateParent = (newState: ISimulationRequestVariable[]) => {
    const allSelected = newState.every(v => v.selected);
    setAll(allSelected);

    if (!allSelected) {
      setSome(newState.some(v => v.selected));
    }
    else {
      setSome(false);
    }
  };

  const handleParentChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    variables.forEach((_, i) => {
      setVariables(oldState => {
        let newState = [...oldState];
        newState[i].selected = event.target.checked;
        updateParent(newState);
        return newState;
      });
    });
  };

  const children = (
    <Box sx={{ display: 'flex', flexDirection: 'column', ml: 3 }}>
      {variables.map((v, i) => <FormControlLabel
        key={i}
        label={`${v.name} (${v.fieldId})`}
        control={<Checkbox checked={v.selected} onChange={(event: React.ChangeEvent<HTMLInputElement>) => {
          setVariables(oldState => {
            let newState = [...oldState];
            newState[i].selected = event.target.checked;
            updateParent(newState);
            return newState;
          });
        }} />}
      />)}
    </Box>
  );

  return (
    <>
      <FormControlLabel
        label={group}
        control={
          <Checkbox
            checked={all}
            indeterminate={some}
            onChange={handleParentChange}
          />
        }
      />
      {children}
    </>
  );
}

const SelectVariables = (params: { request: ISimulationRequest, requestUpdated: (request: ISimulationRequest) => void, requestDeleted: (request: ISimulationRequest) => void }) => {
  const request = params.request;
  const [show, setShow] = useState(request.visible);

  return (
    <Accordion disableGutters={true} sx={{ backgroundColor: 'transparent', backgroundImage: 'none', boxShadow: 'none' }}
      expanded={show} onChange={() => setShow(!show)} square={true}>
      <AccordionSummary>
        <Typography variant="body1">{request.name}</Typography>
      </AccordionSummary>
      <AccordionDetails>
        <Grid container>
          <Grid item xs={6}>
            <VariableList
              group={"Capabilities"}
              variables={request.variables.filter(v => !v.isImpactScore)} />
          </Grid>
          <Grid item xs={6}>
            <VariableList
              group={"Impact Scores"}
              variables={request.variables.filter(v => v.isImpactScore)} />
          </Grid>
          <Grid item xs={12}>
            <Box>
              <Stack direction="row" spacing={1}>
                <Button variant="contained" color="primary" onClick={() => {
                  params.requestUpdated(request);
                  setShow(false);
                }}>OK</Button>
                <Button variant="contained" color="error" onClick={() => {
                  params.requestDeleted(request);
                  setShow(false);
                }}>Remove</Button>
              </Stack>
            </Box>
          </Grid>
        </Grid>
      </AccordionDetails>
    </Accordion>
  );
}

interface ISimulationRequestVariable {
  name: string,
  fieldId: string,
  selected: boolean,
  isImpactScore: boolean
}

interface ISimulationRequest {
  name: string,
  id: string,
  group: number,
  equipmentId: string,
  rule: RuleDto,
  visible: boolean,
  variables: ISimulationRequestVariable[]
};

const GroupHeader = styled('div')(({ theme }) => ({
  position: 'sticky',
  top: '-8px',
  padding: '4px 10px',
  color: theme.palette.primary.main,
  backgroundColor: darken(theme.palette.primary.main, 0.8),
}));

const GroupItems = styled('ul')({
  padding: 0,
});

const RuleSimulation = (props: {
  ruleId: string,
  equipmentId: string,
  showEquipmentInput: boolean,
  rule?: RuleDto,
  useExistingData?: boolean,
  showOutputBindings?: boolean,
  startDate?: Date,
  showDates?: boolean,
  minStartDate?: Date,
  globalVariable?: GlobalVariableDto,
  showModelInput?: boolean,
  showInsights?: boolean,
  canAddSimulations?: boolean,
  canAddRelatedRulesSimulations?: boolean
}) => {
  const theme = useTheme();
  const apiclient = useApi();
  const defaultStartDate = new Date();
  const defaultEndDate = new Date();
  const rule = props.rule;
  const ruleId = props.ruleId;
  const globalVariable = props.globalVariable;
  const showEquipmentInput = props.showEquipmentInput;
  const canAddSimulations = props.canAddSimulations ?? false;
  const canAddRelatedRulesSimulations = props.canAddRelatedRulesSimulations ?? true;
  const useExistingData = props.useExistingData ?? false;
  const showOutputBindings = props.showOutputBindings ?? false;
  const showDates = props.showDates ?? true;
  const showInsights = props.showInsights ?? true;
  const showInsightResults = useExistingData == false && showInsights;
  const showLabel = useExistingData == false;
  const showModelInput = props.showModelInput ?? false;
  const showModelList = showModelInput && showEquipmentInput && props.equipmentId == "";

  const buttonLabel = useExistingData ? "Refresh" : "Run Simulation";
  const progressLabel = useExistingData ? "Loading" : "Running Simulation";
  const startDateLabel = useExistingData ? "Start date" : "Simulation Start date";
  const endDateLabel = useExistingData ? "End date" : "Simulation End date";

  defaultStartDate.setDate(defaultStartDate.getDate() - 2);
  defaultEndDate.setDate(defaultEndDate.getDate());

  const [startDate, setStartDate] = useState<Date | null>(props.startDate ?? defaultStartDate);
  const [endDate, setEndDate] = useState<Date | null>(defaultEndDate);
  const [equipmentId, setEquipmentId] = useState<string>(props.equipmentId);
  const [equipmentValue, setEquipmentValue] = useState<any | null>(null);
  const [simulationStarted, setSimulationStarted] = useState<boolean>(false);
  const [ruleInstance, setRuleInstance] = useState<RuleInstanceDto | undefined>(undefined);
  const [insight, setInsight] = useState<InsightDto | undefined>(undefined);
  const [commands, setCommands] = useState<CommandDto[]>([]);
  const [error, setError] = useState<string | undefined>("");
  const [warning, setWarning] = useState<string | undefined>("");
  const [simulationDone, setSimulationDone] = useState<boolean>(false);
  const [loaded, setLoaded] = useState<boolean>(false);
  const [timeSeriesData, setTimeSeriesData] = useState<any>({ data: [] });
  const [more, showMore] = useState(false);
  const [enableCompression, setEnableCompression] = useState(true);
  const [optimizeCompression, setOptimizeCompression] = useState(true);
  const [generatePointTracking, setGeneratePointTracking] = useState(false);
  const [optimizeExpressions, setOptimizeExpressions] = useState(true);
  const [skipMaxPointLimit, setSkipMaxPointLimit] = useState(false);
  const [ruleInstanceListId, setRuleInstanceListId] = useState(props.ruleId);
  const [simulationRequests, setSimulationRequests] = useState<ISimulationRequest[]>([]);
  const [selectedRequests, setSelectedRequests] = useState<ISimulationRequest[]>([]);
  const [expanded, setExpanded] = useState(false);
  const [showAutoVariables, setShowAutoVariables] = useState(false);
  const [applyLimits, setApplyLimits] = useState(false);
  const [revision, setRevision] = useState(0);

  let minDate = props.minStartDate;

  if (minDate === undefined) {
    minDate = new Date();
    minDate.setDate(minDate.getDate() - 365);//1 year max
  }

  const mergeResponse = (existingData: any, request: ISimulationRequest, timeSeriesDataForRequest: any) => {
    existingData.trendlines = existingData.trendlines.filter((v: any) => !v.id!.startsWith(`${request.name}-`));

    request.variables.filter(v => v.selected === true).forEach(v => {
      var trend = timeSeriesDataForRequest.trendlines!.find((t: any) => v.fieldId == t.id);

      if (trend === undefined) {
        trend = new TrendlineDto({ name: `${v.fieldId} (*No Data)`, id: v.fieldId, data: [] });
      }
      else {
        const axis = timeSeriesDataForRequest.axes!.find((v: any) => v.shortName == trend!.axis);
        if (axis) {
          const existingAxis = existingData.axes!.find((v: any) => v.key == axis!.key);

          if (existingAxis !== undefined) {
            trend.axis = existingAxis.shortName;
          }
          else {
            axis!.shortName = `y${existingData.axes.length + 1}`;
            axis!.longName = `yaxis${existingData.axes.length + 1}`;
            trend.axis = axis!.shortName;
            existingData.axes.push(axis);
          }
        }
        else {
          //the axis has already been moved from a previous trend line
          const existingAxis = existingData.axes!.find((v: any) => v.shortName == trend!.axis);
          if (existingAxis) {
            trend.axis = existingAxis!.shortName;
          }
        }
      }

      trend.id = `${request.name}-${trend.id}`;
      trend.name = `${request.name}<br>${trend.name}`;

      const index = existingData.trendlines.findIndex((v1: any) => v1.id == trend!.id);

      if (index >= 0) {
        existingData.trendlines[index] = trend;
      }
      else {
        existingData.trendlines.push(trend);
      }
    });

    existingData.axes = existingData.axes.filter((v: any) => existingData.trendlines.findIndex((t: any) => t.axis == v.shortName) >= 0);
  }

  const runSimulation = async (updateExistingData: boolean, customRequest?: ISimulationRequest) => {
    if (simulationStarted) {
      return;
    }

    setLoaded(true);
    setSimulationStarted(true);
    setSimulationDone(false);
    setTimeSeriesData({ data: [] });
    setError("");
    setWarning("");

    try {
      const createRequest = (simRequest?: ISimulationRequest): RuleSimulationRequest => {
        const selectedRule = simRequest?.rule ?? rule;
        const request = new RuleSimulationRequest();
        request.ruleId = simRequest?.rule?.id ?? ruleId;
        request.equipmentId = simRequest?.equipmentId ?? equipmentId;
        request.startTime = moment(startDate);
        request.endTime = moment(endDate);
        request.useExistingData = useExistingData;
        request.updateRule = selectedRule !== undefined;
        request.enableCompression = enableCompression;
        request.optimizeCompression = optimizeCompression;
        request.rule = selectedRule !== undefined ? selectedRule : new RuleDto();
        request.global = globalVariable;
        request.generatePointTracking = generatePointTracking;
        request.showAutoVariables = showAutoVariables;
        request.optimizeExpression = optimizeExpressions;
        request.skipMaxPointLimit = skipMaxPointLimit;
        request.applyLimits = applyLimits;
        return request;
      }

      const request = createRequest(customRequest);

      const result = await apiclient.executeSimulationForRule(request);

      if (!showInsights && result.timeSeriesData) {
        result.timeSeriesData.insights = [];
      }

      if (updateExistingData && customRequest) {
        if (!result.error) {
          mergeResponse(timeSeriesData, customRequest, result.timeSeriesData!);
        }
        setTimeSeriesData(timeSeriesData);
      }
      else {
        try {
          for (var i = 0; i < selectedRequests.length; i++) {
            const customRequest = selectedRequests[i];
            const customResult = await apiclient.executeSimulationForRule(createRequest(customRequest));
            if (!customResult.error) {
              mergeResponse(result.timeSeriesData, customRequest, customResult.timeSeriesData!);
            }
          }
        }
        catch (e) {
        }

        setRuleInstance(result.ruleInstance);
        setTimeSeriesData(result.timeSeriesData);
        setRuleInstance(result.ruleInstance);

        if (showInsights) {
          setInsight(result.insight);
        }
        setCommands(result.commands ?? []);
        setRevision(revision + 1);
      }

      setError(result.error);
      setWarning(result.warning);

      setSimulationStarted(false);
      setSimulationDone(true);

    }
    catch (e) {
      setSimulationStarted(false);
      setSimulationDone(true);
      setError((e as string) ?? "An error occurred during your request");
    }
  };

  const equipmentQuery = useQuery(
    ['ruleInstanceList', ruleInstanceListId],
    () => {
      return apiclient.getRuleInstanceList(ruleInstanceListId);
    },
    {
      enabled: (showEquipmentInput && (ruleInstanceListId?.length ?? 0) > 0) || canAddSimulations
    });

  const rulesQuery = useQuery(
    ['rulesForSimulation'],
    async () => {

      const sort = new SortSpecificationDto();
      sort.field = "Name";
      sort.sort = "ASC";

      let request = new BatchRequestDto({ sortSpecifications: [sort] });

      var rules = await apiclient.rules(request);

      rules.items?.forEach(v => {
        v.json = '';
      });

      return rules;
    },
    {
      enabled: canAddSimulations
    });

  const removeRequest = (request: ISimulationRequest) => {
    setTimeSeriesData((prevState: any) => {
      if (prevState) {
        const trendlines = prevState.trendlines.filter((v: any) => !v.id!.startsWith(`${request.name}-`));
        const axes = prevState.axes.filter((v: any) => trendlines.findIndex((t: any) => t.axis == v.shortName) >= 0);
        return { ...prevState, axes: axes, trendlines: trendlines };
      }

      return prevState;
    });

    setSimulationDone(() => {
      setTimeout(() => setSimulationDone(true), 500);
      return false;
    });
    const requests = selectedRequests.filter(v => v != request);
    setSelectedRequests(requests);
    setError("");
    setWarning("");
  };

  const createSimulationRequest = (name: string, id: string, group: number, equipmentId: string, rule: RuleDto): ISimulationRequest => {
    var variables = rule.parameters?.map(v => {
      return {
        name: v.name,
        fieldId: v.fieldId,
        selected: false,
        isImpactScore: false
      } as ISimulationRequestVariable;
    }) ?? [];

    variables = variables.concat(rule.impactScores?.map(v => {
      return {
        name: v.name,
        fieldId: v.fieldId,
        selected: false,
        isImpactScore: true
      } as ISimulationRequestVariable;
    }) ?? []);

    return {
      name: name,
      id: id,
      equipmentId: equipmentId,
      rule: rule,
      group: group,
      variables: variables,
      visible: false
    }
  };

  //for existing data run at startup
  useMemo(() => {
    if (useExistingData) {
      runSimulation(false);
    }
  }, [useExistingData]);

  useEffect(() => {
    if (equipmentQuery.isFetched && rulesQuery.isFetched && revision > 0) {

      var requests: ISimulationRequest[] = [];

      ruleInstance?.ruleDependenciesBound!.forEach(v => {
        var existingRule = rulesQuery.data!.items?.find(r => r.id == v.ruleId);
        if (existingRule !== undefined) {
          requests.push(createSimulationRequest(`${v.twinName ?? v.twinId!} (${v.ruleName})`, `${v.twinId!}_${existingRule.id}`, 1, v.twinId!, existingRule));
        }
      });

      if (canAddRelatedRulesSimulations) {
        rulesQuery.data!.items!.filter(v => v.primaryModelId == rule?.primaryModelId ?? "").forEach(v => {
          requests.push(createSimulationRequest(v.name!, v.id!, 2, equipmentId, v));
        });
      }

      if (rule != undefined) {
        equipmentQuery.data!.forEach(v => {
          const name = rule.name !== undefined ? `${v.equipmentName!} (${rule.name})` : v.equipmentName!;
          requests.push(createSimulationRequest(name, v.id!, 3, v.equipmentId!, rule!));
        });
      }

      setSimulationRequests(requests);
    }
  }, [revision]);

  useEffect(() => {
    if (props.equipmentId && props.equipmentId != equipmentId) {
      setEquipmentId(props.equipmentId);
      setEquipmentValue(equipmentQuery.data?.find(v => v.equipmentId == props.equipmentId) ?? null);
    }
  }, [props.equipmentId]);

  const { pageState, setPageState } = useStateContext();

  return (
    <Box flexGrow={1}>
      <Stack spacing={2}>
        {showModelList &&

          <Grid container spacing={1}>
            <Grid item xs={4}>
              <ModelPickerField
                rule={rule ?? new RuleDto()}
                isRequired={false}
                primaryModelIdChanged={(id) => {
                  if (ruleInstanceListId != id && (id?.length ?? 0) > 0) {
                    setRuleInstanceListId(id ?? "");
                  }
                }}
              />
            </Grid>
          </Grid>
        }
        <Box flexGrow={1}>
          <Grid container spacing={1}>

            {showEquipmentInput && <Grid item xs={8}>

              {(showEquipmentInput && equipmentQuery.isFetching) ? <>Loading Equipment List, please wait <CircularProgress /></> :

                (equipmentQuery!.data?.length! > 0) ?
                  <Autocomplete
                    freeSolo
                    id="simulation-equipment-list"
                    options={equipmentQuery.data ?? []}
                    value={equipmentValue}
                    isOptionEqualToValue={(a: any, b: any) => (a.id == b.id)}
                    getOptionLabel={(option) => {
                      if (typeof option === 'string') {
                        return option;
                      }
                      return `${option.equipmentName} (${option.equipmentId})`;
                    }}
                    renderOption={(props, option) => <li {...props}>{ValidFormatterStatusSimple(option.status!)}&nbsp; {option.equipmentName} ({option.equipmentId})</li>}
                    onChange={(_, newValue: any) => {
                      setEquipmentId(newValue?.equipmentId ?? "");
                    }}
                    renderInput={(params) => <TextField {...params} label="Equipment Id" />}
                  /> :
                  <TextField
                    defaultValue={"No equipment available..."}
                    fullWidth
                    autoComplete='off'
                    placeholder="No equipment available..."
                    label="Equipment Id"
                    disabled={true}
                    InputProps={{
                      readOnly: true,
                    }}
                  />
              }
            </Grid>}

            {showDates && <Grid item xs={2}>
              <LocalizationProvider dateAdapter={AdapterDateFns}>
                <DatePicker
                  label={startDateLabel}
                  value={startDate}
                  minDate={minDate}
                  maxDate={new Date()}
                  sx={{ width: '100%' }}
                  onChange={() => undefined}
                  onAccept={(newValue) => {
                    setStartDate(newValue);
                  }}
                />
              </LocalizationProvider></Grid>}
            {showDates && <Grid item xs={2}>
              <LocalizationProvider dateAdapter={AdapterDateFns}>
                <DatePicker
                  label={endDateLabel}
                  value={endDate}
                  minDate={minDate}
                  maxDate={new Date()}
                  sx={{ width: '100%' }}
                  onChange={() => undefined}
                  onAccept={(newValue) => {
                    setEndDate(newValue);
                  }}
                />
              </LocalizationProvider>
            </Grid>}
            <VisibleIf canViewAdminPage>
              <Grid item xs={12}>
                <Typography> <Link
                  component="button"
                  variant="body2"
                  onClick={(e: any) => {
                    showMore(!more);
                    e.preventDefault();
                  }}
                >
                  {!more ? "+ Options" : "- Options"}
                </Link></Typography>
              </Grid>
              {more &&
                <Grid item xs={12}>
                  <FormControlLabel control={<Checkbox
                    checked={generatePointTracking}
                    onChange={() => setGeneratePointTracking(!generatePointTracking)}
                    color="primary"
                    inputProps={{ 'aria-label': 'primary checkbox' }}
                  />} label="Show Point Data" title="Show calculated values for each point value as a tooltip" />
                  <FormControlLabel control={<Checkbox
                    checked={showAutoVariables}
                    onChange={() => setShowAutoVariables(!showAutoVariables)}
                    color="primary"
                    inputProps={{ 'aria-label': 'primary checkbox' }}
                  />} label="Show Auto Generated Variables" title="Show auto generated variables in the time series" />
                </Grid>}
              {more &&
                <Grid item xs={12}>
                  <FormControlLabel control={<Checkbox
                    checked={enableCompression}
                    onChange={() => setEnableCompression(!enableCompression)}
                    color="primary"
                    inputProps={{ 'aria-label': 'primary checkbox' }}
                  />} label="Enable Compression" title="Indicates whether compression is applied to the timeseries" />
                  <FormControlLabel control={<Checkbox
                    checked={optimizeCompression}
                    onChange={() => setOptimizeCompression(!optimizeCompression)}
                    color="primary"
                    inputProps={{ 'aria-label': 'primary checkbox' }}
                  />} label="Optimize Compression" title="Indicates whether more aggressive compression is used for older values" />
                  <FormControlLabel control={<Checkbox
                    checked={applyLimits}
                    onChange={() => setApplyLimits(!applyLimits)}
                    color="primary"
                    inputProps={{ 'aria-label': 'primary checkbox' }}
                  />} label="Apply Limits" title="Prune buffers to the correct sizes during execution" />
                  <FormControlLabel control={<Checkbox
                    checked={optimizeExpressions}
                    onChange={() => setOptimizeExpressions(!optimizeExpressions)}
                    color="primary"
                    inputProps={{ 'aria-label': 'primary checkbox' }}
                  />} label="Simplify Expressions" title="Remove unnecessary constant expressions before execution" />
                  <FormControlLabel control={<Checkbox
                    checked={skipMaxPointLimit}
                    onChange={() => setSkipMaxPointLimit(!skipMaxPointLimit)}
                    color="primary"
                    inputProps={{ 'aria-label': 'primary checkbox' }}
                  />} label="Skip point limits" title="Skip max point limit validation" />
                </Grid>}
            </VisibleIf>
          </Grid>
        </Box>
        {showDates &&
          <Box>
            <Button variant="contained" onClick={() => { runSimulation(false); if (setPageState) setPageState(false); }}>{pageState ? <Icon icon="refresh" /> : null}{buttonLabel}</Button>
          </Box>}

        {(canAddSimulations && ((simulationRequests?.length ?? 0) > 0)) &&
          <Box>
            <Stack spacing={1}>
              {(selectedRequests.length > 0) && <Typography variant="h4">Added Simulations:</Typography>}
              <Grid container>
                <Grid item>
                  {selectedRequests.map((v, i) => (<SelectVariables
                    key={i}
                    request={v}
                    requestUpdated={(request) => {
                      runSimulation(true, request);
                    }}
                    requestDeleted={(request) => {
                      removeRequest(request);
                    }}
                  />))}
                </Grid>
              </Grid>
              <Grid container>
                <Grid item xs={12} sm={6} mb={1}>
                  <Autocomplete
                    id="simulation-requests-list"
                    options={simulationRequests.sort((a, b) => a.group - b.group) ?? []}
                    isOptionEqualToValue={(a: any, b: any) => (a.id == b.id)}
                    groupBy={(option) => option.group.toString()}
                    getOptionLabel={(option) => {
                      return `${option.name} (${option.id})`;
                    }}
                    renderOption={(props, option) => <li {...props}>{option.name}</li>}
                    onChange={(_, newValue: any, reason: AutocompleteChangeReason, details: AutocompleteChangeDetails<ISimulationRequest> | undefined) => {
                      if (reason == "removeOption" && details) {
                        removeRequest(details.option);
                      }
                      else {
                        if (!selectedRequests.some(v => v.id == newValue.id)) {
                          const requests = [...selectedRequests];
                          newValue.visible = true;
                          requests.push(newValue);
                          setSelectedRequests(requests);
                        }
                      }
                    }}
                    renderGroup={(params) => (
                      <li key={params.key}>
                        <GroupHeader>{(params.group == 1 ? "Dependencies" : (params.group == 2 ? "Sibling Skills" : "Equipment"))}</GroupHeader>
                        <GroupItems>{params.children}</GroupItems>
                      </li>
                    )}
                    renderInput={(params) => <TextField {...params} label="Select simulation" />}
                  />
                </Grid>
              </Grid>
            </Stack>
          </Box>}

        <Box flexGrow={1}>
          <Stack spacing={2}>
            {simulationDone === false &&
              <Box
                display="flex"
                justifyContent="center"
                alignItems="center"
                minHeight="10vh"
              >
                {loaded === false && <>Click the '{buttonLabel}' button to draw results</>}
                {(simulationStarted === true) && <>{progressLabel} &nbsp;<CircularProgress size={14} /></>}
                {(error !== null && error!?.length > 0) && <Alert severity="error">{error}</Alert>}
              </Box>
            }
            {(simulationDone === true && showLabel) && <Typography variant="h3">Simulation Results:</Typography>}
            {(simulationDone === true && warning !== null && warning!?.length > 0) &&
              <Alert severity="warning">
                Warning: {warning}
              </Alert>}
            {simulationDone === true && error !== null && error!?.length > 0 && <Alert severity="error" onClose={() => { setError(""); }}><>{error!.split('\n').map((x, i) => <p key={i}>{x}</p>)}</></Alert>}

            {(simulationDone === true && timeSeriesData) && <InsightTrends ruleInstance={ruleInstance!} showSimulation={false} existingTimeSeriesData={timeSeriesData} />}

            {(simulationDone === true && insight && showInsightResults) &&
              <Box flexGrow={1}>
                <Stack spacing={0.5}>
                  <Grid container>
                    {insight.impactScores?.map((x, i) => <Grid container direction={'row'} key={i}><Grid item xs={3}>{x.name}</Grid><Grid item xs={3}>{x.score!.toFixed(2)} {x.unit}</Grid></Grid>)}
                  </Grid>
                  <Typography>Description:<br/> {insight?.text}</Typography>
                  <Typography>Recommendation:<br /> {insight?.recommendations}</Typography>

                  <Accordion disableGutters={true} sx={{ backgroundColor: 'transparent', backgroundImage: 'none', boxShadow: 'none' }} expanded={expanded} onChange={() => setExpanded(!expanded)}>
                    <AccordionSummary>
                      <Typography variant="h4">Occurrences</Typography>
                    </AccordionSummary>
                    <AccordionDetails>
                      <Stack spacing={0.5}>
                        {insight.occurrences && insight.occurrences?.slice(0).reverse().map((oc, j) =>
                          <Grid key={j} container direction="row" alignItems="top" style={{ color: !oc.isValid ? theme.palette.warning.light : oc.isFaulted ? theme.palette.error.light : theme.palette.success.light }}>
                            <Grid item xs={2}>
                              {oc.started?.format('L HH:mm:ss')}
                            </Grid>
                            <Grid item xs={2}>
                              {oc.ended?.format('L HH:mm:ss')}
                            </Grid>
                            <Grid item xs={8}>
                              {oc.text?.split('\n').map((x, i) => <div key={i}>{x}</div>)}
                            </Grid>
                          </Grid>
                        )}
                      </Stack>
                    </AccordionDetails>
                  </Accordion>

                  {commands?.map((v, i) => <SingleCommandOccurrences key={i} command={v} />)}
                </Stack>
              </Box>
            }

            {(simulationDone === true && showOutputBindings && ruleInstance) &&
              <RuleInstanceBindings ruleInstance={ruleInstance} pageId={"Simulation"} />
            }
          </Stack>
        </Box>
      </Stack>
    </Box>
  )
}

export default RuleSimulation;
