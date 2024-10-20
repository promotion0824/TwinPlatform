import React from 'react'
import type { Meta, StoryObj } from '@storybook/react'
import { Group, Icon, Stack } from '@willowinc/ui'
import { noop } from 'lodash'

import CountsTile, { CountsTileProps } from './CountsTile'
import { CountsItemProps } from './components/CountsItem'

const meta: Meta<typeof CountsTile> = {
  component: CountsTile,
}

export default meta

type Story = StoryObj<typeof CountsTile>

const exampleData: CountsItemProps[] = [
  {
    label: 'New',
    value: 14,
    intent: 'secondary',
    icon: {
      name: 'app_badging',
      filled: false,
    },
    onClick: noop,
  },
  {
    label: 'Open',
    value: 14,
    intent: 'secondary',
    icon: {
      name: 'circle',
      filled: false,
    },
    onClick: noop,
  },
  {
    label: 'Overdue',
    value: 14,
    intent: 'negative',
    icon: {
      name: 'release_alert',
    },
    onClick: noop,
  },
  {
    label: 'In Progress',
    value: 14,
    intent: 'primary',
    icon: {
      name: 'clock_loader_40',
      filled: false,
    },
    onClick: noop,
  },
  {
    label: 'Completed',
    value: 14,
    intent: 'positive',
    icon: {
      name: 'check_circle',
    },
    onClick: noop,
  },
  {
    label: 'Critical',
    value: 14,
    intent: 'negative',
    icon: {
      name: 'error',
      filled: false,
    },
    onClick: noop,
  },
  {
    label: 'Closed',
    value: 14,
    intent: 'positive',
    icon: {
      name: 'new_releases',
    },
    onClick: noop,
  },
  {
    label: 'Ticket',
    value: 14,
    intent: 'notice',
  },
]

const generateDataWithUniqueIntent = (): CountsItemProps[] => {
  const intents = [...new Set(exampleData.map((one) => one.intent))]
  return intents.map((intent) => ({
    label: 'Label',
    value: 14,
    intent,
  }))
}

const baseProps: CountsTileProps = {
  breakpoint: 800,
  data: exampleData,
}

export const Playground: Story = {
  args: baseProps,
}

export const Intent: Story = {
  args: {
    ...baseProps,
    data: generateDataWithUniqueIntent(),
  },
}

export const Icons: Story = {
  render: () => (
    <Stack>
      <CountsTile {...baseProps} data={exampleData} />
      <Group mt="s8">
        {exampleData.map((one: CountsItemProps, index) => {
          if (!one.icon) return null
          return (
            <Icon
              // eslint-disable-next-line react/no-array-index-key
              key={`story_counts_item_icon_${index}`}
              icon={one.icon.name}
              filled={one.icon.filled}
            />
          )
        })}
      </Group>
    </Stack>
  ),
}
