import type { Meta, StoryObj } from '@storybook/react'

import { LineChart } from '.'
import { ChartContainerDecorator } from '../../../storybookUtils/ChartContainer'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof LineChart> = {
  title: 'LineChart',
  component: LineChart,
  decorators: [ChartContainerDecorator],
  ...storybookAutoSourceParameters,
}
export default meta

type Story = StoryObj<typeof LineChart>

export const Playground: Story = {
  args: {
    dataset: [
      {
        name: 'Building 1',
        data: [120, 200, 150, 80, 70, 110, 130],
      },
    ],
    labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
  },
}

export const MultipleLines: Story = {
  args: {
    dataset: [
      {
        name: 'Building 1',
        data: [120, 200, 150, 80, 70, 110, 130],
      },
      {
        name: 'Building 2',
        data: [80, 130, 76, 129, 160, 129, 50],
      },
    ],
    labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
  },
}

export const Smoothed: Story = {
  args: {
    dataset: [
      {
        name: 'Building 1',
        data: [120, 200, 150, 80, 70, 110, 130],
      },
      {
        name: 'Building 2',
        data: [80, 130, 76, 129, 160, 129, 50],
      },
    ],
    labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
    lineStyle: 'smoothed',
  },
}

export const TimeSeries: Story = {
  args: {
    dataset: [
      {
        name: 'Building 1',
        data: [120, 200, 150, 80, 70, 110, 130],
      },
      {
        name: 'Building 2',
        data: [80, 130, 76, 129, 160, 129, 50],
      },
    ],
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
  },
}
