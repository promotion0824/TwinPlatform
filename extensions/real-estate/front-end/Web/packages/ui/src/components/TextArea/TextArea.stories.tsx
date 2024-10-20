import type { Meta, StoryObj } from '@storybook/react'
import React from 'react'

import TextArea from './TextArea'

const meta: Meta<typeof TextArea> = {
  component: TextArea,
  decorators: [
    (Story) => (
      <div style={{ display: 'flex', flexDirection: 'column' }}>
        <Story />
      </div>
    ),
  ],
}

export default meta
type Story = StoryObj<typeof TextArea>

// default

export const Empty: Story = {
  args: {
    label: 'TextArea',
  },
}

export const Placeholder: Story = {
  args: {
    label: 'TextArea with placeholder',
    placeholder: 'TextArea placeholder',
  },
}

export const Value: Story = {
  args: {
    label: 'TextArea with value',
    value: 'TextArea value',
  },
}

// readonly

export const ReadonlyEmpty: Story = {
  args: {
    label: 'Readonly TextArea',
    readOnly: true,
  },
}

export const ReadonlyPlaceholder: Story = {
  args: {
    label: 'Readonly TextArea with placeholder',
    readOnly: true,
    placeholder: 'TextArea placeholder',
  },
}

export const ReadonlyValue: Story = {
  args: {
    label: 'Readonly TextArea with value',
    readOnly: true,
    value: 'TextArea value',
  },
}

// disabled states

export const DisabledEmpty: Story = {
  args: {
    label: 'Disabled TextArea',
    disabled: true,
  },
}

export const DisabledPlaceholder: Story = {
  args: {
    label: 'Disabled TextArea with placeholder',
    disabled: true,
    placeholder: 'TextArea placeholder',
  },
}

export const DisabledValue: Story = {
  args: {
    label: 'Disabled TextArea with value',
    disabled: true,
    value: 'TextArea value',
  },
}

// error states

export const ErrorEmpty: Story = {
  args: {
    error: 'Errored TextArea',
  },
}

export const ErrorPlaceholder: Story = {
  args: {
    error: 'Errored TextArea with placeholder',
    placeholder: 'TextArea placeholder',
  },
}

export const ErrorValue: Story = {
  args: {
    error: 'Errored TextArea with value',
    value: 'TextArea value',
  },
}
