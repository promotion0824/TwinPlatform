import type { Meta, StoryObj } from '@storybook/react'

import { AreaChart } from '.'
import { ChartContainerDecorator } from '../../../storybookUtils/ChartContainer'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof AreaChart> = {
  title: 'AreaChart',
  component: AreaChart,
  decorators: [ChartContainerDecorator],
  args: {
    dataset: [
      {
        data: [44, 42, 52, 61, 60, 50, 58, 42, 62, 56, 70, 80],
        name: 'Score',
      },
    ],
    labels: [
      'Jan',
      'Feb',
      'Mar',
      'Apr',
      'May',
      'Jun',
      'Jul',
      'Aug',
      'Sep',
      'Oct',
      'Nov',
      'Dec',
    ],
  },
  ...storybookAutoSourceParameters,
}

export default meta

type Story = StoryObj<typeof AreaChart>

export const Playground: Story = {}

export const Smoothed: Story = {
  args: {
    lineStyle: 'smoothed',
  },
}

export const MultipleDatasets: Story = {
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

export const LineOverlay: Story = {
  args: {
    lineOverlays: [
      {
        data: [23, 20, 21, 30, 25, 22, 24, 20, 21, 30, 25, 22],
        name: 'Temperature (°C)',
      },
    ],
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
