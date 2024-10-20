import { Grid, Stack, Typography } from '@mui/material';
import { MLModelDto } from '../../Rules';

// See https://medium.com/terria/typescript-transforming-optional-properties-to-required-properties-that-may-be-undefined-7482cb4e1585
type Complete<T> = { [P in keyof Required<T>]: Pick<T, P> extends Required<Pick<T, P>> ? T[P] : (T[P] | undefined); }

const MLModelDetails = (params: {
  model: MLModelDto
}) => {

  const model = params.model as Complete<MLModelDto>;

  return (
    <Stack spacing={1} sx={{ mt: 2 }}>
      <Typography variant="h4">Name: <Typography variant="body1">{model.fullName}</Typography></Typography>
      <Typography variant="h4">Parameters:</Typography>
      <Grid container direction={'row'}>
        <Grid item xs={2}><Typography variant="h5">Name</Typography></Grid>
        <Grid item xs={1}><Typography variant="h5">Unit</Typography></Grid>
        <Grid item xs={1}><Typography variant="h5">Input Size</Typography></Grid>
        <Grid item xs={8}><Typography variant="h5">Description</Typography></Grid>
      </Grid>
      {model.inputParams!.map((x, i) =>
        <Grid container direction={'row'} key={i}>
          <Grid item xs={2}>{x.name}</Grid>
          <Grid item xs={1}>{x.unit}</Grid>
          <Grid item xs={1}>{x.size}</Grid>
          <Grid item xs={8}>{x.description}</Grid>
        </Grid>)}
    </Stack>
  )
}

export default MLModelDetails;
