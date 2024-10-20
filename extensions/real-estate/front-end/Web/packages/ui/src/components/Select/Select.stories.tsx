import type { Meta, StoryObj } from '@storybook/react'
import React, { useState } from 'react'

import { OnClickOutsideIdsProvider } from '../../providers/OnClickOutsideIdsProvider/OnClickOutsideIdsProvider'
import Select, { Option } from './Select'

const meta: Meta<typeof Select> = {
  component: Select,
  render: ({ value, ...args }) => {
    const options = [
      'Australia',
      'European Union',
      'Canada',
      'Hong Kong',
      'United Kingdom',
    ]
    const [val, setVal] = useState(value)
    return (
      <OnClickOutsideIdsProvider>
        <Select {...args} value={val} onChange={setVal}>
          {options.map((option) => (
            <Option key={option} value={option}>
              {option}
            </Option>
          ))}
        </Select>
      </OnClickOutsideIdsProvider>
    )
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
type Story = StoryObj<typeof Select>

// Default states

export const Empty: Story = {
  args: {
    label: 'Select',
  },
}

export const Placeholder: Story = {
  args: {
    label: 'Select with placeholder',
    placeholder: 'Select placeholder',
  },
}

export const Value: Story = {
  args: {
    label: 'Select with value',
    value: 'Australia',
  },
}

// Readonly states

export const ReadonlyEmpty: Story = {
  args: {
    label: 'Readonly Select',
    readOnly: true,
  },
}

export const ReadonlyPlaceholder: Story = {
  args: {
    label: 'Readonly Select with placeholder',
    readOnly: true,
    placeholder: 'Select placeholder',
  },
}

export const ReadonlyValue: Story = {
  args: {
    label: 'Readonly Select with value',
    readOnly: true,
    value: 'Australia',
  },
}

// Disabled states

export const DisabledEmpty: Story = {
  args: {
    label: 'Disabled Select',
    disabled: true,
  },
}

export const DisabledPlaceholder: Story = {
  args: {
    label: 'Disabled Select with placeholder',
    disabled: true,
    placeholder: 'Select placeholder',
  },
}

export const DisabledValue: Story = {
  args: {
    label: 'Disabled Select with value',
    disabled: true,
    value: 'Australia',
  },
}

// Error states

export const ErrorEmpty: Story = {
  args: {
    error: 'Errored select',
  },
}

export const ErrorPlaceholder: Story = {
  args: {
    error: 'Errored Select with placeholder',
    placeholder: 'Select placeholder',
  },
}

export const ErrorValue: Story = {
  args: {
    error: 'Errored Select with value',
    value: 'Australia',
  },
}
