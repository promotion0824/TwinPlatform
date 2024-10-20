import { CommandDto } from '../../Rules';
import { Suspense } from 'react';
import TwinGraph from '../graphs/TwinGraph';
import useApi from '../../hooks/useApi';
import { useQuery } from 'react-query';

const CommandGraph = (props: { single: CommandDto }) => {
  const command = props.single;
  const apiclient = useApi();

  const ruleInstanceQuery = useQuery(["ruleInstance", command.ruleInstanceId], async (s) => {
    const ruleInstance = await apiclient.getRuleInstance(command.ruleInstanceId);
    return ruleInstance;
  });

  return (ruleInstanceQuery.isFetched ? <Suspense fallback={<div>Loading...</div>}>
    <TwinGraph twinIds={[command.equipmentId!]} isCollapsed={false} highlightedIds={ruleInstanceQuery.data!.pointEntityIds?.map(v => v.id!)} />
    </Suspense> : <div>Loading...</div>)
};

export default CommandGraph;
