import type { Meta, StoryObj } from '@storybook/react'

import NumberInput from './NumberInput'

const meta: Meta<typeof NumberInput> = {
  component: NumberInput,
  args: {
    label: 'NumberInput label',
    value: null,
    readOnly: false,
    disabled: false,
    placeholder: null,
    error: null,
  },
  decorators: [
    (Story) => (
      <div style={{ display: 'flex', flexDirection: 'column' }}>
        <Story />
      </div>
    ),
  ],
}

export default meta
type Story = StoryObj<typeof NumberInput>

// Default states

export const Empty: Story = {}

export const Placeholder: Story = {
  args: { placeholder: 'NumberInput placeholder' },
}

export const Value: Story = {
  args: { value: 'NumberInput value' },
}

export const ReadonlyEmpty: Story = {
  args: {
    readOnly: true,
  },
}

export const ReadonlyValue: Story = {
  args: {
    readOnly: true,
    value: 'NumberInput value',
  },
}

export const ReadonlyPlaceholder: Story = {
  args: {
    readOnly: true,
    placeholder: 'NumberInput placeholder',
  },
}

// Disable states

export const DisabledEmpty: Story = {
  args: {
    disabled: true,
  },
}

export const DisabledPlaceholder: Story = {
  args: {
    disabled: true,
    placeholder: 'NumberInput placeholder',
  },
}

export const DisabledValue: Story = {
  args: {
    disabled: true,
    value: 'NumberInput value',
  },
}

// Error states

export const ErrorEmpty: Story = {
  args: { error: 'NumberInput error message' },
}

export const ErrorPlaceholder: Story = {
  args: {
    error: 'Errored NumberInput with placeholder',
    placeholder: 'NumberInput placeholder',
  },
}

export const ErrorValue: Story = {
  args: {
    error: 'Errored NumberInput with value',
    value: 'NumberInput value',
  },
}
