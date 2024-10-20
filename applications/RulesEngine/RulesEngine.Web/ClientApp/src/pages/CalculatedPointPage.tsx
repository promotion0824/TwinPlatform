import { ExpandMore } from '@mui/icons-material';
import { Box, Button, Card, CardContent, Divider, Stack, Typography, useTheme } from '@mui/material';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useQuery } from 'react-query';
import { useParams } from 'react-router-dom';
import CalculatedPointTrends from '../components/chart/CalculatedPointTrends';
import ExpressionParameters from '../components/formparts/ExpressionParameters';
import OutputValues from '../components/grids/OutputValues';
import PointEntitiesTable from '../components/grids/PointEntitiesTable';
import { ADTActionStatusLookup } from '../components/Lookups';
import RuleSimulation from '../components/RuleSimulation';
import { NoSpecialCharacters } from '../components/StringOptions';
import StyledLink from '../components/styled/StyledLink';
import useApi from '../hooks/useApi';
import { DateFormatter } from '../components/LinkFormatters';
import { CalculatedPointSource, RuleDto, RuleParameterDto } from '../Rules';
import FlexTitle from '../components/FlexPageTitle';

const CalculatedPointPage = () => {
  const theme = useTheme();
  const params = useParams<{ id: string }>();
  const apiclient = useApi();
  const form = useForm();
  const { register, formState: { errors } } = form;
  const [showSimulation, setShowSimulation] = useState<boolean>(false);
  const [rule, setRule] = useState<RuleDto>();
  const [cpType, setCpType] = useState<CalculatedPointSource>();

  const cpQuery = useQuery(["calculatedPoint", params.id], async (_x: any) => {
    const data = await apiclient.getCalculatedPoint(params.id);

    setCpType(data.source);

    if (data.source == CalculatedPointSource._0) {
      const ruleParameter = new RuleParameterDto(
        {
          pointExpression: data.valueExpression,
          fieldId: "result",
          name: params.id!
        });

      setRule(new RuleDto({ id: params.id, parameters: [ruleParameter], templateId: "calculated-point" }));
    }

    return data;
  });

  const cp = cpQuery.data;

  if (cpQuery.isLoading) return <div>Loading...</div>;
  if (cpQuery.isError || !cp) return <div>Not Found...</div>;

  const updateParameters = (parameters: RuleParameterDto[]) => {
    const newRule: RuleDto = new RuleDto();
    newRule.init({ ...rule, parameters: parameters });
    setRule(newRule);
  };

  const getFormErrors = () => errors;
  const getFormRegister = () => register;

  return (
    <Stack spacing={2}>
      <FlexTitle>
        <StyledLink to={"/calculatedPoints"}>Calculated Points</StyledLink>
        {cp.name}
      </FlexTitle>
      <Card sx={{ backgroundColor: theme.palette.background.paper }} >
        <CardContent>
          <Stack spacing={2}>
            {(cp.actionStatus != 0 && cp.actionStatus != 2) && <Typography variant="body1">ADT Status: {ADTActionStatusLookup.getStatusString(cp?.actionStatus!)}</Typography>}
            {(cp.actionStatus == 0 || cp.actionStatus == 2) && <Typography variant="body1">Id: <StyledLink to={"/equipment/" + encodeURIComponent(cp.id!)}>{cp.id}</StyledLink></Typography>}
            {cp.ruleId && <Typography variant="body1">Skill: <StyledLink to={"/rule/" + encodeURIComponent(cp.ruleId!)}>{cp.ruleId!}</StyledLink></Typography>}
            {cp.isCapabilityOf && <Typography variant="body1">
              Equipment: <StyledLink to={"/equipment/" + encodeURIComponent(cp.isCapabilityOf!)}>{cp.isCapabilityOf}</StyledLink>
            </Typography>}
            <Typography variant="body1">
              Capability: <StyledLink to={"/equipment/" + encodeURIComponent(cp.id!)}>{cp.id}</StyledLink>
            </Typography>
            <Typography variant="body1">Skill Instance: <StyledLink to={"/ruleinstance/" + encodeURIComponent(cp.id!)}> {cp.id}</StyledLink ></Typography>
            <Typography variant="body1">Description: {cp.description ?? "-"}</Typography>
            {cp.valueExpression && <Typography variant="body1">Expression: {cp.valueExpression}</Typography>}
            {((cp.triggerCount ?? 0) < 1) ?
              <Typography variant="body1">No data received yet</Typography> :
              <Typography variant="body1">Received {cp.triggerCount} triggers, last was at {cp.lastTriggered?.format('L LTS')}</Typography>
            }
            <Typography variant="body1">Last Sync'd (UTC): {cp.lastSyncDateUTC ? DateFormatter(cp.lastSyncDateUTC) : "-"}</Typography>
          </Stack>
        </CardContent>
      </Card>

      <CalculatedPointTrends calculatedPoint={cp} namedPoints={cp.pointEntityIds!} />

      {((cp.outputValues ?? []).length > 0) && <>
        <Typography variant="h4">Output log</Typography>
        <OutputValues props={{ outputValues: cp.outputValues, key: NoSpecialCharacters(cp.id!), pageId: 'CalculatedPoint' }} />
      </>}

      <Box flexGrow={1}>
        <Stack spacing={2} mb={3}>
          <Box>
            <Button onClick={() => setShowSimulation(!showSimulation)} variant="outlined" color="secondary">
              Simulation Test <ExpandMore sx={{ fontSize: 20 }} />
            </Button>
          </Box>
          {showSimulation &&
            <Stack spacing={2}>
              {cpType == CalculatedPointSource._0 &&
                <ExpressionParameters
                  parameters={rule?.parameters!}
                  allParams={[]}
                  label={"Connect capabilities to rule"}
                  showUnits={false}
                  showField={true}
                  showSettings={true}
                  canRename={false}
                  updateParameters={updateParameters}
                  updateAllParams={()=>{}}
                  getFormErrors={getFormErrors}
                  getFormRegister={getFormRegister}
                />
              }
              <Divider textAlign="left">Execution Simulation</Divider>
              <RuleSimulation ruleId={cpType == CalculatedPointSource._0 ? rule?.id! : cp?.ruleId!}
                equipmentId={cpType == CalculatedPointSource._0 ? params.id! : cp?.isCapabilityOf!} showEquipmentInput={false} rule={rule} />
            </Stack>
          }
        </Stack>
      </Box>
    </Stack>
  );
}

export default CalculatedPointPage;
