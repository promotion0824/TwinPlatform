import { Card, CardContent, Grid, Stack, Typography, useTheme } from '@mui/material';
import { CommandDto } from '../../Rules';
import { IsTriggeredFormatter } from '../LinkFormatters';
import { GetCommandTypeText } from './CommandTypeFormatter';

const CommandOccurrences = (props: { single: CommandDto }) => {
  const theme = useTheme();
  const { occurrences } = props.single;

  return (
    <Card variant="outlined">
      <CardContent>
        <Stack spacing={0.5}>
          <Grid container direction="row" alignItems="top">
            <Grid item xs={1}>
              Set Start time
            </Grid>
            <Grid item xs={1}>
              Set End time
            </Grid>
            <Grid item xs={1}>
              Command
            </Grid>
            <Grid item xs={9}>
              Window
            </Grid>
          </Grid>
          {occurrences?.slice(0).reverse().map((oc, j) =>
            <Grid key={j} container direction="row" alignItems="top" style={{ color: oc.isTriggered ? theme.palette.error.light : theme.palette.success.light }}>
              <Grid item xs={1}>
                {oc.triggerStartTime?.format('L HH:mm:ss')}
              </Grid>
              <Grid item xs={1}>
                {oc.triggerEndTime?.format('L HH:mm:ss') ?? '-'}
              </Grid>
              <Grid item xs={1}>
                {GetCommandTypeText(props.single.commandType!)}: {oc.value}
              </Grid>
              <Grid item xs={9}>
                {oc.started?.format('L HH:mm:ss')} - {oc.ended?.format('L HH:mm:ss')}
              </Grid>
            </Grid>
          )}
          <Grid container>
            <Grid item xs={1}>
              <Typography>
                {IsTriggeredFormatter(true)}&nbsp;Triggering <span style={{ fontSize: 10 }}></span><br />
              </Typography>
            </Grid>
            <Grid item xs={2}>
              <Typography>
                {IsTriggeredFormatter(false)}&nbsp;Not Triggering <span style={{ fontSize: 10 }}></span><br />
              </Typography>
            </Grid>
          </Grid>
        </Stack>
      </CardContent>
    </Card>
  )
};

export default CommandOccurrences;
