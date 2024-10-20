import React from 'react'
import { Meta, StoryObj } from '@storybook/react'
import Search from './Search'

const meta: Meta<typeof Search> = {
  component: Search,
}

export default meta
type Story = StoryObj<typeof Search>

export const Default: Story = {
  render: () => <Search useTwinsLanding={() => ({ t: (_) => _ })} />,
}

export const WithTerm: Story = {
  render: () => (
    <Search useTwinsLanding={() => ({ t: (_) => _ })} term="My search term" />
  ),
}
