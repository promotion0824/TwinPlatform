import React, { useState } from 'react'
import { OnClickOutsideIdsProvider } from '@willow/ui'
import type { Meta, StoryObj } from '@storybook/react'
import TimePicker from './TimePicker'

const meta: Meta<typeof TimePicker> = {
  component: TimePicker,
  render: ({ value, ...props }) => {
    const [val, setVal] = useState(value)
    return (
      <OnClickOutsideIdsProvider>
        <TimePicker {...props} value={val} onChange={setVal} />
      </OnClickOutsideIdsProvider>
    )
  },
}

export default meta
type Story = StoryObj<typeof TimePicker>

export const Basic: Story = {
  args: {
    value: '12:34:00Z',
    copy: { stuff: 'stuff' },
    disabled: false,
  },
}

export const Disabled: Story = {
  args: {
    value: '12:34:00Z',
    copy: { stuff: 'stuff' },
    disabled: true,
  },
}
