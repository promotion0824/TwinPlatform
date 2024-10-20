import { Box, Button, CircularProgress, Grid, Tooltip } from '@mui/material';
import { useEffect, useState } from 'react';
import { useMutation, useQueryClient } from 'react-query';
import useApi from '../hooks/useApi';
import { RuleDto } from '../Rules';

export const RuleEnableSync = (params: { rule: RuleDto }) => {
  const apiclient = useApi();
  const queryClient = useQueryClient();
  const rule = params.rule;

  const [commandEnabled, setCommandEnabled] = useState(false);
  useEffect(() => {
    setCommandEnabled(rule?.commandEnabled === true);
  }, [rule?.commandEnabled]);

  const syncCommandMutation = useMutation(async (data: boolean) => {
    await apiclient.enabledInsightToCommand(rule.id, data);
    queryClient.invalidateQueries(["insights"]);
    queryClient.invalidateQueries(["commands"]);
    queryClient.invalidateQueries(["rule", rule.id]);
  });

  const enableSync = async () => {
    await syncCommandMutation.mutateAsync(true);
    setCommandEnabled(true);
  }

  const disableSync = async () => {
    await syncCommandMutation.mutateAsync(false);
    setCommandEnabled(false);
  }

  return (
    <Box flexGrow={1}>
      <Grid container spacing={2} alignItems="center">
        <Grid item>
          {commandEnabled === false && <Tooltip title="Enable skill to sync Insights and Commands"><Button onClick={() => enableSync()}
            disabled={syncCommandMutation.isLoading} variant="outlined" color="secondary">
            Enable
          </Button></Tooltip>}
          {commandEnabled === true && <Tooltip title="Disable skill to sync Insights and Commands"><Button onClick={() => disableSync()}
            disabled={syncCommandMutation.isLoading} variant="outlined" color="error">
            Disable
          </Button></Tooltip>}
        </Grid>
        <Grid item>
          {syncCommandMutation.isLoading && <CircularProgress size={20} />}
        </Grid>
      </Grid>
    </Box>
  );
};
