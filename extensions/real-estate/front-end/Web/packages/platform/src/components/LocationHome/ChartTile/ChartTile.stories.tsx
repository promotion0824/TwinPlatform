import type { Meta, StoryObj } from '@storybook/react'
import { BarChart, LineChart, PieChart } from '@willowinc/ui'
import React from 'react'
import { ChartTile } from './ChartTile'

const meta: Meta<typeof ChartTile> = {
  component: ChartTile,
}

export default meta

type Story = StoryObj<typeof ChartTile>

export const PieChartMinimum: Story = {
  args: {
    chart: (
      <PieChart
        dataset={[
          {
            data: [40, 60],
            name: 'Energy Usage',
          },
        ]}
        labels={['Air Cooled Chiller', 'Electrical Circuit']}
        position="left"
        showLabel={false}
      />
    ),
    description: 'Pie Chart Minimum',
    title: 'Pie Chart Minimum',
    w: 400,
  },
}

export const PieChartMaximum: Story = {
  args: {
    chart: (
      <PieChart
        dataset={[
          {
            data: [15, 45, 30, 45, 60, 166],
            name: 'Energy Usage',
          },
        ]}
        labels={[
          'Air Cooled Chiller',
          'Electrical Circuit',
          'Electricl Panelboard',
          'Elevator',
          'Switchboard',
          'Other',
        ]}
        position="left"
        showLabel={false}
      />
    ),
    description: 'Pie Chart Maximum',
    title: 'Pie Chart Maximum',
    w: 400,
  },
}

export const HorizontalBarChartMinimum: Story = {
  args: {
    chart: (
      <BarChart
        dataset={[
          {
            data: [100, 90],
            name: 'Energy Usage',
          },
        ]}
        labels={['Air Cooled Chiller', 'Electrical Circuit']}
      />
    ),
    description: 'Horizontal Bar Chart Minimum',
    title: 'Horizontal Bar Chart Minimum',
    w: 400,
  },
}

export const HorizontalBarChartMaximum: Story = {
  args: {
    chart: (
      <BarChart
        dataset={[
          {
            data: [100, 90, 65, 25, 5],
            name: 'Energy Usage',
          },
        ]}
        labels={[
          'Air Cooled Chiller',
          'Electrical Circuit',
          'Electricl Panelboard',
          'Elevator',
          'Other',
        ]}
      />
    ),
    description: 'Horizontal Bar Chart Maximum',
    title: 'Horizontal Bar Chart Maximum',
    w: 400,
  },
}

export const VerticalBarChartMinimum: Story = {
  args: {
    chart: (
      <BarChart
        dataset={[
          {
            data: [60, 80],
            name: 'Energy Usage',
          },
        ]}
        labels={['Air Cooled Chiller', 'Electrical Circuit']}
        orientation="vertical"
      />
    ),
    description: 'Vertical Bar Chart Minimum',
    title: 'Vertical Bar Chart Minimum',
    w: 400,
  },
}

export const VerticalBarChartMaximum: Story = {
  args: {
    chart: (
      <BarChart
        dataset={[
          {
            data: [60, 80, 70, 50, 90],
            name: 'Energy Usage',
          },
        ]}
        labels={[
          '2024-01-01',
          '2024-02-01',
          '2024-03-01',
          '2024-04-01',
          '2024-05-01',
        ]}
        labelsType="time"
        orientation="vertical"
      />
    ),
    description: 'Vertical Bar Chart Maximum',
    title: 'Vertical Bar Chart Maximum',
    w: 400,
  },
}

export const LineChartChartMinimum: Story = {
  args: {
    chart: (
      <LineChart
        dataset={[
          {
            data: [40, 70, 40, 60, 50, 80],
            name: 'Air Cooled Chiller',
          },
        ]}
        labels={[
          '2024-01-01',
          '2024-02-01',
          '2024-03-01',
          '2024-04-01',
          '2024-05-01',
          '2024-06-01',
        ]}
        labelsType="time"
      />
    ),
    description: 'Line Chart Chart Minimum',
    title: 'Line Chart Chart Minimum',
    w: 400,
  },
}

export const LineChartChartMaximum: Story = {
  args: {
    chart: (
      <LineChart
        dataset={[
          {
            data: [40, 70, 40, 60, 50, 80],
            name: 'Air Cooled Chiller',
          },
          {
            data: [30, 20, 50, 40, 60, 70],
            name: 'Electrical Circuit',
          },
          {
            data: [78, 14, 84, 56, 60, 78],
            name: 'Electricl Panelboard',
          },
          {
            data: [68, 63, 45, 23, 55, 42],
            name: 'Elevator',
          },
          {
            data: [51, 42, 30, 60, 78, 90],
            name: 'Switchboard',
          },
        ]}
        labels={[
          '2024-01-01',
          '2024-02-01',
          '2024-03-01',
          '2024-04-01',
          '2024-05-01',
          '2024-06-01',
        ]}
        labelsType="time"
      />
    ),
    description: 'Line Chart Chart Maximum',
    title: 'Line Chart Chart Maximum',
    w: 400,
  },
}
