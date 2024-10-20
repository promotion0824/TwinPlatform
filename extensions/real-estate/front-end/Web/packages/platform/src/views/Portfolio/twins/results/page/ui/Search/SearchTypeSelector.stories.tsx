import React from 'react'
import type { Meta, StoryObj } from '@storybook/react'
import SearchTypeSelector from './SearchTypeSelector'

const meta: Meta<typeof SearchTypeSelector> = {
  component: SearchTypeSelector,
  render: (args) => (
    <SearchTypeSelector useSearchResults={() => ({ t: (_) => _, ...args })} />
  ),
}

export default meta
type Story = StoryObj<typeof SearchTypeSelector>

export const Default: Story = {}

export const TwinsSelected: Story = {
  args: {
    modelId: 'dtmi:com:willowinc:Asset;1',
  },
}
export const FilesSelected: Story = {
  args: {
    modelId: 'dtmi:com:willowinc:Document;1',
  },
}
export const SensorsSelected: Story = {
  args: {
    modelId: 'dtmi:com:willowinc:Capability;1',
  },
}
