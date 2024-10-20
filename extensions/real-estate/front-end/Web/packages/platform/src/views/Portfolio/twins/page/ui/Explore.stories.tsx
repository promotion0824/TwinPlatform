import React from 'react'
import type { Meta, StoryObj } from '@storybook/react'
import Explore from './Explore'

const meta: Meta<typeof Explore> = {
  component: Explore,
  parameters: { layout: 'fullscreen' },
}

export default meta
type Story = StoryObj<typeof Explore>

export const Default: Story = {
  render: () => <Explore useTwinsLanding={() => ({ t: (_) => _ })} />,
}
