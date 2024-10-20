import type { StoryObj } from '@storybook/react'

import { Pill } from '.'

const defaultStory = {
  component: Pill,
  title: 'Pill',
  args: {
    children: 'Label',
  },
}

export default defaultStory

type Story = StoryObj<typeof Pill>

export const DisabledWithRemovalButton: Story = {
  args: {
    disabled: true,
    withRemoveButton: true,
  },
}

export const SizeMd: Story = {
  args: {
    size: 'md',
    withRemoveButton: true,
  },
}
