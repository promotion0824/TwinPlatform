import type { Meta, StoryObj } from '@storybook/react'

import { StackedBarChart } from '.'
import {
  ChartContainer,
  ChartContainerDecorator,
} from '../../../storybookUtils/ChartContainer'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof StackedBarChart> = {
  title: 'StackedBarChart',
  component: StackedBarChart,
  args: {
    dataset: [
      { name: 'Direct', data: [320, 302, 301, 334, 390, 330, 320] },
      { name: 'Mail Ad', data: [120, 132, 101, 134, 90, 230, 210] },
      { name: 'Affiliate Ad', data: [220, 182, 191, 234, 290, 330, 310] },
      { name: 'Video Ad', data: [150, 212, 201, 154, 190, 330, 410] },
      { name: 'Search Engine', data: [820, 832, 901, 934, 1290, 1330, 1320] },
    ],
    labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
  },
}
export default meta

type Story = StoryObj<typeof StackedBarChart>

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
    const dataset = [
      { name: 'Direct', data: [-120, -212, -301, 334, -190, -230, 410] },
      { name: 'Mail Ad', data: [320, 302, 101, 134, -390, 330, 320] },
      { name: 'Affiliate Ad', data: [220, 132, 191, 234, 90, 330, 210] },
      { name: 'Video Ad', data: [150, 182, 201, 154, 290, 330, 310] },
      { name: 'Search Engine', data: [820, 832, 901, 934, 1290, 1330, 1320] },
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
          <StackedBarChart labels={args.labels} dataset={dataset} />
        </ChartContainer>
        <ChartContainer data-testid="column-with-negatives">
          <StackedBarChart
            dataset={dataset}
            labels={args.labels}
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
        name: 'Average Temperature (°C)',
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
