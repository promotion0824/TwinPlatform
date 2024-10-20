import { RuleDto } from '../Rules';
import ModelGraph from './graphs/ModelGraph';
import { Box } from '@mui/material';

const RuleGraph = (params: { rule: RuleDto }) => {

  const rule = params.rule;
  const modelId = rule.twinQuery!.modelIds![0];

  return (
    <Box>
      <ModelGraph modelId={modelId} secondaryModelIds={rule.ruleMetadata?.modelIds} />
    </Box >

  );
}

export default RuleGraph;
