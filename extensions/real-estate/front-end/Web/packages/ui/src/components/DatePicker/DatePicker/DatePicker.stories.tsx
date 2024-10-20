import React, { useState } from 'react'
import type { Meta, StoryObj } from '@storybook/react'
import { FeatureFlagContext } from '../../../providers/FeatureFlagProvider/FeatureFlagContext'
import { LanguageContext } from '../../../providers/LanguageProvider/LanguageContext'
import { OnClickOutsideIdsProvider } from '../../../providers/OnClickOutsideIdsProvider/OnClickOutsideIdsProvider'
import DatePicker from './DatePicker'

const meta: Meta<typeof DatePicker> = {
  component: DatePicker,
  render: ({ value, ...args }) => {
    const [val, setVal] = useState(value)
    const [selectedQuickRange, setSelectedQuickRange] = useState()

    return (
      <FeatureFlagContext.Provider value={{ hasFeatureToggle: () => true }}>
        <LanguageContext.Provider value={{}}>
          <OnClickOutsideIdsProvider>
            <DatePicker
              value={val}
              onChange={setVal}
              selectedQuickRange={selectedQuickRange}
              onSelectQuickRange={setSelectedQuickRange}
              {...args}
            />
          </OnClickOutsideIdsProvider>
        </LanguageContext.Provider>
      </FeatureFlagContext.Provider>
    )
  },
}

export default meta
type Story = StoryObj<typeof DatePicker>

export const Basic: Story = {
  args: {
    type: 'date-time',
    value: null,
  },
}

export const DateTime: Story = {
  args: {
    type: 'date-time',
    value: null,
  },
}
export const DateRange: Story = {
  args: {
    type: 'date-range',
    value: [],
  },
}
export const DateTimeRange: Story = {
  args: {
    type: 'date-time-range',
    value: [],
  },
}
export const DatePickerWithQuickRangeOptions: Story = {
  args: {
    type: 'date-range',
    value: {},
    quickRangeOptions: ['7D', '1M', '3M', '6M'],
  },
}
