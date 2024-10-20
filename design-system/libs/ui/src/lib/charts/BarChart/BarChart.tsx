import { EChartsOption, BarSeriesOption } from 'echarts'
import EChartsReact from 'echarts-for-react'
import { forwardRef } from 'react'
import {
  BaseChart,
  BaseChartProps,
  ChartDataset,
  IntentThresholds,
  willowIntentVisualMap,
} from '../utils/chartUtils'

export interface BarChartProps extends BaseChartProps {
  /** The data to be displayed in the chart. It only supports a single dataset. */
  dataset: [ChartDataset[0]]
  /**
   * Set the thresholds where the colors should change when using the "willow-intent" theme.
   * @default { positiveThreshold: 100, noticeThreshold: 75 }
   */
  intentThresholds?: IntentThresholds
  /**
   * Shows a shadow on the bar that's being hovered over.
   * @default true
   */
  showHighlightShadow?: boolean
  /**
   * Specify the theme to be used for the chart.
   * @default 'willow'
   */
  theme?: 'willow' | 'willow-intent'
}

/**
 * `BarChart` is a chart that presents the comparisons among discrete data.
 * The length of the bars is proportionally related to the categorical data.
 */
export const BarChart = forwardRef<EChartsReact, BarChartProps>(
  (
    {
      intentThresholds = {
        positiveThreshold: 100,
        noticeThreshold: 75,
      },
      showHighlightShadow = true,
      theme = 'willow',
      tooltipEnabled,
      ...restProps
    },
    ref
  ) => {
    const option: EChartsOption = {
      tooltip: {
        trigger: !tooltipEnabled
          ? 'none'
          : showHighlightShadow
          ? 'axis'
          : 'item',
        axisPointer: {
          type: showHighlightShadow ? 'shadow' : 'none',
        },
      },
      ...(theme === 'willow-intent' && {
        visualMap: willowIntentVisualMap(intentThresholds),
      }),
    }

    const seriesOption: BarSeriesOption = { type: 'bar' }

    return (
      <BaseChart
        option={option}
        preserveAxisLabelAlignment
        seriesOption={seriesOption}
        ref={ref}
        {...restProps}
      />
    )
  }
)
