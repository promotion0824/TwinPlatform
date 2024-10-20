import React from 'react'
import type { Meta, StoryObj } from '@storybook/react'
import { noop } from 'lodash'
import { Group, SparklineProps, Stack } from '@willowinc/ui'
import { NumberTile, NumberTileProps } from './NumberTile'

const meta: Meta<typeof NumberTile> = {
  component: NumberTile,
}

export default meta

type Story = StoryObj<typeof NumberTile>

const sparklineData: SparklineProps = {
  labels: [
    '2024-01-01',
    '2024-01-02',
    '2024-01-03',
    '2024-01-04',
    '2024-01-05',
    '2024-01-06',
    '2024-01-07',
    '2024-01-08',
    '2024-01-09',
    '2024-01-10',
  ],
  dataset: [
    {
      name: 'Building 1',
      data: [54, 85, 1, 90, 34, 57, 71, 58, 80, 60],
    },
  ],
}

const baseProps: NumberTileProps = {
  description:
    'The estimated yearly cost that could be avoided if the unresolved insights were resolved.',
  label: 'Estimated Avoidable Cost',
  onClick: noop,
  trendingInfo: {
    sentiment: 'positive',
    trend: 'upwards',
    value: '5%',
  },
  unit: 'USD',
  value: '327K',
}

const sentiments = ['negative', 'neutral', 'notice', 'positive'] as const
const trends = ['downwards', 'sidewards', 'upwards'] as const

export const Playground: Story = {
  args: baseProps,
}

export const NonInteractive: Story = {
  args: {
    ...baseProps,
    onClick: undefined,
  },
}

export const BadgeVariants: Story = {
  render: () => (
    <Stack>
      <Group wrap="nowrap">
        {sentiments.map((sentiment) => (
          <NumberTile
            {...baseProps}
            key={sentiment}
            trendingInfo={{
              sentiment,
              trend: 'upwards',
              value: '5%',
            }}
          />
        ))}
      </Group>

      <Group wrap="nowrap">
        {trends.map((trend) => (
          <NumberTile
            {...baseProps}
            key={trend}
            trendingInfo={{
              sentiment: 'neutral',
              trend,
              value: '5%',
            }}
          />
        ))}
      </Group>
    </Stack>
  ),
}

export const LargeSize: Story = {
  args: {
    ...baseProps,
    size: 'large',
  },
}

export const NoBadge: Story = {
  render: () => (
    <Stack>
      <NumberTile {...baseProps} trendingInfo={undefined} />
      <NumberTile {...baseProps} size="large" trendingInfo={undefined} />
    </Stack>
  ),
}

export const NoDescription: Story = {
  args: {
    ...baseProps,
    description: undefined,
  },
}

export const WithSparkline: Story = {
  args: {
    ...baseProps,
    size: 'large',
    sparkline: sparklineData,
  },
}
