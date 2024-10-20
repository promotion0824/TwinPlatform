import type { Meta, StoryObj } from '@storybook/react'
import React, { useState } from 'react'
import ToggleSwitch from './ToggleSwitch'

const meta: Meta<typeof ToggleSwitch> = {
  component: ToggleSwitch,
  render: () => {
    const [value, setValue] = useState(false)
    return <ToggleSwitch checked={value} onChange={setValue} />
  },
}

export default meta
type Story = StoryObj<typeof ToggleSwitch>

export const Basic: Story = {}
