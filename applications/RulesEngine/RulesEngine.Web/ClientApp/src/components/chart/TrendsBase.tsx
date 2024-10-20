import { CircularProgress, FormControl, Grid, InputLabel, MenuItem, OutlinedInput, Select, SelectChangeEvent, Theme, useTheme } from "@mui/material";
import moment from "moment";
import { Annotations, Data, Layout, LayoutAxis, Shape } from 'plotly.js';
import { useEffect, useState } from "react";
import Plot from 'react-plotly.js';
import useApi from "../../hooks/useApi";
import { BatchRequestDto, TimeSeriesBufferDto, TrendlineDto } from "../../Rules";
import { ExportToCsv } from "../ExportToCsv";
import { createCsvFileResponse } from "../grids/GridFunctions";
import { AllStatuses } from "../TimeSeriesStatusFormatter";

export interface TrendLookup {
  label: string,
  name: string,
  isParent: boolean,
  id: string,
  axisKey: string,
  trendLine: string,
  selected: boolean,
  loading: boolean,
  notFound: boolean,
  preLoad: boolean
}

interface BufferResult {
  buffer?: TimeSeriesBufferDto,
  item: TrendLookup
}

export interface Axis {
  key?: string | undefined;
  shortName?: string | undefined;
  longName?: string | undefined;
  title?: string | undefined;
}

export interface Annotation {
  timestamp: moment.Moment,
  text: string,
  yref: string
}

const MenuProps = {
  PaperProps: {
    style: {
      maxHeight: 500
    },
  },
};

export function toUtc(date: Date) {
  return moment.utc(moment(date).format('YYYY-MM-DDTHH:mm:ss'));
}

export function toDateNoOffset(date: moment.Moment) {
  return moment(date.format('YYYY-MM-DDTHH:mm:ss')).toDate()!
}

export function mapX(trendline?: TrendlineDto) {
  return trendline?.data!.map(v => toDateNoOffset(v.timestamp!));
}

export function mapY(trendline?: TrendlineDto, convertPercentages?: boolean) {
  //convert seconds to hours. Total faulty time gets large
  if (trendline?.unit === "s") {
    const HOUR = 60 * 60;
    return trendline!.data!.map(v => {
      return v.valueDouble! / HOUR;
    });
  }
  if ((convertPercentages ?? false) && (trendline?.isSystemGenerated ?? false) && (trendline?.unit?.startsWith("%") ?? false)) {
    return trendline!.data!.map(v => {
      return v.valueDouble! * 100;
    });
  }

  return trendline?.data!.map(v => v.valueDouble ?? (v.valueBool ? 1 : 0));
}

export function plotStatus(plotlydata: Data[], trendline?: TrendlineDto, yaxis?: string, name?: string, legendGroup?: string) {
  if (trendline?.statuses?.length ?? 0 > 0) {
    const statusFilter = AllStatuses();

    if ((trendline?.statuses ?? []).length > 0) {
      trendline?.statuses?.forEach(s => {
        //take the first status
        const status = statusFilter.find(v => v.value & s.status!)!;

        if (s.status == 0) {
          return;
        }

        plotlydata.push({
          x: mapX(s.values),
          y: mapY(s.values),
          type: 'scatter',
          mode: 'lines+markers',
          name: name ?? trendline?.name,
          yaxis: yaxis ?? trendline?.axis!,
          hovertemplate: mapHoverTemplate(trendline, status.text),
          line: { color: "black", shape: trendline?.shape === 'hv' ? "hv" : "linear", dash: 'longdash' },
          legendgroup: legendGroup ?? trendline?.name,
          showlegend: false
        });
      });
    }
  }
}

export function mapHoverTemplate(trendline?: TrendlineDto, status?: string, includeText?: boolean) {
  const text = includeText === true ? "%{text}" : "";

  if (status !== undefined) {
    status = status + ", ";
  }
  else {
    status = "";
  }

  if (trendline?.unit === "s") {
    return status + '%{x}<br>%{y:.1f}hr<br>' + text;
  }
  if (trendline?.unit === "bool") {
    return status + '%{x}<br>%{y:d}<br>' + text;
  }
  if (trendline?.unit === "fault") {
    return status + '%{x}<br>%{y:d}<br>' + text;
  }
  if (trendline?.unit?.startsWith("%") ?? false) {
    return status + '%{x}<br>%{y:.1f}%<br>' + text;
  }
  return status + '%{x}<br>%{y:.2f} ' + (trendline?.unit ?? '') + '<br>' + text;
}

/**
 * Plots the trend lines for multiple capabilties
 */
const TrendsBase = (props: { trendItems: TrendLookup[], existingData?: Data[], shapes?: Partial<Shape>[], existingAxis?: Axis[], annotations?: Annotation[], startDate?: Date, endDate?: Date, timezone?: string }) => {

  const annotations = (props.annotations ?? []).map(v => {
    return {
      arrowcolor: "white",
      hovertext: `${v.text}:${v.timestamp.format('MMM DD, yyyy, HH:mm:ss')}`,
      x: toDateNoOffset(v.timestamp).getTime(),
      y: 0,
      xref: 'x',
      yref: v.yref,
      yanchor: 'top',
      text: v.text,
      showarrow: false,
      arrowhead: 0,
      ax: -50,
      ay: 20
    } as Annotations;
  });

  const createLayout = (subplots: Axis[]): Partial<Layout> => {
    const axes = subplots.reduce((acc: { [key: string]: Partial<LayoutAxis> }, curr) =>
      (acc[curr.longName!] = { title: curr.key == "s" ? "hours" : curr.title!, showgrid: false }, acc), {});
    return {
      grid: {
        rows: subplots.length + 1, columns: 1,
        roworder: 'top to bottom',
        subplots: subplots.map(y => `x${y}`) ?? undefined,
      },
      shapes: props.shapes ?? [],
      paper_bgcolor: theme.palette.background.paper,
      plot_bgcolor: theme.palette.background.default,
      font: { family: theme.typography.fontFamily, color: theme.palette.text.primary },
      autosize: true,
      height: Math.max(subplots.length * 125, 680),
      title: "Timeseries",
      annotations: annotations,
      //colorway: ["red", "green", "blue", "goldenrod", "magenta", "brown", "orange", "magenta", "cyan", "brown", "black"],
      ...axes
    }
  };

  const capabilityStartTime: (moment.Moment | undefined) = props.startDate !== undefined ? moment(props.startDate) : undefined;
  const capabilityEndTime: (moment.Moment | undefined) = props.endDate !== undefined ? moment(props.endDate) : undefined;
  const timezone = props.timezone === null ? undefined : props.timezone;
  const [axis, setAxis] = useState<Axis[]>(props.existingAxis ?? []);
  const [twinIds, setTwinIds] = useState<string[]>([]);
  const apiclient = useApi();
  const [data, setData] = useState<TrendLookup[]>([]);
  const theme = useTheme();
  const [revision, setRevision] = useState(0);
  const [loaded, setLoaded] = useState(false);
  const [plotlydata, setPlotData] = useState<Data[]>(props.existingData ?? []);
  const [bufferResult, setResult] = useState<BufferResult>();
  const [plotlylayout, setplotlylayout] = useState<Partial<Layout>>(createLayout(props.existingAxis ?? []));

  //push in data into mullti select list
  useEffect(() => {
    setData(props.trendItems);
    setTwinIds(props.trendItems.filter(v => v.selected === true).map(v => v.id));
    const promises: Promise<TimeSeriesBufferDto>[] = [];

    props.trendItems.filter(v => v.selected === true).forEach((item) => {
      const promise = apiclient.getTimeSeriesDataForCapability(item.id, capabilityStartTime, capabilityEndTime, timezone, true, true);
      promises.push(promise);
    })

    if (promises.length > 0) {
      Promise.all(promises)
        .then((buffers) => {
          buffers.forEach((buffer, i) => {
            setTimeout(() => {
              const item = props.trendItems.find(v => v.id == buffer.id);
              setResult({
                buffer: buffer,
                item: item!
              });
            }, 1000 * (i + 1));//TODO: any better way than this?
          });
          setLoaded(true);
        })
        .catch(_ => { });
    }
    else {
      setLoaded(true);
    }
  }, []);

  const updateLookup = (item: TrendLookup) => {
    setData(oldState => {
      let newState = [...oldState];
      const index = newState.findIndex(v => v.id == item.id);
      newState[index] = item;
      return newState;
    });
  }

  const handleBufferResult = () => {
    if (bufferResult === undefined) {
      return;
    }
    const item = bufferResult.item;
    const buffer = bufferResult.buffer;
    const trendline = buffer?.trendline;

    if (!buffer || !trendline) {
      item.notFound = true;
      item.loading = false;
      updateLookup(item);
      return;
    }

    const unit = item.axisKey;
    let existingAxis = axis.find(v => v.key === unit);

    const currentAxis = [...axis];

    if (existingAxis === undefined) {
      const axisIndex = axis.length;

      existingAxis = {
        key: unit,
        title: unit,
        longName: axisIndex == 0 ? "yaxis" : "yaxis" + (axisIndex + 1),
        shortName: axisIndex == 0 ? "y" : "y" + (axisIndex + 1)
      };

      currentAxis.push(existingAxis);

      setAxis([...axis, existingAxis]);
    }

    let text = trendline.data!.map(v => v.text ?? "");

    const newData: Data[] = [{
      x: mapX(trendline),
      y: mapY(trendline),
      type: 'scatter',
      mode: 'lines+markers',
      name: item.name,
      text: text,//we use the text property for removals and it also shows
      //as hover text which is nice when multiple lines have the same name
      yaxis: existingAxis.shortName,
      xaxis: 'x',
      hovertemplate: mapHoverTemplate(trendline, undefined, true),
      line: { shape: trendline.shape === 'hv' ? "hv" : "linear" },  // coerce string to enum
      legendgroup: trendline?.name
    }];

    plotStatus(newData, trendline, existingAxis.shortName, item.name, trendline?.name);

    setPlotData([...plotlydata, ...newData]);

    const newLayout = createLayout(currentAxis);

    setplotlylayout(newLayout);

    setRevision(revision + 1);

    item.trendLine = trendline.name!;
    item.loading = false;

    updateLookup(item);
  }

  const removeTrendline = (item: TrendLookup) => {
    const entries: any = plotlydata.filter((v) => (v as any).legendgroup === item.trendLine);
    if (entries.length == 0) {
      return;
    }

    const entry = entries[0];
    const existingAxis: any = axis.find(v => v.key == item.axisKey);
    const axisEntries: any = plotlydata.filter((v: any) => v.yaxis === existingAxis!.shortName);

    //remove subplot if it's the only one on it
    if (axisEntries.length == entries.length) {
      const currentAxis = axis.filter((v) => v.shortName !== entry.yaxis);
      setAxis(axis.filter((v) => v.shortName !== entry.yaxis));
      const newLayout = createLayout(currentAxis);
      setplotlylayout(newLayout);
    }

    setPlotData(plotlydata.filter((v) => (v as any).legendgroup !== item.trendLine));
    setRevision(revision + 1);
  }

  const [_, setCount] = useState<number>(0);

  const setBuffer = async (item: TrendLookup) => {
    try {
      const buffer = await apiclient.getTimeSeriesDataForCapability(item.id, capabilityStartTime, capabilityEndTime, timezone, true, true);

      setCount(v => {
        //this is to delay binding with safe timing, surely there is a better way?
        setTimeout(() => {
          setResult(_ => {
            return {
              buffer: buffer,
              item: item
            }
          })
        }, 1000 * (v % 2 + 1));
        return v + 1;
      });


    }
    catch (e) {
      setResult({
        item: item
      })
    }
  }

  useEffect(() => {
    handleBufferResult();
  }, [bufferResult]);

  const handleChange = async (event: SelectChangeEvent<typeof twinIds>) => {
    const {
      target: { value },
    } = event;

    let ids = typeof value === 'string' ? value.split(',') : value;

    const newItem = data.find(item => ids.includes(item.id) && item.selected !== true);

    if (newItem !== undefined) {
      if (newItem.isParent === true) {
        return;
      }
      newItem.selected = true;
      newItem.loading = true;
      updateLookup(newItem);
      setTwinIds(ids);
      await setBuffer(newItem);
    }
    else {
      let deletedItem = data.find(item => !ids.includes(item.id) && item.selected === true);

      if (deletedItem !== undefined) {
        //wait until everything's loaded
        if (data.some(v => v.loading === true)) {
          return;
        }
        deletedItem.selected = false;
        updateLookup(deletedItem);
        setTwinIds(ids);
        removeTrendline(deletedItem);
      }
    }
  };

  function getStyles(id: string, theme: Theme) {
    const item = data.find((v) => v.id == id)!;
    return {
      fontWeight: item.selected === true ? theme.typography.fontWeightRegular : theme.typography.fontWeightMedium
    };
  }

  function disabled(item: TrendLookup) {
    return item.notFound === true;
  }

  const csvExport = {
    downloadCsv: (_: any) => {
      let data: any = [];
      var orderedNames = plotlydata.map(v => v.name);

      function sortF(ob1: any, ob2: any) {
        const timeDiff = ob1.TimeStamp.getTime() - ob2.TimeStamp.getTime();

        if (timeDiff != 0) {
          return timeDiff;
        }

        return orderedNames.indexOf(ob1.name) - orderedNames.indexOf(ob2.name);
      }

      plotlydata.forEach((v: any) => {
        let name = v.name as string;
        let unit = "";
        let timeseries = name;

        axis.forEach(x => {
          const yaxis = x.key!;
          if (name.endsWith(" " + yaxis)) {
            const spaceIndex = name.lastIndexOf(" " + yaxis);
            //strip unit from name
            unit = name.substring(spaceIndex + 1, name.length);
            timeseries = name.substring(0, spaceIndex);
          }
        });

        const isSeconds = unit == "s";

        v.x.filter((v: any) => v !== null).forEach((x: any, i: number) => {
          let y = v.y[i];
          if (isSeconds) {
            y = Math.round(y * 60 * 60);
          }
          data.push({
            TimeSeries: timeseries,
            Unit: unit,
            TimeStamp: x,
            Value: y,
            Expression: (v.text[i] ?? "").replace(/,/g, ';')
          })
        })
      })

      return createCsvFileResponse(data.sort(sortF).map((d: any) => {
        return {
          TimeSeries: d.TimeSeries,
          Unit: d.Unit,
          TimeStamp: moment(d.TimeStamp).format('L HH:mm:ss'),
          Value: d.Value,
          Expression: d.Expression
        };
      }), `ExportResults.csv`);
    },
    createBatchRequest: () => new BatchRequestDto()
  };

  return (loaded) ? (<div style={{ width: '100%' }}>
    {props.trendItems.length > 0 &&
      <FormControl sx={{ mb: 1, width: "50%" }}>
        <InputLabel id="capabilities-label">Select a capability</InputLabel>
        <Select
          labelId="capabilities-label"
          id="capabilities-list"
          multiple
          value={twinIds}
          onChange={handleChange}
          input={<OutlinedInput label="Select a capabilty" />}
          MenuProps={MenuProps}
        >
          {data.map((v) => (
            <MenuItem
              key={v.id}
              value={v.id}
              disabled={disabled(v)}
              style={getStyles(v.id, theme)}
            >
              {v.label}{v.notFound && <>&nbsp;(*no data)</>}&nbsp;{v.loading && <CircularProgress color="inherit" size={15} />}
            </MenuItem>
          ))}
        </Select>
      </FormControl>
    }

    <Grid container>
      <Grid item xs={6}>{timezone && <p>Timezone: {timezone}</p>}</Grid>
      <Grid item xs={6}>
        <div style={{ float: 'right', paddingLeft: 5 }}>
          <ExportToCsv source={csvExport} />
        </div>
      </Grid>
    </Grid>

    <Plot
      data={plotlydata}
      revision={revision}
      style={{ width: "100%", height: "100%" }}
      layout={plotlylayout} />
  </div>) : <>Time series pending <CircularProgress size={15} /></>;
}

export default TrendsBase;
