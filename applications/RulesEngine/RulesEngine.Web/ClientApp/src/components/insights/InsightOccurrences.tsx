import { Card, CardContent, Grid, Stack, Typography, useTheme } from '@mui/material';
import { InsightDto } from '../../Rules';

const InsightOccurrences = (props: { single: InsightDto }) => {
  const theme = useTheme();
  const { occurrences } = props.single;

  return (
    <Card variant="outlined">
      <CardContent>
        <Stack spacing={2}>
          <Typography variant="body1">Timezone: {props.single.timeZone}</Typography>
          <Typography variant="h4">Failed Occurrence Count: {occurrences?.filter((v) => v.isFaulted === true).length}</Typography>
          <Stack spacing={0.5}>
            {occurrences && occurrences.slice(0).reverse().map((oc, j) =>
              <Grid key={j} container direction="row" alignItems="top" style={{ color: !oc.isValid ? theme.palette.warning.light : oc.isFaulted ? theme.palette.error.light : theme.palette.success.light }}>
                <Grid item xs={2}>
                  {oc.started?.format('L HH:mm:ss')}
                </Grid>
                <Grid item xs={2}>
                  {oc.ended?.format('L HH:mm:ss')}
                </Grid>
                <Grid item xs={8}>
                  {oc.text?.split('\n').map((x, i) => <div key={i}>{x}</div>)}
                </Grid>
              </Grid>
            )}
          </Stack>
        </Stack>
      </CardContent>
    </Card>
  )
};

export default InsightOccurrences;
