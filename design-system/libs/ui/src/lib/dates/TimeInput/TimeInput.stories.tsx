import type { Meta, StoryObj } from '@storybook/react'

import dayjs from 'dayjs'
import { useState } from 'react'

import { TimeInput } from '.'
import { storyContainerTestId } from '../../../storybookUtils/StoryContainers'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof TimeInput> = {
  title: 'TimeInput',
  component: TimeInput,
  decorators: [
    (Story) => (
      <div
        data-testid={storyContainerTestId}
        css={{
          height: 300,
        }}
      >
        <Story />
      </div>
    ),
  ],
}
export default meta

type Story = StoryObj<typeof TimeInput>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
}

export const DefaultValue: Story = {
  ...storybookAutoSourceParameters,
  args: {
    defaultValue: '08:00',
  },
}

export const Controlled: Story = {
  render: () => {
    const [value, setValue] = useState('')
    return <TimeInput value={value} onChange={setValue} />
  },
}

export const Readonly: Story = {
  ...storybookAutoSourceParameters,
  args: {
    defaultValue: '08:00',
    readOnly: true,
  },
}

export const WithLabel: Story = {
  ...storybookAutoSourceParameters,
  args: {
    defaultValue: '08:00',
    label: 'Time Input Label',
  },
}

export const WithDescription: Story = {
  ...storybookAutoSourceParameters,
  args: {
    defaultValue: '08:00',
    description: 'Description text',
  },
}

export const HorizontalLayout: Story = {
  ...storybookAutoSourceParameters,
  args: {
    layout: 'horizontal',
    defaultValue: '08:00',
    label: 'Time Input Label',
    description: 'Description text',
  },
}

export const HorizontalLayoutWithLabelWidth: Story = {
  ...storybookAutoSourceParameters,
  args: {
    layout: 'horizontal',
    defaultValue: '08:00',
    label: 'Time Input Label',
    labelWidth: 300,
    description: 'Description text',
  },
}

export const WithError: Story = {
  ...storybookAutoSourceParameters,
  args: {
    defaultValue: '08:00',
    error: 'Error Message',
  },
}

export const Disabled: Story = {
  ...storybookAutoSourceParameters,
  args: {
    defaultValue: '08:00',
    disabled: true,
  },
}

export const With1HourInterval: Story = {
  ...storybookAutoSourceParameters,
  args: {
    interval: 60 * 60 * 1000,
  },
}

export const WithSecondsFormat: Story = {
  ...storybookAutoSourceParameters,
  args: {
    format: 'hh:mm:ss a',
  },
}

export const In24HoursFormat: Story = {
  ...storybookAutoSourceParameters,
  args: {
    format: 'HH:mm',
  },
}

export const DisabledTimeItem: Story = {
  render: () => (
    <TimeInput
      getTimes={(times) =>
        times.map((time) => {
          return {
            ...time,
            disabled: time.value.includes(':15'),
          }
        })
      }
    />
  ),
}

export const FilteredTimeItem: Story = {
  render: () => (
    <TimeInput
      getTimes={(times) =>
        times.filter(
          // filter out times between 9:00 and 17:00
          (time) =>
            dayjs(`2001-01-01 ${time.value}`).hour() > 8 &&
            dayjs(`2001-01-01 ${time.value}`) <= dayjs(`2001-01-01 17:00:00`)
        )
      }
    />
  ),
}
