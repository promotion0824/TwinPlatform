import type { Meta, StoryObj } from '@storybook/react'

import { ChartCard } from '.'
import { ChartContainerDecorator } from '../../../storybookUtils/ChartContainer'
import { AreaChart } from '../AreaChart'
import { BarChart } from '../BarChart'
import { ChartTable, progressColumnType } from '../ChartTable'
import { GroupedBarChart } from '../GroupedBarChart'
import { LineChart } from '../LineChart'
import { StackedBarChart } from '../StackedBarChart'
import { PieChart } from '../PieChart'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof ChartCard> = {
  title: 'ChartCard',
  component: ChartCard,
  ...storybookAutoSourceParameters,
}
export default meta

type Story = StoryObj<typeof ChartCard>

export const Playground: Story = {
  args: {
    chart: (
      <BarChart
        axisLabel="Day"
        dataset={[
          {
            data: [120, 200, 150, 80, 70, 110, 130],
            name: 'Building 1',
          },
        ]}
        labels={['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']}
      />
    ),
    description: 'This is a bar chart',
    title: 'Bar Chart',
  },
  decorators: [ChartContainerDecorator],
}

export const StackedBar: Story = {
  args: {
    chart: (
      <StackedBarChart
        axisLabel="Day"
        dataset={[
          { name: 'Direct', data: [320, 302, 301, 334, 390, 330, 320] },
          { name: 'Mail Ad', data: [120, 132, 101, 134, 90, 230, 210] },
          { name: 'Affiliate Ad', data: [220, 182, 191, 234, 290, 330, 310] },
          { name: 'Video Ad', data: [150, 212, 201, 154, 190, 330, 410] },
          {
            name: 'Search Engine',
            data: [820, 832, 901, 934, 1290, 1330, 1320],
          },
        ]}
        labels={['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']}
      />
    ),
    title: 'Stacked Bar Chart',
  },
  decorators: [ChartContainerDecorator],
}

export const GroupedBar: Story = {
  args: {
    chart: (
      <GroupedBarChart
        axisLabel="Day"
        dataset={[
          { name: 'Building 1', data: [33, 52, 63, 44] },
          { name: 'Building 2', data: [85, 45, 51, 58] },
          { name: 'Building 3', data: [62, 64, 48, 68] },
        ]}
        labels={['Mon', 'Tue', 'Wed', 'Thu']}
      />
    ),
    title: 'Grouped Bar Chart',
  },
  decorators: [ChartContainerDecorator],
}

export const Line: Story = {
  args: {
    chart: (
      <LineChart
        axisLabel="Day"
        dataset={[
          {
            name: 'Building 1',
            data: [120, 200, 150, 80, 70, 110, 130],
          },
          {
            name: 'Building 2',
            data: [80, 130, 76, 129, 160, 129, 50],
          },
        ]}
        labels={['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']}
      />
    ),
    title: 'Line Chart',
  },
  decorators: [ChartContainerDecorator],
}

export const Area: Story = {
  args: {
    chart: (
      <AreaChart
        dataset={[
          {
            name: 'Building 1',
            data: [120, 200, 150, 80, 70, 110, 130],
          },
          {
            name: 'Building 2',
            data: [80, 130, 76, 129, 160, 129, 50],
          },
        ]}
        labels={['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']}
      />
    ),
    title: 'Area Chart',
  },
  decorators: [ChartContainerDecorator],
}

export const Table: Story = {
  args: {
    chart: (
      <ChartTable
        columns={[
          {
            field: 'floor',
            headerName: 'Floor',
          },
          {
            ...progressColumnType({
              intentThresholds: {
                noticeThreshold: 50,
                positiveThreshold: 80,
              },
            }),
            field: 'comfortScore',
            headerName: 'Comfort Score',
          },
        ]}
        getRowId={(row) => row.floor}
        rows={[
          {
            floor: 'Level 1',
            comfortScore: 50,
          },
          {
            floor: 'Level 2',
            comfortScore: 55,
          },
          {
            floor: 'Level 3',
            comfortScore: 70,
          },
          {
            floor: 'Level 4',
            comfortScore: 85,
          },
          {
            floor: 'Level 5',
            comfortScore: 40,
          },
          {
            floor: 'Level 6',
            comfortScore: 90,
          },
          {
            floor: 'Level 7',
            comfortScore: 65,
          },
          {
            floor: 'Level 8',
            comfortScore: 70,
          },
        ]}
      />
    ),
    title: 'Chart Table',
  },
}

export const Pie: Story = {
  args: {
    chart: (
      <PieChart
        axisLabel="Day"
        dataset={[
          {
            data: [120, 200, 150, 80, 70, 110, 130],
            name: 'Building 1',
          },
        ]}
        labels={['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']}
      />
    ),
    description: 'This is a pie chart',
    title: 'Pie Chart',
  },
  decorators: [ChartContainerDecorator],
}
