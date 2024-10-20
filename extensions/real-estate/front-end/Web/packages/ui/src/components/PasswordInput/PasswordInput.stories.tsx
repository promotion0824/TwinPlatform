import type { Meta, StoryObj } from '@storybook/react'

import PasswordInput from './PasswordInput'

const meta: Meta<typeof PasswordInput> = {
  component: PasswordInput,
  render: (args) => <PasswordInput {...args} />,
}

export default meta
type Story = StoryObj<typeof PasswordInput>

export const Basic: Story = {}
