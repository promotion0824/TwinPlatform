import type { StoryObj } from '@storybook/react'

import { DateTimeInput } from '.'

const defaultStory = {
  component: DateTimeInput,
  title: 'DateTimeInput',
}

export default defaultStory

type Story = StoryObj<typeof DateTimeInput>

export const Readonly: Story = {
  args: {
    readOnly: true,
  },
}

export const Disabled: Story = {
  args: {
    disabled: true,
  },
}

export const Error: Story = {
  args: {
    error: true,
  },
}

export const CustomPlaceholder: Story = {
  args: {
    placeholder: 'Custom placeholder text',
  },
}

// need to have a fixed calendar date in display for screenshot in testing
const defaultCalendarDate = new Date('2024-02-01')

export const DateType: Story = {
  storyName: 'Date',
  args: { defaultCalendarDate },
}

export const DateRange: Story = {
  args: {
    type: 'date-range',
    defaultCalendarDate,
  },
}
export const DateTime: Story = {
  args: {
    type: 'date-time',
    defaultCalendarDate,
  },
}
export const DateTimeRange: Story = {
  args: {
    type: 'date-time-range',
    defaultCalendarDate,
  },
}
