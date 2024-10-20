import { Grid, Stack } from '@mui/material';
import FlexTitle from '../components/FlexPageTitle';
import CommandsGrid from '../components/grids/CommandsGrid';
import useApi from '../hooks/useApi';
import { BatchRequestDto } from '../Rules';

const CommandsPage = () => {
  const apiclient = useApi();

  const commansGridQuery = {
    invokeQuery: (request: BatchRequestDto) => {
      return apiclient.getCommandsAfter("none", request);
    },
    downloadCsv: (request: BatchRequestDto) => {
      return apiclient.exportCommandsAfter("none", request);
    },
    key: 'none',
    pageId: 'Commands'
  };

  return (
    <Stack spacing={2}>
      <Grid container>
        <Grid item xs={12} md={4}>
          <FlexTitle>
            Commands
          </FlexTitle>
        </Grid>
      </Grid>
      <CommandsGrid query={commansGridQuery} />
    </Stack>
  );
}

export default CommandsPage;
