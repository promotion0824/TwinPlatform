import { Box, CircularProgress, FormControlLabel, Switch } from "@mui/material";
import { Data, Shape } from 'plotly.js';
import { useState } from "react";
import { useQuery } from "react-query";
import useApi from "../../hooks/useApi";
import { RuleInstanceDto, TimeSeriesDataDto } from "../../Rules";
import { groupBy } from "../graphs/GraphBase";
import RuleSimulation from "../RuleSimulation";
import TrendsBase, { Annotation, mapHoverTemplate, mapX, mapY, toDateNoOffset, TrendLookup } from "./TrendsBase";

/**
 * Plots the trend lines for a single insight (or Rules Instance)
 * @param props
 * @returns
 */
const InsightTrends = (props: { ruleInstance?: RuleInstanceDto, existingTimeSeriesData?: TimeSeriesDataDto, showSimulation: boolean, showDebugOptions?: boolean, startDate?: Date, minStartDate?: Date }) => {

  const apiclient = useApi();
  const rulesInstance = props.ruleInstance;
  const showDebugOptions = props.showDebugOptions ?? false;

  if (!rulesInstance) {
    return (<p>Missing trend data</p>);
  }

  const startDateForSimulation = props.startDate;
  const [showSimulation, setShowSimulation] = useState<boolean>(props.showSimulation ?? false);

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setShowSimulation(event.target.checked == false);
  };

  let isFetched = false;
  let data: (TimeSeriesDataDto | undefined) = undefined;

  if (props.existingTimeSeriesData !== undefined) {
    isFetched = true;
    data = props.existingTimeSeriesData;
  }
  else {
    ; ({ isFetched, data } = useQuery(['timeseries', rulesInstance.id], async () => {
      var ts = await apiclient.getTimeSeriesData(rulesInstance.id);
      return ts;
    }, {
      refetchInterval: 10 * 60 * 1000
    }));
  }

  const annotations: Annotation[] = [];
  if (data && data.trendlines) {
    data.trendlines.forEach(t => {
      t.annotations?.forEach(a => {
        annotations.push({
          text: a.text!,
          timestamp: a.timestamp!,
          yref: t.axis!
        });
      });
    });
  }
  const plotlydata: Data[] = data && data.trendlines ?
    data.trendlines.map((trendline): Data => {

      let marker = {};
      let text = trendline.data!.map(v => v.text ?? "");

      if (trendline.data!.some(v => v.triggered)) {
        marker = {
          size: trendline.data!.map(v => v.triggered ? 10 : 6),
          symbol: trendline.data!.map(v => v.triggered ? 'square' : 'circle'),
          line: {
            width: trendline.data!.map(v => v.triggered ? 1 : 0)
          }
        }
      }

      //this null workaround allows legends with no data to still show up in the chart. This allows addtional simulations to show up in the legend with a "*No data" label
      return {
        x: trendline.data!.length == 0 ? [null] : mapX(trendline),
        y: trendline.data!.length == 0 ? [null] : mapY(trendline, true),
        type: 'scatter',
        mode: 'lines+markers',
        text: text,
        marker: marker,
        name: trendline.name || 'missing',
        yaxis: trendline.axis!,
        xaxis: 'x',// trendline.id === 'result' ? 'x2' : 'x',
        hovertemplate: mapHoverTemplate(trendline, undefined, true),
        line: { shape: trendline.shape === 'hv' ? "hv" : "linear" },  // coerce string to enum
      };
    }) : [];

  const errorShapes: Partial<Shape>[] = data && data.insights ?
    data.insights.map((insight): Partial<Shape> => {
      return ({
        type: 'rect',
        xref: 'x',
        yref: 'paper',
        layer: 'below',
        x0: toDateNoOffset(insight!.startTimestamp!),
        y0: 0.1,
        x1: toDateNoOffset(insight!.endTimestamp!),
        y1: 1,
        fillcolor: insight.isValid == true ? '#f01a1a' : '#f4f4f4',
        opacity: insight.isValid == true ? 0.2 : 0.1,
        line: {
          width: 0
        },
        label: {
          text: `${insight.hours}hrs`,
          font: { size: 10, color: 'white' },
          textposition: 'top center',
        }
      } as any);//we case to any as label prop not on TS interface
    }) : [];

  const dataAxes = (data?.axes ?? []).map(v => {
    return {
      key: v.key,
      title: v.title,
      longName: v.longName,
      shortName: v.shortName,
    }
  });

  const grouped = groupBy(props.ruleInstance!.pointEntityIds!, (d) => d.unit ?? "No Unit");

  const initialData: TrendLookup[] = [];
  for (const key in grouped) {
    const items = grouped[key];
    initialData.push({
      isParent: true,
      name: `[${key}]`,
      label: `[${key}]`,
      id: key,
      trendLine: "",
      selected: false,
      axisKey: key,
      loading: false,
      notFound: false,
      preLoad: false
    });

    for (const itemKey in items.sort((a, b) => {
      return a.id!.localeCompare(b.id!, 'en', { sensitivity: 'base' })
    })) {
      const item = items[itemKey];
      initialData.push({
        isParent: false,
        name: item.variableName ?? "",
        label: `${item.variableName}, ${item.id!} (${item.shortName!})`,
        id: item.id!,
        trendLine: "",
        selected: false,
        axisKey: item.unit!,
        loading: false,
        notFound: false,
        preLoad: false
      });
    }
  }

  return (<>
    {showDebugOptions && <FormControlLabel control={<Switch
      checked={!showSimulation}
      onChange={handleChange}
      inputProps={{ 'aria-label': 'controlled' }}
    />} label="Show tracked values" />}

    {(showSimulation && isFetched) && <RuleSimulation
      ruleId={rulesInstance.ruleId!}
      equipmentId={rulesInstance.equipmentId!}
      startDate={startDateForSimulation}
      useExistingData={true}
      showDates={true}
      minStartDate={props.minStartDate}
      showEquipmentInput={false}
      canAddSimulations={true} />}

    {(!showSimulation && isFetched) && <TrendsBase
      trendItems={initialData}
      existingData={plotlydata}
      existingAxis={dataAxes}
      shapes={errorShapes}
      annotations={annotations}
      startDate={data?.startTime?.toDate()}
      endDate={data?.endTime?.toDate()}
      timezone={rulesInstance.timeZone} />
    }

    {(!isFetched) && <Box
      display="flex"
      justifyContent="center"
      alignItems="center"
      minHeight="10vh"
    >
      Time series pending <CircularProgress />
    </Box>}
  </>);
}

export default InsightTrends;
