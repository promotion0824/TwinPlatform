import type { Meta, StoryObj } from '@storybook/react'
import IndeterminateCheckbox from './IndeterminateCheckbox'

const meta: Meta<typeof IndeterminateCheckbox> = {
  component: IndeterminateCheckbox,
  render: (args) => <IndeterminateCheckbox {...args} />,
}

export default meta
type Story = StoryObj<typeof IndeterminateCheckbox>

export const Checked: Story = { args: { checked: true } }

export const Unchecked: Story = { args: { checked: false } }

export const Indeterminate: Story = { args: { indeterminate: true } }
