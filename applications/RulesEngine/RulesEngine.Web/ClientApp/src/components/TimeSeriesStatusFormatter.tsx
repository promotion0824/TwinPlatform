import { Grid, Typography } from '@mui/material';
import Tooltip from '@mui/material/Tooltip';
import { FaCircle } from 'react-icons/fa';
import { TimeSeriesDto, TimeSeriesStatus } from '../Rules';

const valid = { text: "Valid", value: TimeSeriesStatus._0, color: "green" };
const notwin = { text: "No twin", value: TimeSeriesStatus._16, color: "purple" };
const periodOutOfRange = { text: "Period out of range", value: TimeSeriesStatus._8, color: "red" };
const valueOutOfRange = { text: "Value out of range", value: TimeSeriesStatus._4, color: "orange" };
const stuck = { text: "Stuck", value: TimeSeriesStatus._2, color: "blue" };
const offline = { text: "Offline", value: TimeSeriesStatus._1, color: "grey" };

const LegendItem = (params: { status: any }) => {
  const status = params.status;
  return (<><FaCircle color={status.color} />&nbsp; {status.text} <span style={{ fontSize: 10 }}></span><br /></>);
}

const InlineItem = (params: { status: any, value: TimeSeriesStatus }) => {
  const status = params.status;
  const value = params.value;
  if (value == 0 && status.value == 0) {
    return <Tooltip title={status.text} ><span><FaCircle color={status.color} /></span></Tooltip>;
  }
  return ((value & status.value) ? <Tooltip title={status.text} ><span><FaCircle color={status.color} />&nbsp;</span></Tooltip> : <></>);
}

export function AllStatuses() {
  return [
    valid,
    notwin,
    periodOutOfRange,
    valueOutOfRange,
    stuck,
    offline
  ]
}

export function GetTimeSeriesStatusFilter() {
  return AllStatuses().map(v => {
    return { label: v.text, value: v.value };
  });
}

export const TimeSeriesStatusFormatterLegend = () => {
  return (
    <Grid container spacing={2}>
      <Grid item xs={2}>
        <Typography sx={{ p: 2 }}>
          <LegendItem status={valid} />
          <LegendItem status={notwin} />
        </Typography>
      </Grid>
      <Grid item xs={3}>
        <Typography sx={{ p: 2 }}>
          <LegendItem status={periodOutOfRange} />
          <LegendItem status={valueOutOfRange} />
        </Typography>
      </Grid>
      <Grid item xs={2}>
        <Typography sx={{ p: 2 }}>
          <LegendItem status={stuck} />
          <LegendItem status={offline} />
        </Typography>
      </Grid>
    </Grid>
  )
};

export const TimeSeriesStatusFormatterStatus = (status: TimeSeriesStatus) => {
  return (
    <>
      <InlineItem status={notwin} value={status} />
      <InlineItem status={periodOutOfRange} value={status} />
      <InlineItem status={valueOutOfRange} value={status} />
      <InlineItem status={stuck} value={status} />
      <InlineItem status={offline} value={status} />
      <InlineItem status={valid} value={status} />
    </>
  );
}

export const TimeSeriesStatusFormatter = (ts: TimeSeriesDto) => {
  return TimeSeriesStatusFormatterStatus(ts.status!);
}
