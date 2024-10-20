import {
  BarSeriesOption,
  EChartsOption,
  LineSeriesOption,
  XAXisComponentOption,
} from 'echarts'
import EChartsReact from 'echarts-for-react'
import { merge } from 'lodash'
import { forwardRef, useImperativeHandle, useRef } from 'react'
import invariant from 'tiny-invariant'
import { gridSize } from '../../theme/echartsTheme'
import useLegendHeight from './useLegendHeight'

export const ERR_DATA_LENGTH =
  'The length of each dataset must match the length of labels.'

export type ChartDataset = Array<{
  data: number[]
  name: string
}>

type SeriesOption = BarSeriesOption | LineSeriesOption

export interface BaseChartProps {
  /**
   * The name of the category/timeseries axis. Will be shown on the data export if provided.
   * The default will match the type of labels chosen.
   * @default 'Category' | 'Time'
   */
  axisLabel?: string
  /**
   * Please use the `labels` prop instead. The `labels` prop also supports time series data.
   * @deprecated
   */
  categories?: string[]
  /** The data to be displayed in the chart. */
  dataset: ChartDataset
  /** The labels to be displayed below the data in the chart. */
  labels: string[]
  /**
   * Specifies how the non-value axis labels should be displayed. If set to "time",
   * the labels will scale accordingly to fit the chart display.
   * @default 'category'
   */
  labelsType?: 'category' | 'time'
  /**
   * Add lines on top of the chart to show additional datasets.
   * These can use the same axis as the main dataset or their own. (See `lineOverlaysAxis` prop.)
   */
  lineOverlays?: ChartDataset
  /**
   * Specifies the type of axis to be used for all line overlays.
   * 'independent': Will use a new axis displayed on the opposite side of the chart from the main axis
   * 'shared': Will use the same axis as the main dataset
   * @default 'independent'
   */
  lineOverlaysAxis?: 'independent' | 'shared'
  /**
   * Switch between a row or column chart.
   * @default 'horizontal'
   */
  orientation?: 'horizontal' | 'vertical'
  /**
   * Display a tooltip when hovering over a bar. Disabling this also disables the row highlighting.
   * @default true
   */
  tooltipEnabled?: boolean
}

interface BaseChartComponentProps extends BaseChartProps {
  /** ECharts options that are merged into the base options. */
  option?: EChartsOption
  /**
   * When set to true, the alignment of the category axis labels won't be updated.
   * @default false
   */
  preserveAxisLabelAlignment?: boolean
  /**
   * The series options to the displayed on the chart. Will be applied to each row in the dataset.
   * Line overlays are automatically added to the end of the series.
   */
  seriesOption: SeriesOption
}

export const BaseChart = forwardRef<EChartsReact, BaseChartComponentProps>(
  (
    {
      labelsType = 'category',
      axisLabel = labelsType === 'category' ? 'Category' : 'Time',
      dataset,
      labels,
      lineOverlays = [],
      lineOverlaysAxis = 'independent',
      orientation = 'horizontal',
      option = {},
      preserveAxisLabelAlignment = false,
      seriesOption,
      tooltipEnabled = true,
      ...restProps
    },
    ref
  ) => {
    for (const group of dataset) {
      if (group.data.length !== labels.length) {
        throw new Error(ERR_DATA_LENGTH)
      }
    }

    const chartRef = useRef<EChartsReact>(null)
    const legendHeight = useLegendHeight(chartRef)

    useImperativeHandle(
      ref,
      () => {
        invariant(chartRef.current, 'chartRef is not set')
        return chartRef.current
      },
      [chartRef]
    )

    const chartDataset = {
      source: [
        [
          axisLabel,
          ...dataset.map(({ name }) => name),
          ...lineOverlays.map(({ name }) => name),
        ],
        ...labels.map((label, index) => [
          label,
          ...dataset.map(({ data }) => data[index]),
          ...lineOverlays.map(({ data }) => data[index]),
        ]),
      ],
    }

    const getAxisLabel = (axisType: XAXisComponentOption['type']) =>
      (orientation === 'horizontal' && axisType === 'value') ||
      (orientation === 'vertical' && axisType !== 'value')
        ? {
            alignMinLabel: 'left',
            alignMaxLabel: 'right',
          }
        : {
            verticalAlignMinLabel: 'bottom',
            verticalAlignMaxLabel: 'top',
          }

    const labelsAxis = {
      ...(!preserveAxisLabelAlignment
        ? { axisLabel: getAxisLabel(labelsType) }
        : {}),
      inverse: orientation === 'horizontal',
      type: labelsType,
    }

    const valuesAxes = [
      { axisLabel: getAxisLabel('value'), position: 'left', type: 'value' },
    ]

    if (lineOverlays.length) {
      valuesAxes.push({
        axisLabel: getAxisLabel('value'),
        position: 'right',
        type: 'value',
      })
    }

    const mainSeries = dataset.map<SeriesOption>((data, index) => ({
      encode:
        orientation === 'horizontal'
          ? {
              x: index + 1,
              y: 0,
            }
          : {
              x: 0,
              y: index + 1,
            },
      name: data.name,
      ...seriesOption,
    }))

    const chartSeries = [
      ...mainSeries,
      ...lineOverlays.map<LineSeriesOption>((lineOverlay) => ({
        encode:
          orientation === 'horizontal'
            ? {
                x: lineOverlay.name,
                y: 0,
              }
            : {
                x: 0,
                y: lineOverlay.name,
              },
        name: lineOverlay.name,
        symbol: 'circle',
        type: 'line',
        ...(orientation === 'horizontal'
          ? { xAxisIndex: lineOverlaysAxis === 'shared' ? 0 : 1 }
          : { yAxisIndex: lineOverlaysAxis === 'shared' ? 0 : 1 }),
      })),
    ]

    const chartOptions = merge(
      {
        dataset: chartDataset,
        grid: { bottom: legendHeight + gridSize },
        series: chartSeries,
        legend: {
          left: 0,
          top: 'bottom',
          show: chartSeries.length > 1,
        },
        xAxis: orientation === 'horizontal' ? valuesAxes : labelsAxis,
        yAxis: orientation === 'horizontal' ? labelsAxis : valuesAxes,
        tooltip: {
          trigger: tooltipEnabled ? 'axis' : 'none',
        },
      },
      option
    )

    return (
      <EChartsReact
        option={chartOptions}
        ref={chartRef}
        style={{ height: '100%' }}
        theme="willow"
        {...restProps}
      />
    )
  }
)
