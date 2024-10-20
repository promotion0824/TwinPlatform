import type { Meta, StoryObj } from '@storybook/react'

import { MetricCard } from '.'
import { storybookAutoSourceParameters } from '../../utils/constant'

const MetricCardDecorator = (Story: React.ComponentType) => (
  <div data-testid="metric-card-decorator" style={{ width: '400px' }}>
    <Story />
  </div>
)

const meta: Meta<typeof MetricCard> = {
  title: 'MetricCard',
  component: MetricCard,
  args: {
    title: 'Total assets',
    trendDifference: 14.29,
    trendDirection: 'upwards',
    trendSentiment: 'positive',
    trendValue: 1000,
    value: 8000,
  },
  decorators: [MetricCardDecorator],
  ...storybookAutoSourceParameters,
}
export default meta

type Story = StoryObj<typeof MetricCard>

export const Playground: Story = {}

export const NegativeSentiment: Story = {
  args: {
    trendDifference: -14.29,
    trendDirection: 'downwards',
    trendSentiment: 'negative',
    trendValue: -1000,
    value: 6000,
  },
}

export const NoticeSentiment: Story = {
  args: {
    trendDifference: 1.43,
    trendDirection: 'downwards',
    trendSentiment: 'notice',
    trendValue: -500,
    value: 6500,
  },
}

export const NeutralSentiment: Story = {
  args: {
    trendDifference: 1.43,
    trendDirection: 'sidewards',
    trendSentiment: 'neutral',
    trendValue: 100,
    value: 7100,
  },
}

export const Units: Story = {
  args: {
    title: 'Digital twins',
    units: 'twins',
    value: 22_000,
  },
}

export const Description: Story = {
  args: {
    description: 'The total number of all assets.',
  },
}

export const WithoutThousandsSeparator: Story = {
  args: {
    showThousandsSeparator: false,
  },
}
