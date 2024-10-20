import { CommandDto } from '../../Rules';
import { useQuery } from 'react-query';
import useApi from '../../hooks/useApi';
import InsightTrends from '../chart/InsightTrends';
import { Box, Button, CircularProgress, Divider, Stack } from '@mui/material';
import RuleSimulation from '../RuleSimulation';
import { ExpandMore } from '@mui/icons-material';
import { useState } from 'react';

const CommandTimeSeries = (props: { single: CommandDto }) => {
  const single = props.single;
  const apiclient = useApi();
  const [showSimulation, setShowSimulation] = useState<boolean>(false);

  const ruleInstanceQuery = useQuery(["ruleInstance", single.ruleInstanceId], async (s) => {
    const ruleInstance = await apiclient.getRuleInstance(single.ruleInstanceId);
    return ruleInstance;
  });

  return (<>{ruleInstanceQuery.isFetched ?
    <Stack spacing={2}>
      <InsightTrends ruleInstance={ruleInstanceQuery.data!} showSimulation={true} />
      <Box>
        <Button onClick={() => setShowSimulation(!showSimulation)} variant="outlined" color="secondary">
          Simulation Test <ExpandMore sx={{ fontSize: 20 }} />
        </Button>
      </Box>
      {showSimulation &&
        <Stack spacing={2}>
          <Divider textAlign="left">Execution Simulation</Divider>
          <RuleSimulation ruleId={single.ruleId!} equipmentId={single.equipmentId!} showEquipmentInput={false} />
        </Stack>
      }
    </Stack> : <>Time series pending <CircularProgress size={15} /></>}</>)
};

export default CommandTimeSeries;
