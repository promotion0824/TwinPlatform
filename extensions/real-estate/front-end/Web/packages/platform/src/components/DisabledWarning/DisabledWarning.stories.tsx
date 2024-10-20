import type { Meta, StoryObj } from '@storybook/react'
import DisabledWarning from './DisabledWarning'

const meta: Meta<typeof DisabledWarning> = {
  component: DisabledWarning,
}

export default meta
type Story = StoryObj<typeof DisabledWarning>

export const Basic: Story = {
  args: {
    title: 'Warning title',
    icon: 'password',
    children: undefined,
  },
}
