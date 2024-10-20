import React from 'react'
import type { Meta, StoryObj } from '@storybook/react'
import ExploreTwins from './ExploreTwins'

const meta: Meta<typeof ExploreTwins> = {
  component: ExploreTwins,
  render: (args) => <ExploreTwins useSearchResults={() => args} />,
}

export default meta
type Story = StoryObj<typeof ExploreTwins>

export const NothingSelected: Story = {
  args: {
    modelId: '',
  },
}
export const ModelSelected: Story = {
  args: {
    modelId: 'dtmi:com:willowinc:Asset;1',
  },
}
