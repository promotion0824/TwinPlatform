import type { Meta, StoryObj } from '@storybook/react'
import React, { useState } from 'react'
import { OnClickOutsideIdsProvider } from '../../providers'
import Duration, { InputDuration, parseIsoDuration } from './DurationInput'

const meta: Meta<typeof Duration> = {
  component: Duration,
  render: () => {
    const initialValue = 'P1DT12H'
    const [value, setValue] = useState(
      parseIsoDuration(initialValue) as InputDuration
    )

    return (
      <OnClickOutsideIdsProvider>
        <Duration value={value} onChange={setValue} />
      </OnClickOutsideIdsProvider>
    )
  },
}

export default meta
type Story = StoryObj<typeof Duration>

export const Basic: Story = {}
