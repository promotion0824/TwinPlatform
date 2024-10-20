import { EChartsOption, LineSeriesOption } from 'echarts'
import EChartsReact from 'echarts-for-react'
import { forwardRef } from 'react'
import { BaseChart, BaseChartProps } from '../utils/chartUtils'

export interface LineChartProps
  extends Omit<
    BaseChartProps,
    'lineOverlays' | 'lineOverlaysAxis' | 'orientation'
  > {
  /**
   * The style in which the line is drawn on the chart.
   * - smoothed: The points are connected with a smoothed curve.
   * - straight: Each point is connected directly with a straight line.
   * @default 'straight'
   */
  lineStyle?: 'smoothed' | 'straight'
}

/**
 * `LineChart` is used to display data on a line chart.
 */
export const LineChart = forwardRef<EChartsReact, LineChartProps>(
  ({ dataset, lineStyle = 'straight', ...restProps }, ref) => {
    const option: EChartsOption = {
      xAxis: { boundaryGap: false },
    }

    const seriesOption: LineSeriesOption = {
      smooth: lineStyle === 'smoothed',
      symbol: 'circle',
      type: 'line',
    }

    return (
      <BaseChart
        dataset={dataset}
        option={option}
        orientation="vertical"
        seriesOption={seriesOption}
        ref={ref}
        {...restProps}
      />
    )
  }
)
