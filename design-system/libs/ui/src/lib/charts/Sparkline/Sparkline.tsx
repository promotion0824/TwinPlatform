import { EChartsOption, LineSeriesOption } from 'echarts'
import EChartsReact from 'echarts-for-react'
import { forwardRef } from 'react'
import { BaseChart, BaseChartProps, ChartDataset } from '../utils/chartUtils'

export interface SparklineProps
  extends Pick<BaseChartProps, 'dataset' | 'labels' | 'tooltipEnabled'> {
  /** The data to be displayed in the chart. It only supports a single dataset. */
  dataset: [ChartDataset[0]]
  /**
   * Fills in the area below the line.
   * @default false
   */
  fill?: boolean
}

/**
 * `Sparkline` is a small line chart that most often displays time series data.
 */
export const Sparkline = forwardRef<EChartsReact, SparklineProps>(
  ({ dataset, fill = false, ...restProps }, ref) => {
    const option: EChartsOption = {
      animation: false,
      grid: {
        left: 0,
        top: 0,
        right: 0,
        bottom: 0,
        containLabel: false,
      },
      tooltip: {
        axisPointer: {
          type: 'none',
        },
        confine: true,
      },
      xAxis: { show: false },
      yAxis: [{ show: false }],
    }

    const seriesOption: LineSeriesOption = {
      areaStyle: fill ? {} : undefined,
      symbol: 'none',
      type: 'line',
    }

    return (
      <BaseChart
        dataset={dataset}
        labelsType="time"
        option={option}
        orientation="vertical"
        seriesOption={seriesOption}
        ref={ref}
        {...restProps}
      />
    )
  }
)
