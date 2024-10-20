import type { Meta, StoryObj } from '@storybook/react'
import EChartsReact from 'echarts-for-react'
import { useRef } from 'react'
import { BarChart, BarChartProps } from '../BarChart'
import { useResizeChartInGrid } from '../utils'

const meta: Meta<typeof BarChart> = {
  title: 'useResizeChartInGrid',
  component: BarChart,
}

export default meta

type Story = StoryObj<typeof BarChart>

/**
 * When using charts inside grid layouts (and possibly some flexbox layouts), charts will not
 * resize reliably by themselves as the container is resized. To fix this you can use the
 * `useResizeChartInGrid` hook for each chart inside the container as shown below, which
 * will sync up the resizing for you.
 */
export const ChartResizing: Story = {
  render: () => {
    const chartOneRef = useRef<EChartsReact>(null)
    const chartTwoRef = useRef<EChartsReact>(null)
    const chartThreeRef = useRef<EChartsReact>(null)
    const containerRef = useRef<HTMLDivElement>(null)

    useResizeChartInGrid(chartOneRef, containerRef)
    useResizeChartInGrid(chartTwoRef, containerRef)
    useResizeChartInGrid(chartThreeRef, containerRef)

    const dataset: BarChartProps['dataset'] = [
      { data: [120, 200, 150, 80, 70, 110, 130], name: 'Building 1' },
    ]

    const labels = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']

    return (
      <div
        style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)' }}
        ref={containerRef}
      >
        <div>
          <BarChart dataset={dataset} labels={labels} ref={chartOneRef} />
        </div>
        <div>
          <BarChart dataset={dataset} labels={labels} ref={chartTwoRef} />
        </div>
        <div>
          <BarChart dataset={dataset} labels={labels} ref={chartThreeRef} />
        </div>
      </div>
    )
  },
}
