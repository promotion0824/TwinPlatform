import React from 'react'
import type { Meta, StoryObj } from '@storybook/react'
import FileType from './FileType'

const meta: Meta<typeof FileType> = {
  component: FileType,
  render: (args) => (
    <FileType useSearchResults={() => ({ t: (_) => _, ...args })} />
  ),
}

export default meta
type Story = StoryObj<typeof FileType>

export const Default: Story = {}

export const Selected: Story = {
  args: {
    fileType: 'pdf',
  },
}
