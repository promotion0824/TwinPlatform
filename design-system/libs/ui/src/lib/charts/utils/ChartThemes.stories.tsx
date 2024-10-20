import type { Meta, StoryObj } from '@storybook/react'

import { BarChart } from '../BarChart'
import { ChartCard } from '../ChartCard'
import { ChartContainerDecorator } from '../../../storybookUtils/ChartContainer'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof ChartCard> = {
  title: 'Chart Themes',
  component: ChartCard,
  decorators: [ChartContainerDecorator],
  ...storybookAutoSourceParameters,
}
export default meta

type Story = StoryObj<typeof ChartCard>

/**
 * By default the `willow` theme will be used.
 */
export const Default: Story = {
  args: {
    chart: (
      <BarChart
        dataset={[
          {
            data: [100, 88, 76, 62, 40, 23],
            name: 'Performance Score',
          },
        ]}
        labels={[
          'Building A',
          'Building B',
          'Building C',
          'Building D',
          'Building E',
          'Building F',
        ]}
      />
    ),
    title: 'Default Theme',
  },
}

/**
 * Setting `willow-intent` will use the Intent theme on `BarChart`.
 */
export const Intent: Story = {
  args: {
    chart: (
      <BarChart
        dataset={[
          {
            data: [100, 88, 76, 62, 40, 23],
            name: 'Performance Score',
          },
        ]}
        labels={[
          'Building A',
          'Building B',
          'Building C',
          'Building D',
          'Building E',
          'Building F',
        ]}
        theme="willow-intent"
      />
    ),
    title: 'Intent',
  },
}

/**
 * Setting the `intentThresholds` prop allows the thresholds to be configured.
 */
export const IntentThresholds: Story = {
  args: {
    chart: (
      <BarChart
        dataset={[
          {
            data: [100, 88, 76, 62, 40, 23],
            name: 'Performance Score',
          },
        ]}
        labels={[
          'Building A',
          'Building B',
          'Building C',
          'Building D',
          'Building E',
          'Building F',
        ]}
        intentThresholds={{ positiveThreshold: 80, noticeThreshold: 60 }}
        theme="willow-intent"
      />
    ),
    title: 'Intent Thresholds',
  },
}
