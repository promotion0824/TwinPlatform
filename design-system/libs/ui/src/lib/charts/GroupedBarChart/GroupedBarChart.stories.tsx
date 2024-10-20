import type { Meta, StoryObj } from '@storybook/react'

import { GroupedBarChart } from '.'
import {
  ChartContainer,
  ChartContainerDecorator,
} from '../../../storybookUtils/ChartContainer'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof GroupedBarChart> = {
  title: 'GroupedBarChart',
  component: GroupedBarChart,
}
export default meta

type Story = StoryObj<typeof GroupedBarChart>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {
    dataset: [
      { name: 'Building 1', data: [33, 52, 63, 44] },
      { name: 'Building 2', data: [85, 45, 51, 58] },
      { name: 'Building 3', data: [62, 64, 48, 68] },
    ],
    labels: ['Mon', 'Tue', 'Wed', 'Thu'],
  },
  decorators: [ChartContainerDecorator],
}

export const ColumnChart: Story = {
  ...storybookAutoSourceParameters,
  args: {
    dataset: [
      { name: 'Building 1', data: [50, 33, 59, 20, 88, 50, 43] },
      { name: 'Building 2', data: [80, 58, 73, 78, 46, 67, 31] },
      { name: 'Building 3', data: [58, 35, 54, 59, 33, 50, 62] },
    ],
    labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
    orientation: 'vertical',
  },
  decorators: [ChartContainerDecorator],
}

export const PositiveAndNegativeValues: Story = {
  render: () => (
    <div
      style={{
        display: 'flex',
        flexDirection: 'column',
        gap: '1rem',
        height: '100%',
      }}
    >
      <ChartContainer data-testid="row-positive-and-negative">
        <GroupedBarChart
          dataset={[
            { name: 'Building 1', data: [33, -40, -52, 48] },
            { name: 'Building 2', data: [54, 40, -43, -46] },
            { name: 'Building 3', data: [-19, -61, 29, -53] },
          ]}
          labels={['Mon', 'Tue', 'Wed', 'Thu']}
        />
      </ChartContainer>
      <ChartContainer data-testid="column-positive-and-negative">
        <GroupedBarChart
          dataset={[
            { name: 'Building 1', data: [50, -33, -25, 49, -35, 60, 58] },
            { name: 'Building 2', data: [-74, 58, 77, -64, -64, 59, 60] },
            { name: 'Building 3', data: [-50, 31, 56, 31, 39, -48, 49] },
          ]}
          labels={['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']}
          orientation="vertical"
        />
      </ChartContainer>
    </div>
  ),
}

export const NegativeValues: Story = {
  render: () => (
    <div
      style={{
        display: 'flex',
        flexDirection: 'column',
        gap: '1rem',
        height: '100%',
      }}
    >
      <ChartContainer data-testid="row-negative">
        <GroupedBarChart
          dataset={[
            { name: 'Building 1', data: [-33, -52, -63, -44] },
            { name: 'Building 2', data: [-85, -45, -51, -58] },
            { name: 'Building 3', data: [-62, -64, -48, -68] },
          ]}
          labels={['Mon', 'Tue', 'Wed', 'Thu']}
        />
      </ChartContainer>
      <ChartContainer data-testid="column-negative">
        <GroupedBarChart
          dataset={[
            { name: 'Building 1', data: [-50, -33, -59, -20, -88, -50, -43] },
            { name: 'Building 2', data: [-80, -58, -73, -78, -46, -67, -31] },
            { name: 'Building 3', data: [-58, -35, -54, -59, -33, -50, -62] },
          ]}
          labels={['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']}
          orientation="vertical"
        />
      </ChartContainer>
    </div>
  ),
}

export const LineOverlay: Story = {
  ...storybookAutoSourceParameters,
  args: {
    dataset: [
      { name: 'Building 1', data: [33, 52, 63, 44] },
      { name: 'Building 2', data: [85, 45, 51, 58] },
      { name: 'Building 3', data: [62, 64, 48, 68] },
    ],
    labels: ['Mon', 'Tue', 'Wed', 'Thu'],
    lineOverlays: [
      {
        data: [23, 20, 21, 18],
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
    dataset: [
      { name: 'Building 1', data: [33, 52, 63, 44] },
      { name: 'Building 2', data: [85, 45, 51, 58] },
      { name: 'Building 3', data: [62, 64, 48, 68] },
    ],
    labels: ['2024-01-01', '2024-02-01', '2024-03-01', '2024-04-01'],
    labelsType: 'time',
    orientation: 'vertical',
  },
  decorators: [ChartContainerDecorator],
}
