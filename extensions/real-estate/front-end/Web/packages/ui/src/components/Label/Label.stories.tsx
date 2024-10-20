import type { Meta, StoryObj } from '@storybook/react'
import Label from './Label'

const meta: Meta<typeof Label> = {
  component: Label,
  render: (args) => <Label label="My label" {...args} />,
}

export default meta
type Story = StoryObj<typeof Label>

export const Basic: Story = {}
