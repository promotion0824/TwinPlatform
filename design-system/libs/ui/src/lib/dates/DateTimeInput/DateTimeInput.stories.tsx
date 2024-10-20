import type { Meta, StoryObj } from '@storybook/react'
import { useState } from 'react'

import { filter } from 'lodash'
import { DateTimeInput, useDateTimeInputExtension } from '.'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof DateTimeInput> = {
  title: 'DateTimeInput',
  component: DateTimeInput,
}

export default meta

type Story = StoryObj<typeof DateTimeInput>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: {
    label: 'Date picker',
    description: 'Please select a date to start search',
  },
}

export const DateType: Story = {
  storyName: 'Date' /** name Date is used by date constructor */,
  render: (args) => <DateTimeInput {...args} />,
}

export const DateRange: Story = {
  render: (args) => <DateTimeInput type="date-range" {...args} />,
}

export const DateTime: Story = {
  render: (args) => <DateTimeInput type="date-time" {...args} />,
}

export const DateTimeRange: Story = {
  render: (args) => <DateTimeInput type="date-time-range" {...args} />,
}

export const HorizontalLayout: Story = {
  ...storybookAutoSourceParameters,
  args: {
    layout: 'horizontal',
    label: 'Date picker',
    description: 'Please select a date to start search',
  },
}

export const HorizontalLayoutWithLabelWidth: Story = {
  ...storybookAutoSourceParameters,
  args: {
    layout: 'horizontal',
    label: 'Date picker',
    labelWidth: 300,
    description: 'Please select a date to start search',
  },
}

export const ControlledDateTimeRangeInput: Story = {
  render: (args) => {
    const [dates, setDates] = useState<[Date, Date] | Date>([
      new Date(1706648400000),
      new Date(1709205181000),
    ])

    const [timezone, setTimezone] = useState<string | null>('America/Toronto')
    return (
      <DateTimeInput
        {...args}
        type="date-time-range"
        value={dates}
        onChange={setDates}
        timezone={timezone}
        onTimezoneChange={setTimezone}
      />
    )
  },
}

export const CustomizeFirstDayOfWeek: Story = {
  ...storybookAutoSourceParameters,
  args: {
    datePickerProps: {
      firstDayOfWeek: 1,
    },
  },
}

export const CustomizedQuickActionsUseOptionShortcutsFilter: Story = {
  render: (args) => (
    <DateTimeInput
      {...args}
      quickActionProps={{
        optionShortcuts: (optionShortcuts) =>
          filter(optionShortcuts, (option) => option.unit === 'day'),
      }}
    />
  ),
}

export const CustomizedQuickActionsUseOptionShortcuts: Story = {
  render: (args) => (
    <DateTimeInput
      {...args}
      quickActionProps={{
        optionShortcuts: [
          {
            unit: 'day' as const,
            values: [1, 2, 3, 4, 5],
            suffix: 'ago' as const,
          },
        ],
      }}
    />
  ),
}

export const CustomizedQuickActionsUseOptionsFilter: Story = {
  render: (args) => (
    <DateTimeInput
      {...args}
      quickActionProps={{
        options: (optionGroups) =>
          optionGroups.map((group) =>
            filter(group, (option) => option.label.includes('day'))
          ),
      }}
    />
  ),
}

export const CustomizedQuickActionsUseOptions: Story = {
  render: (args) => {
    function daysAgo(days: number) {
      const now = new Date()
      now.setDate(now.getDate() - days)
      return now
    }

    function weeksAgo(weeks: number) {
      const now = new Date()
      now.setDate(now.getDate() - weeks * 7)
      return now
    }

    return (
      <DateTimeInput
        {...args}
        quickActionProps={{
          options: [
            [
              {
                label: '1 day ago',
                getValue: () => daysAgo(1),
              },
              {
                label: '2 days ago',
                getValue: () => daysAgo(2),
              },
              {
                label: '3 days ago',
                getValue: () => daysAgo(3),
              },
            ],

            [
              { label: '1 week ago', getValue: () => weeksAgo(1) },
              { label: '2 weeks ago', getValue: () => weeksAgo(2) },
            ],
          ],
        }}
      />
    )
  },
}

/**
 * `useDateTimeInputExtension` contains two predefined selectors that you can opt in to.
 * `daysFilter` and `hoursFilter` are the two values to use after the user applies the selection.
 * And it won't change the original `value` produced by `DateTimeInput`.
 * If you wish to apply those filter values to the original `value`, you need to do it manually.
 * Feel free to reach out if you want a helper function to do that.
 *
 * Please note, it doesn't make sense to use those selectors with `Date` and
 * `DateTime` type.
 *
 */
export const WithDaysAndHoursExtension: Story = {
  render: (args) => {
    const { daysFilter, hoursFilter, ...customProps } =
      useDateTimeInputExtension()

    return (
      <>
        <DateTimeInput type="date-range" {...args} {...customProps} />
        <br />
        The result filter values are{' '}
        <code>{JSON.stringify({ daysFilter, hoursFilter })}</code>
      </>
    )
  },
}

/**
 * You could also disable one of the extensions, and config the initial value as well
 * as the select options.
 */
export const CustomizedDaysExtension: Story = {
  render: (args) => {
    const { daysFilter, ...customProps } = useDateTimeInputExtension({
      initialDaysFilter: null,
      daysFilterLabel: 'Day range',
      daysData: [
        { label: 'Customized all days', value: 'allDays' },
        { label: 'Customized weekdays', value: 'weekDays' },
        { label: 'Customized weekends', value: 'weekEnds' },
      ],
      hideHoursFilter: true,
    })

    return (
      <>
        <DateTimeInput type="date-range" {...args} {...customProps} />
        <br />
        The result filter value is <code>{JSON.stringify({ daysFilter })}</code>
      </>
    )
  },
}

/**
 * In current WillowApp, we retrieve the list of timezones using a backend API.
 * Therefore, you simply need to pass that list of timezones as `data` to the `timezoneSelectorProps`
 * like below example. And also pass your `buildingTimezone` as the initial selected timezone to
 * `timezone` prop.
 */
export const CustomizedTimezoneList: Story = {
  render: (args) => {
    const [timezone, setTimezone] = useState<string | null>('America/Toronto')
    return (
      <DateTimeInput
        {...args}
        type="date-time-range"
        timezone={timezone}
        onTimezoneChange={setTimezone}
        timezoneSelectorProps={{
          data: [
            { label: 'America/Toronto', value: 'America/Toronto' },
            { label: 'Asia/Tokyo', value: 'Asia/Tokyo' },
            { label: 'Europe/London', value: 'Europe/London' },
          ],
        }}
      />
    )
  },
}
