import { InsightDto } from '../../Rules';
import { Suspense } from 'react';
import TwinGraph from '../graphs/TwinGraph';
import useApi from '../../hooks/useApi';
import { useQuery } from 'react-query';

const InsightGraph = (props: { single: InsightDto }) => {
  const insight = props.single;
  const apiclient = useApi();

  const ruleInstanceQuery = useQuery(["ruleInstance", insight.id], async (s) => {
    const ruleInstance = await apiclient.getRuleInstance(insight.id);
    return ruleInstance;
  });

  return (ruleInstanceQuery.isFetched ? <Suspense fallback={<div>Loading...</div>}>
    <TwinGraph twinIds={[insight.equipmentId!]} isCollapsed={false} highlightedIds={ruleInstanceQuery.data!.pointEntityIds?.map(v => v.id!)} />
    </Suspense> : <div>Loading...</div>)
};

export default InsightGraph;
