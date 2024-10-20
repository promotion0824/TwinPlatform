import type { StoryObj } from '@storybook/react'

import { TimezoneSelector } from './TimezoneSelector'

const defaultStory = {
  component: TimezoneSelector,
  title: 'TimezoneSelector',
}

export default defaultStory

type Story = StoryObj<typeof TimezoneSelector>

export const HiddenStoryName: Story = {
  render: () => <TimezoneSelector />,
}
