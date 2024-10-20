import { EChartsOption, BarSeriesOption } from 'echarts'
import EChartsReact from 'echarts-for-react'
import { forwardRef } from 'react'
import { BaseChart, BaseChartProps } from '../utils/chartUtils'

export interface GroupedBarChartProps extends BaseChartProps {}

/**
 * `GroupedBarChart` is a bar chart where the data is shown is groups across the different categories.
 */
export const GroupedBarChart = forwardRef<EChartsReact, GroupedBarChartProps>(
  ({ dataset, ...restProps }, ref) => {
    const option: EChartsOption = {
      tooltip: {
        axisPointer: {
          type: 'shadow',
        },
      },
    }

    const seriesOption: BarSeriesOption = { type: 'bar' }

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
