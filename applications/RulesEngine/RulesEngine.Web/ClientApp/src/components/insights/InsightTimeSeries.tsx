import { InsightDto } from '../../Rules';
import { useQuery } from 'react-query';
import useApi from '../../hooks/useApi';
import InsightTrends from '../chart/InsightTrends';
import { Box, Button, CircularProgress, Divider, Stack } from '@mui/material';
import RuleSimulation from '../RuleSimulation';
import { ExpandMore } from '@mui/icons-material';
import { useState } from 'react';

const InsightTimeSeries = (props: { single: InsightDto }) => {
  const insight = props.single;
  const apiclient = useApi();
  const [showSimulation, setShowSimulation] = useState<boolean>(false);
  const ruleInstanceQuery = useQuery(["ruleInstance", insight.id], async () => {
    const ruleInstance = await apiclient.getRuleInstance(insight.id);
    return ruleInstance;
  });
  const ruleQuery = useQuery(["rule", insight.id], async () => {
    const rule = await apiclient.getRule(insight.ruleId);
    //Set the JSON to empty for security reasons
    rule!.json = '';
    return rule;
  });
  let startDate = undefined;
  if (insight.occurrences!.length > 0) {
    let date = insight.occurrences![insight.occurrences!.length - 1].ended!.clone();
    startDate = date.add(-2, 'days');
  }
  return (<>{(ruleInstanceQuery.isFetched && ruleQuery.isFetched) ?
    <Stack spacing={2}>
      <InsightTrends ruleInstance={ruleInstanceQuery.data!} showSimulation={true} startDate={startDate?.toDate()} />
      <Box>
        <Button onClick={() => setShowSimulation(!showSimulation)} variant="outlined" color="secondary">
          Simulation Test <ExpandMore sx={{ fontSize: 20 }} />
        </Button>
      </Box>
      {showSimulation &&
        <Stack spacing={2}>
          <Divider textAlign="left">Execution Simulation</Divider>
          <RuleSimulation ruleId={insight.ruleId!} rule={ruleQuery.data} equipmentId={insight.equipmentId!} showEquipmentInput={false} canAddSimulations={true} />
        </Stack>
      }
    </Stack> : <>Time series pending <CircularProgress size={15} /></>}</>)
};

export default InsightTimeSeries;
