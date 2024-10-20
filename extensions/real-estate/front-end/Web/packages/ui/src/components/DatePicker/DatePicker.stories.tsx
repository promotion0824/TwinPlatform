import type { Meta, StoryObj } from '@storybook/react'
import React, { useState } from 'react'

import { FeatureFlagContext } from '../../providers/FeatureFlagProvider/FeatureFlagContext'
import { LanguageContext } from '../../providers/LanguageProvider/LanguageContext'
import DatePicker from './DatePicker'

const today = new Date()

const meta: Meta<typeof DatePicker> = {
  component: DatePicker,
  render: ({ value, ...args }) => {
    const [val, setVal] = useState(value)
    const [selectedQuickRange, setSelectedQuickRange] = useState()

    return (
      <FeatureFlagContext.Provider value={{ hasFeatureToggle: () => true }}>
        <LanguageContext.Provider value={{}}>
          <DatePicker
            value={val}
            onChange={setVal}
            selectedQuickRange={selectedQuickRange}
            onSelectQuickRange={setSelectedQuickRange}
            {...args}
          />
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

// Default states

export const Empty: Story = {
  args: {
    label: 'DatePicker',
  },
}

export const Placeholder: Story = {
  args: {
    label: 'DatePicker with placeholder',
    placeholder: 'DatePicker placeholder',
  },
}

export const Value: Story = {
  args: {
    label: 'DatePicker with value',
    value: today,
  },
}

// readonly

export const ReadonlyEmpty: Story = {
  args: {
    label: 'Readonly DatePicker',
    readOnly: true,
  },
}

export const ReadonlyPlaceholder: Story = {
  args: {
    label: 'Readonly DatePicker with placeholder',
    readOnly: true,
    placeholder: 'DatePicker placeholder',
  },
}

export const ReadonlyValue: Story = {
  args: {
    label: 'Readonly DatePicker with value',
    readOnly: true,
    value: today,
  },
}

// disabled states

export const DisabledEmpty: Story = {
  args: {
    label: 'Disabled DatePicker',
    disabled: true,
  },
}

export const DisabledPlaceholder: Story = {
  args: {
    label: 'Disabled DatePicker with placeholder',
    disabled: true,
    placeholder: 'DatePicker placeholder',
  },
}

export const DisabledValue: Story = {
  args: {
    label: 'Disabled DatePicker with value',
    disabled: true,
    value: today,
  },
}

// error states

export const ErrorEmpty: Story = {
  args: {
    error: 'Errored DatePicker',
  },
}

export const ErrorPlaceholder: Story = {
  args: {
    error: 'Errored DatePicker with placeholder',
    placeholder: 'DatePicker placeholder',
  },
}

export const ErrorValue: Story = {
  args: {
    error: 'Errored DatePicker with value',
    value: today,
  },
}
