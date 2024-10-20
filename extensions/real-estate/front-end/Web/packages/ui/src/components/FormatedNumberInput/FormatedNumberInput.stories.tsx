import type { Meta, StoryObj } from '@storybook/react'

import FormatedNumberInput from './FormatedNumberInput'

const meta: Meta<typeof FormatedNumberInput> = {
  component: FormatedNumberInput,
  args: {
    label: 'FormatedNumberInput label',
  },
}

export default meta
type Story = StoryObj<typeof FormatedNumberInput>

export const Default: Story = {}

export const Required: Story = {
  args: {
    required: true,
  },
}

export const Placeholder: Story = {
  args: {
    placeholder: 'Add value',
    value: null,
  },
}

export const ReadonlyValue: Story = {
  args: {
    readOnly: true,
    value: '5',
  },
}

export const ReadonlyEmpty: Story = {
  args: {
    readOnly: true,
    value: null,
  },
}

export const DisabledValue: Story = {
  args: {
    disabled: true,
    value: '5',
  },
}

export const DisabledEmpty: Story = {
  args: {
    disabled: true,
    value: null,
  },
}

export const DisabledPlaceholder: Story = {
  args: {
    disabled: true,
    value: null,
    placeholder: 'Add value',
  },
}
export const ErrorValue: Story = {
  args: {
    error: 'Error message',
    value: '5',
  },
}

export const ErrorEmpty: Story = {
  args: {
    error: 'Error message',
    value: null,
  },
}
