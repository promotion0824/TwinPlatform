import type { Meta, StoryObj } from '@storybook/react'

import { BarChart, BarChartProps } from '.'
import {
  ChartContainer,
  ChartContainerDecorator,
} from '../../../storybookUtils/ChartContainer'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof BarChart> = {
  title: 'BarChart',
  component: BarChart,
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

type Story = StoryObj<typeof BarChart>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  decorators: [ChartContainerDecorator],
}

export const ColumnChart: Story = {
  ...storybookAutoSourceParameters,
  args: {
    orientation: 'vertical',
  },
  decorators: [ChartContainerDecorator],
}

export const NegativeValues: Story = {
  render: (args) => {
    const dataset: BarChartProps['dataset'] = [
      {
        data: [-0.07, -0.09, 0.2, 0.44, -0.23, 0.08, -0.17],
        name: 'Building 1',
      },
    ]

    return (
      <div
        style={{
          display: 'flex',
          flexDirection: 'column',
          gap: '1rem',
          height: '100%',
        }}
      >
        <ChartContainer data-testid="row-with-negatives">
          <BarChart labels={args.labels} dataset={dataset} />
        </ChartContainer>
        <ChartContainer data-testid="column-with-negatives">
          <BarChart
            labels={args.labels}
            dataset={dataset}
            orientation="vertical"
          />
        </ChartContainer>
      </div>
    )
  },
}

export const LineOverlay: Story = {
  ...storybookAutoSourceParameters,
  args: {
    lineOverlays: [
      {
        data: [23, 20, 21, 30, 25, 22, 24],
        name: 'Temperature (°C)',
      },
    ],
    orientation: 'vertical',
  },
  decorators: [ChartContainerDecorator],
}

export const TimeSeries: Story = {
  ...storybookAutoSourceParameters,
  args: {
    labels: [
      '2024-01-01',
      '2024-02-01',
      '2024-03-01',
      '2024-04-01',
      '2024-05-01',
      '2024-06-01',
      '2024-07-01',
    ],
    labelsType: 'time',
    orientation: 'vertical',
  },
  decorators: [ChartContainerDecorator],
}
