import { EChartsOption, LineSeriesOption } from 'echarts'
import EChartsReact from 'echarts-for-react'
import { forwardRef } from 'react'
import { BaseChart, BaseChartProps } from '../utils/chartUtils'

export interface AreaChartProps extends Omit<BaseChartProps, 'orientation'> {
  /**
   * The style in which the line is drawn on the chart.
   * - smoothed: The points are connected with a smoothed curve.
   * - straight: Each point is connected directly with a straight line.
   * @default 'straight'
   */
  lineStyle?: 'smoothed' | 'straight'
}

/**`AreaChart` displays data by filling in the area below lines on the chart. */
export const AreaChart = forwardRef<EChartsReact, AreaChartProps>(
  ({ dataset, lineStyle = 'straight', ...restProps }, ref) => {
    const option: EChartsOption = {
      xAxis: { boundaryGap: false },
    }

    const seriesOption: LineSeriesOption = {
      areaStyle: {},
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
