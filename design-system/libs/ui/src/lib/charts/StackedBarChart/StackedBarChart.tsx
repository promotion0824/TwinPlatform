import { EChartsOption, BarSeriesOption } from 'echarts'
import EChartsReact from 'echarts-for-react'
import { forwardRef } from 'react'
import { BaseChart, BaseChartProps } from '../utils/chartUtils'

export interface StackedBarChartProps extends BaseChartProps {
  /**
   * Highlight all items in the same series when hovering over an item.
   * @default true
   */
  emphasizeFocusedSeries?: boolean
  /**
   * Show labels on each bar segment.
   * @default true
   */
  showLabels?: boolean
}

/**
 * `StackedBarChart` is a chart where data in the same category will be stacked up in one row/column.
 */
export const StackedBarChart = forwardRef<EChartsReact, StackedBarChartProps>(
  (
    { dataset, emphasizeFocusedSeries = true, showLabels = true, ...restProps },
    ref
  ) => {
    const option: EChartsOption = {
      tooltip: {
        axisPointer: {
          type: 'shadow',
        },
      },
    }

    const seriesOption: BarSeriesOption = {
      emphasis: { focus: emphasizeFocusedSeries ? 'series' : 'none' },
      label: { show: showLabels },
      stack: 'total',
      type: 'bar',
    }

    return (
      <BaseChart
        dataset={dataset}
        option={option}
        preserveAxisLabelAlignment
        seriesOption={seriesOption}
        ref={ref}
        {...restProps}
      />
    )
  }
)
