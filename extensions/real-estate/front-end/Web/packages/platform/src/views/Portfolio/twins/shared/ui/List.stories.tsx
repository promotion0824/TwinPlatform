import React from 'react'
import type { Meta, StoryObj } from '@storybook/react'
import List from './List'

const meta: Meta<typeof List> = {
  component: List,
  parameters: { layout: 'fullscreen' },
}

export default meta
type Story = StoryObj<typeof List>

export const WithRows: Story = {
  render: () => (
    <List>
      <li>One</li>
      <li>Two</li>
      <li>Three</li>
    </List>
  ),
}

export const Empty: Story = {
  render: () => <List />,
}
