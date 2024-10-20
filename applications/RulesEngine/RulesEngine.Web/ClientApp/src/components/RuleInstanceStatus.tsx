import { Alert, Grid, Typography } from '@mui/material';
import { FaCheckCircle, FaCircle } from 'react-icons/fa';
import { RuleInstanceDto, RuleInstanceStatus } from '../Rules';
import { RuleInstanceStatusLookup } from './Lookups';

export const RuleInstanceStatusAlert = (params: { ruleInstance: RuleInstanceDto }) => {
  const ruleInstance = params.ruleInstance;

  if (ruleInstance.status! == 1) {
    return (<></>);
  }

  return (<>{RuleInstanceStatusLookup.getStatuses(ruleInstance.status!, false).map((v, i) =>
    <Alert key={`${i}_Status`} severity="warning">
      <Typography variant="caption">{v.name}</Typography> <Typography variant="subtitle1">{v.description}</Typography>
    </Alert>)}</>);
}

export const RuleInstanceStatusLegend = () => (<Grid container spacing={2}>
  <Grid item xs={4}>
    <Typography sx={{ p: 2 }}>
      <FaCheckCircle color='green' /> {RuleInstanceStatusLookup.Status[RuleInstanceStatus._1].name}<br />
      <FaCircle color='red' /> {RuleInstanceStatusLookup.Status[RuleInstanceStatus._2].name}
    </Typography>
  </Grid>
  <Grid item xs={4}>
    <Typography sx={{ p: 2 }}>
      <FaCircle color='orange' /> {RuleInstanceStatusLookup.Status[RuleInstanceStatus._8].name}<br />
      <FaCircle color='blue' /> {RuleInstanceStatusLookup.Status[RuleInstanceStatus._16].name}
    </Typography>
  </Grid>
  <Grid item xs={4}>
    <Typography sx={{ p: 2 }}>
      <FaCircle color='grey' /> {RuleInstanceStatusLookup.Status[RuleInstanceStatus._4].name}<br />
      <FaCircle color='pink' /> {RuleInstanceStatusLookup.Status[RuleInstanceStatus._32].name}
    </Typography>
  </Grid>
</Grid>
);
