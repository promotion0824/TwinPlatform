import { Alert, Box, Button, Card, CardContent, Checkbox, CircularProgress, FormControlLabel, Grid, Link, Stack, Tooltip, Typography } from '@mui/material';
import { DatePicker, LocalizationProvider } from '@mui/x-date-pickers-pro';
import { AdapterDateFns } from '@mui/x-date-pickers-pro/AdapterDateFns';
import { Collapse } from '@willowinc/ui';
import { Data } from 'plotly.js';
import { useState } from 'react';
import { useQuery } from 'react-query';
import useApi from '../../hooks/useApi';
import { ProblemDetails } from '../../Rules';
import { VisibleIf } from '../auth/Can';
import { TimeSeriesStatusFormatter } from '../TimeSeriesStatusFormatter';
import TrendsBase, { Axis, mapHoverTemplate, mapX, mapY, plotStatus, toUtc, TrendLookup } from './TrendsBase';

const formatTimeSpan = (s: number) => {
  if (s < 91) return s.toFixed(2) + ' s';
  if (s < 90 * 60) return (s / 60).toFixed(2) + ' min';
  return (s / 60 / 60).toFixed(2) + ' hr';
};


/**
 * Plots a single trend line
 * @param props
 * @returns
 */
const SingleTrend = (props: { id: string, timezone?: string, trendItems?: TrendLookup[], existingAxis?: Axis[] }) => {
  const id = props.id;
  const today = new Date();
  let minDate = new Date();
  minDate.setDate(today.getDate() - (365 * 1));//1 year max?

  const defaultStartDate = new Date(today.getFullYear(), today.getMonth(), today.getDate() - 15);
  const defaultEndDate = new Date(today.getFullYear(), today.getMonth(), today.getDate());

  const [startDate, setStartDate] = useState<Date>(defaultStartDate);
  const [endDate, setEndDate] = useState<Date>(defaultEndDate);
  const [refreshing, setRefreshing] = useState(true);
  const [more, showMore] = useState(false);
  const [showDetails, setShowDetails] = useState(false);
  const [enableCompression, setEnableCompression] = useState(true);
  const [optimizeCompression, setOptimizeCompression] = useState(false);
  const apiclient = useApi();

  const {
    data,
    isFetched,
    isError,
    refetch,
    error
  } = useQuery(['timeBuffer', id], async () => {
    try {
      const endOfDayEndDate = new Date(endDate.getFullYear(), endDate.getMonth(), endDate.getDate(), 23, 59, 59);
      const data = await apiclient.getTimeSeriesDataForCapability(id, toUtc(startDate), toUtc(endOfDayEndDate), props.timezone ?? "", enableCompression, optimizeCompression);
      setRefreshing(false);
      return data;
    }
    catch (e) {
      setRefreshing(false);
      throw e;
    }
  });

  if (isFetched && isError) {
    if ((error as ProblemDetails)?.status === 500) {
      return (<Alert severity="error">An error occured fetching trend data</Alert>);
    }
  }

  if (!isFetched || refreshing) {
    return (<Box
      display="flex"
      justifyContent="center"
      alignItems="center"
      minHeight="10vh">
      Time series pending &nbsp;<CircularProgress />
    </Box>);
  }

  const buffer = data!;

  const trendline = buffer?.trendline;
  const rawTrendline = buffer?.rawTrendline;

  const plotlydata: Data[] = [
    {
      x: mapX(rawTrendline),
      y: mapY(rawTrendline),
      type: 'scatter',
      mode: 'lines+markers',
      marker: { color: "lightgrey", opacity: 0.5 },
      name: rawTrendline?.name || 'missing',
      text: rawTrendline?.data?.map(v => v.text ?? "") ?? [],
      yaxis: rawTrendline?.axis!,
      hovertemplate: mapHoverTemplate(rawTrendline, undefined, true),
      xaxis: 'x',
      line: { width: 0.5, color: "lightgrey", shape: rawTrendline?.shape === 'hv' ? "hv" : "linear", dash: 'dash' },
      showlegend: false,
      opacity: 0.5
    },
    {
      x: mapX(trendline),
      y: mapY(trendline),
      type: 'scatter',
      mode: 'lines+markers',
      //marker: { color: colorFromPoint(trendline.id!) },
      name: trendline?.name || 'missing',
      yaxis: trendline?.axis!,
      text: trendline?.data?.map(v => v.text ?? "") ?? [],
      hovertemplate: mapHoverTemplate(rawTrendline, undefined, true),
      xaxis: 'x',// trendline.id === 'result' ? 'x2' : 'x',
      line: { shape: trendline?.shape === 'hv' ? "hv" : "linear" },  // coerce string to enum
      legendgroup: trendline?.name,
      showlegend: true
    }];

  plotStatus(plotlydata, trendline);

  return (
    <Stack spacing={2}>
      <Card variant="outlined">
        <CardContent>
          <Stack spacing={2}>
            <Grid container spacing={2} alignItems="flex-end">
              <Grid item sm={2}>
                <LocalizationProvider dateAdapter={AdapterDateFns}>
                  <DatePicker
                    label="Timeseries start date"
                    value={startDate}
                    minDate={minDate}
                    maxDate={new Date()}
                    sx={{ width: '100%' }}
                    onChange={(newValue) => {
                      if (newValue !== null) {
                        setStartDate(newValue);
                      }
                    }}
                  />
                </LocalizationProvider>
              </Grid>
              <Grid item sm={2}>
                <LocalizationProvider dateAdapter={AdapterDateFns}>
                  <DatePicker
                    label="Timeseries end date"
                    value={endDate}
                    minDate={minDate}
                    maxDate={new Date()}
                    sx={{ width: '100%' }}
                    onChange={(newValue) => {
                      if (newValue !== null) {
                        setEndDate(newValue);
                      }
                    }}
                  />
                </LocalizationProvider>
              </Grid>
              <Grid item sm={8}>
                <Button variant="contained" onClick={() => { setRefreshing(true); refetch(); }}>Refresh</Button>
              </Grid>

              <Grid item hidden={isError || !data}>
                <Typography>
                  <Link component="button" variant="body2" onClick={() => { setShowDetails(!showDetails) }}>
                    {!showDetails ? "+ Details" : "- Details"}
                  </Link>
                </Typography>
              </Grid>
              <Grid item hidden={isError || !data}>
                <VisibleIf canViewAdminPage>
                  <Typography>
                    <Link component="button" variant="body2" onClick={() => { showMore(!more) }}>
                      {!more ? "+ Options" : "- Options"}
                    </Link>
                  </Typography>
                </VisibleIf>
              </Grid>

              {(more && data) &&
              <Grid item sm={12}>
                  <FormControlLabel control={<Checkbox
                    checked={enableCompression}
                    onChange={() => setEnableCompression(!enableCompression)}
                    color="primary"
                    inputProps={{ 'aria-label': 'primary checkbox' }}
                  />} label="Enable Compression" title="Indicates whether compression is applied to the timeseries" />
                  <FormControlLabel control={<Checkbox
                    checked={optimizeCompression}
                    onChange={() => setOptimizeCompression(!optimizeCompression)}
                    color="primary"
                    inputProps={{ 'aria-label': 'primary checkbox' }}
                  />} label="Optimize Compression" title="Indicates whether more aggressive compression is used for older values" />
              </Grid>}

              {(data && !isError) &&
              <Grid item sm={12} hidden={!showDetails}>
                <Collapse opened={showDetails}>
                  <Stack spacing={1}>
                    <Typography>Max: {buffer.timeSeries?.maxValue?.toFixed(2) ?? '-'} {buffer.timeSeries?.unitOfMeasure}</Typography>
                    <Typography>Min: {buffer.timeSeries?.minValue?.toFixed(2) ?? '-'} {buffer.timeSeries?.unitOfMeasure}</Typography>
                    <Typography>Average: {buffer.timeSeries?.averageValue?.toFixed(2) ?? '-'} {buffer.timeSeries?.unitOfMeasure}</Typography>
                    <Typography>Average in buffer: {buffer.timeSeries?.averageInBuffer?.toFixed(2) ?? '-'} {buffer.timeSeries?.unitOfMeasure}</Typography>
                    <Typography>Total in buffer: {buffer.timeSeries?.bufferCount}</Typography>
                    <Typography>Period (est): {formatTimeSpan(buffer.timeSeries?.estimatedPeriod!)}</Typography>
                    <Typography>End time: {buffer.timeSeries?.endTime?.format()}</Typography>
                    <Typography>Values processed: {buffer.timeSeries?.totalValuesProcessed}</Typography>
                    <Typography>Timezone: {props.timezone}</Typography>
                    <Typography>Status: {TimeSeriesStatusFormatter(buffer.timeSeries!)}</Typography>
                  </Stack>
                </Collapse>
              </Grid>}
            </Grid>

            {isError && <Alert severity="warning">Trend data missing or not recent</Alert>}

          </Stack>
        </CardContent>
      </Card>
      {(data && !isError) &&
        <TrendsBase
          trendItems={props.trendItems ?? []}
          existingAxis={props.existingAxis ?? []}
          existingData={plotlydata}
          startDate={startDate}
          endDate={endDate}
          timezone={props.timezone} />
      }
    </Stack>
  )
}

export default SingleTrend;
