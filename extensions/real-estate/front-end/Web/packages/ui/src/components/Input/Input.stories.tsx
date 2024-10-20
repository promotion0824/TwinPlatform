import type { Meta, StoryObj } from '@storybook/react'
import React from 'react'

import Input from './Input'

const meta: Meta<typeof Input> = {
  component: Input,
  args: {
    label: 'Input label',
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
type Story = StoryObj<typeof Input>

// Default states

export const Empty: Story = {
  args: {
    label: 'Input',
  },
}

export const Placeholder: Story = {
  args: {
    label: 'Input with placeholder',
    placeholder: 'Input placeholder',
  },
}

export const Value: Story = {
  args: {
    label: 'Input with value',
    value: 'Input value',
  },
}

// readonly

export const ReadonlyEmpty: Story = {
  args: {
    label: 'Readonly Input',
    readOnly: true,
  },
}

export const ReadonlyPlaceholder: Story = {
  args: {
    label: 'Readonly Input with placeholder',
    readOnly: true,
    placeholder: 'Input placeholder',
  },
}

export const ReadonlyValue: Story = {
  args: {
    label: 'Readonly Input with value',
    readOnly: true,
    value: 'Input value',
  },
}

// disabled states

export const DisabledEmpty: Story = {
  args: {
    label: 'Disabled Input',
    disabled: true,
  },
}

export const DisabledPlaceholder: Story = {
  args: {
    label: 'Disabled Input with placeholder',
    disabled: true,
    placeholder: 'Input placeholder',
  },
}

export const DisabledValue: Story = {
  args: {
    label: 'Disabled Input with value',
    disabled: true,
    value: 'Input value',
  },
}

// error states

export const ErrorEmpty: Story = {
  args: {
    error: 'Errored Input',
  },
}

export const ErrorPlaceholder: Story = {
  args: {
    error: 'Errored Input with placeholder',
    placeholder: 'Input placeholder',
  },
}

export const ErrorValue: Story = {
  args: {
    error: 'Errored Input with value',
    value: 'Input value',
  },
}
