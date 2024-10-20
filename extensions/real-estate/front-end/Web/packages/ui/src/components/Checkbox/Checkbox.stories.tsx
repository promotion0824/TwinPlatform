import type { Meta, StoryObj } from '@storybook/react'
import React, { useState } from 'react'

import Checkbox from './Checkbox'

const meta: Meta<typeof Checkbox> = {
  component: Checkbox,
  render: ({ value, ...args }) => {
    const [val, setVal] = useState(value)
    return (
      <Checkbox {...args} value={val} onChange={setVal}>
        Checkbox children
      </Checkbox>
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
type Story = StoryObj<typeof Checkbox>

// default

export const Unchecked: Story = {
  args: {
    label: 'Checkbox unchecked',
  },
}

export const Checked: Story = {
  args: {
    label: 'Checkbox checked',
    value: true,
  },
}

// readonly

export const ReadonlyUnchecked: Story = {
  args: {
    label: 'Readonly Checkbox unchecked',
    readOnly: true,
  },
}

export const ReadonlyChecked: Story = {
  args: {
    label: 'Readonly Checkbox checked',
    readOnly: true,
    value: true,
  },
}

// disabled states

export const DisabledUnchecked: Story = {
  args: {
    label: 'Disabled Checkbox unchecked',
    disabled: true,
  },
}

export const DisabledChecked: Story = {
  args: {
    label: 'Disabled Checkbox checked',
    disabled: true,
    value: true,
  },
}

// error states

export const ErrorUnchecked: Story = {
  args: {
    error: 'Errored Checkbox unchecked',
  },
}

export const ErrorChecked: Story = {
  args: {
    error: 'Errored Checkbox checked',
    value: true,
  },
}
