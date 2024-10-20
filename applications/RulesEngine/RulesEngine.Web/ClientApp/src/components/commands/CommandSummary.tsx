import { Card, CardContent, Stack, Typography } from '@mui/material';
import { CommandDto } from '../../Rules';
import { DateFormatter, IsTriggeredFormatter, IsTriggeredTextFormatter } from '../LinkFormatters';
import StyledLink from '../styled/StyledLink';
import { GetCommandTypeText } from './CommandTypeFormatter';

const CommandSummary = (props: { single: CommandDto }) => {
  const single = props.single;

  return (
    <Card variant="outlined" >
      <CardContent>
        <Stack spacing={2}>
          <Typography variant="body1">Status: {IsTriggeredTextFormatter(single.isTriggered!)}</Typography>
          <Typography variant="body1">Command: {single.isValid ? <>{GetCommandTypeText(single.commandType!)} = {single.value}</> : <span>-</span>}</Typography>
          <Typography variant="body1">Start time: {DateFormatter(single.startTime)}</Typography>
          <Typography variant="body1">End time:  {DateFormatter(single.endTime)}</Typography>
          <Typography variant="body1">Last Sync Date (UTC):  {DateFormatter(single.lastSyncDate)}</Typography>
          <Typography variant="body1">Skill Instance: <StyledLink to={"/ruleinstance/" + encodeURIComponent(single.ruleInstanceId!)}> {single.ruleInstanceId}</StyledLink ></Typography>

        </Stack>
      </CardContent>
    </Card>
  );
}

export default CommandSummary;
