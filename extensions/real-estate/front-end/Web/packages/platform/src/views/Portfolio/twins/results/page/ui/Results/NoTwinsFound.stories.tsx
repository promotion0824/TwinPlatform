import type { Meta, StoryObj } from '@storybook/react'
import NoTwinsFound from './NoTwinsFound'

const meta: Meta<typeof NoTwinsFound> = {
  component: NoTwinsFound,
  render: (args) => (
    <NoTwinsFound t={(_) => _} onRegisterInterest={() => {}} {...args} />
  ),
}

export default meta
type Story = StoryObj<typeof NoTwinsFound>

export const HasNotRegisteredInterest: Story = {
  args: { hasRegisteredInterest: false },
}

export const HasRegisteredInterest: Story = {
  args: { hasRegisteredInterest: true },
}
