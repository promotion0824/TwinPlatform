import { Grid, useTheme } from '@mui/material';
import { Layout, PieData } from 'plotly.js';
import React from 'react';
import Plot from 'react-plotly.js';
import { SystemSummaryDto } from '../../Rules';

interface PieChartProps {
  summary: SystemSummaryDto
}

/**
 * Plots a pie chart
 * @param props
 * @returns
 */
const PieChart: React.FC<PieChartProps> = (props: PieChartProps) => {
  const theme = useTheme();

  const summary = props.summary;

  const insightsData = summary.insightsByModel!;
  const commandsData = summary.commandsByModel!;

  var plotdata1: Partial<PieData>[] = [
    {
      type: 'pie',
      values: [summary.countInsightsHealthy!, summary.countInsightsFaulted!, summary.countInsightsInValid!],
      labels: ['Not faulted', 'Faulted', 'Invalid'],
      marker: { colors: ["green", "red", "grey"] }
    }];

  var plotdata2: Partial<PieData>[] = [
    {
      type: 'pie',
      values: Object.values(insightsData!).map(x => x),
      labels: Object.keys(insightsData).map(x => x.toString().replace("dtmi:com:willowinc:", "").replace(";1", ""))
    }
  ];

  var plotdata3: Partial<PieData>[] = [
    {
      type: 'pie',
      values: Object.values(commandsData).map(x => x),
      labels: Object.keys(commandsData).map(x => x.toString().replace("dtmi:com:willowinc:", "").replace(";1", ""))
    }
  ];

  const plotlylayout1: Partial<Layout> =
  {
    title: 'Skill Instances',
    paper_bgcolor: theme.palette.background.paper,
    font: { family: theme.typography.fontFamily, color: theme.palette.text.primary }
  };

  const plotlylayout2: Partial<Layout> =
  {
    title: 'Faults by model',
    paper_bgcolor: theme.palette.background.paper,
    font: { family: theme.typography.fontFamily, color: theme.palette.text.primary }
  };

  const plotlylayout3: Partial<Layout> =
  {
    title: 'Commands by model',
    paper_bgcolor: theme.palette.background.paper,
    font: { family: theme.typography.fontFamily, color: theme.palette.text.primary }
  };

  return (<>
    <Grid container spacing={1}>
      {Object.values(insightsData).length > 0 &&
        <Grid item xs={6}>
          <Plot
            data={plotdata1}
            layout={plotlylayout1} />
        </Grid>
      }
      {Object.values(insightsData).length > 0 &&
        <Grid item xs={6}>
          <Plot
            data={plotdata2}
            layout={plotlylayout2} />
        </Grid>
      }
      {Object.values(commandsData).length > 0 &&
        <Grid item xs={6}>
          <Plot
            data={plotdata3}
            layout={plotlylayout3} />
        </Grid>
      }
    </Grid>

  </>);
}

export default PieChart;
