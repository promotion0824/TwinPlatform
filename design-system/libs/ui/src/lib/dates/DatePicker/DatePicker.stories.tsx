import type { Meta, StoryObj } from '@storybook/react'

import { DatePicker } from '.'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof DatePicker> = {
  title: 'DatePicker',
  component: DatePicker,
}
export default meta

type Story = StoryObj<typeof DatePicker>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {},
}

export const WeekdaysOnly: Story = {
  render: () => (
    <DatePicker
      type="range"
      excludeDate={(date) => {
        const day = date.getDay()
        return day === 0 || day === 6
      }}
    />
  ),
}
