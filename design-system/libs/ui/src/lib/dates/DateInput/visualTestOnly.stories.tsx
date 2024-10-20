import type { Meta, StoryObj } from '@storybook/react'

import { DateInput } from '.'

const defaultStory: Meta<typeof DateInput> = {
  component: DateInput,
  title: 'DateInput',
}

export default defaultStory

type Story = StoryObj<typeof DateInput>

export const HiddenDefaultDate: Story = {
  args: {
    // this defaultDate will impact visual regression tests
    defaultDate: new Date('2023-01-01'),
  },
}

export const HiddenDisabledDate: Story = {
  args: {
    // this defaultDate will impact visual regression tests
    defaultDate: new Date('2023-01-01'),
    excludeDate: (date) => date.getDate() > 22 || date.getDate() < 10,
  },
}
