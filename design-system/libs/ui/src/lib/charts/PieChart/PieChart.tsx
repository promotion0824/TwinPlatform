import { useTheme } from '@willowinc/theme'
import EChartsReact from 'echarts-for-react'
import { forwardRef } from 'react'
import {
  BaseChartProps,
  ChartDataset,
  ERR_DATA_LENGTH,
} from '../utils/chartUtils'
import { useResizeObserver } from '@mantine/hooks'
import { Box } from '../../misc/Box'

export interface PieChartProps extends BaseChartProps {
  /**
   * Determines whether to show a label.
   * If true, a label will be shown, otherwise, no labels will be shown.
   * @default false
   */
  showLabels?: boolean
  /**
   * position property determines the position of the chart.
   * @default center
   */
  position?: 'left' | 'center' | 'right'
  /**
   * layout property determines the layout of the chart.
   * @default horizontal
   */
  layout?: 'horizontal' | 'vertical'
  /** The data to be displayed in the chart. It only supports a single dataset. */
  dataset: [ChartDataset[0]]
}

// TODO: Maybe update this to use BaseChart.

/**
 * `PieChart` is a component that presents the comparisons among discrete categories.
 * The size of the pie slices is proportionally related to the data values.
 */
export const PieChart = forwardRef<EChartsReact, PieChartProps>(
  (
    {
      axisLabel = 'Category',
      dataset,
      labels,
      layout = 'horizontal',
      position = 'center',
      showLabels = false,
      tooltipEnabled = true,
      ...restProps
    },
    ref
  ) => {
    const theme = useTheme()

    for (const group of dataset) {
      if (group.data.length !== labels.length) {
        throw new Error(ERR_DATA_LENGTH)
      }
    }

    const [containerRef, containerRect] = useResizeObserver()
    const isMobileView =
      containerRect.width <= parseInt(theme.breakpoints.mobile)

    const chartDataset = {
      source: [
        [axisLabel, dataset[0].name],
        ...labels.map((label, index) => [label, dataset[0].data[index]]),
      ],
    }

    // Don't show labels on mobile view
    const _showLabels = !isMobileView ? showLabels : false
    const labelOptions = {
      label: {
        show: _showLabels,
        color: theme.color.neutral.fg.highlight,
      },
      labelLine: {
        show: _showLabels,
      },
      emphasis: {
        label: {
          show: _showLabels,
        },
        labelLine: {
          show: _showLabels,
        },
        scale: false,
      },
    }

    const legendOption =
      layout === 'horizontal'
        ? {
            orient: 'vertical',
            left: 'right',
            top: 'auto',
            textStyle: {
              width: 80,
              overflow: 'break',
            },
          }
        : {
            orient: 'horizontal',
            left: 'left',
            top: 'bottom',
          }

    const option = {
      tooltip: {
        trigger: tooltipEnabled ? 'item' : 'none',
      },
      dataset: chartDataset,
      series: [
        {
          type: 'pie',
          left: position,
          width: position === 'center' ? '100%' : '60%',
          itemStyle: {
            borderColor: theme.color.neutral.bg.panel.default,
            borderWidth: 2,
          },
          ...labelOptions,
        },
      ],
      legend: legendOption,
    }

    return (
      <Box ref={containerRef} h="100%">
        <EChartsReact
          ref={ref}
          option={option}
          style={{ height: '100%' }}
          theme="willow"
          {...restProps}
        />
      </Box>
    )
  }
)
