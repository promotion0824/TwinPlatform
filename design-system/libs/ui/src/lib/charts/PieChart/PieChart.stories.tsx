import type { Meta, StoryObj } from '@storybook/react'

import { PieChart, PieChartProps } from '.'
import {
  ChartContainer,
  ChartContainerDecorator,
} from '../../../storybookUtils/ChartContainer'
import { Stack } from '../../layout/Stack'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof PieChart> = {
  title: 'PieChart',
  component: PieChart,
  args: {
    dataset: [
      {
        data: [120, 200, 150, 80, 70, 110, 130],
        name: 'Building 1',
      },
    ],
    labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
  },
}

export default meta

type Story = StoryObj<typeof PieChart>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  decorators: [ChartContainerDecorator],
}

export const ShowLabel: Story = {
  ...storybookAutoSourceParameters,
  args: {
    showLabels: true,
  },
  decorators: [ChartContainerDecorator],
}

const layouts: Array<PieChartProps['layout']> = ['horizontal', 'vertical']
const positions: Array<PieChartProps['position']> = ['left', 'center', 'right']

export const Layout: Story = {
  render: (args) => {
    return (
      <Stack gap={12}>
        {layouts.flatMap((layout: PieChartProps['layout']) =>
          positions.map((position: PieChartProps['position']) => (
            <ChartContainer
              data-testid="pie-chart"
              key={`piechart_story_${layout}_${position}`}
            >
              <PieChart
                layout={layout}
                position={position}
                dataset={args.dataset}
                labels={args.labels}
              />
            </ChartContainer>
          ))
        )}
      </Stack>
    )
  },
}
