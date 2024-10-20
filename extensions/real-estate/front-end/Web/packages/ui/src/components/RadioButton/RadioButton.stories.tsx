import type { Meta, StoryObj } from '@storybook/react'
import RadioButton from './RadioButton'

const meta: Meta<typeof RadioButton> = {
  component: RadioButton,
  render: (args) => <RadioButton {...args} />,
}

export default meta
type Story = StoryObj<typeof RadioButton>

export const BasicRadioButton: Story = {
  args: {
    checked: true,
  },
}
