import type { Meta, StoryObj } from '@storybook/react'

import dayjs from 'dayjs'
import { useState } from 'react'

import { DateInput } from '.'
import { Icon } from '../../misc/Icon'
import { storybookAutoSourceParameters } from '../../utils/constant'

const meta: Meta<typeof DateInput> = {
  title: 'DateInput',
  component: DateInput,
  args: {
    // this date will impact visual tests
    defaultValue: new Date('2023-10-05'),
  },
}
export default meta

type Story = StoryObj<typeof DateInput>

export const Playground: Story = {
  ...storybookAutoSourceParameters,
  args: { placeholder: 'Date input placeholder', defaultValue: undefined },
}

export const DefaultValue: Story = {
  ...storybookAutoSourceParameters,
  args: {},
}

export const DefaultDate: Story = {
  ...storybookAutoSourceParameters,
  args: {},
}

export const ControlledValue: Story = {
  render: () => {
    const [value, setValue] = useState<Date | null>(new Date())

    return <DateInput value={value} onChange={setValue} />
  },
}

export const PrefixAndSuffix: Story = {
  ...storybookAutoSourceParameters,
  args: {
    prefix: <Icon icon="info" />,
    suffix: <Icon icon="info" />,
  },
}

export const RequiredWithLabelAndError: Story = {
  render: () => {
    const [error, setError] = useState<string | boolean>(
      'This field is required'
    )
    const [value, setValue] = useState<Date | null>(null)

    return (
      <DateInput
        label="Date input label"
        required
        error={error}
        value={value}
        onChange={(value) => {
          if (value) {
            setError(false)
          }
          setValue(value)
        }}
      />
    )
  },
}

export const HorizontalLayout: Story = {
  ...storybookAutoSourceParameters,
  args: {
    layout: 'horizontal',
    label: 'Date input label',
    description: 'Date input description',
  },
}

export const HorizontalLayoutWithLabelWidth: Story = {
  ...storybookAutoSourceParameters,
  args: {
    layout: 'horizontal',
    label: 'Date input label',
    labelWidth: 300,
    description: 'Date input description',
  },
}

export const Readonly: Story = {
  ...storybookAutoSourceParameters,
  args: {
    readOnly: true,
  },
}

export const Disabled: Story = {
  ...storybookAutoSourceParameters,
  args: {
    disabled: true,
  },
}

export const Description: Story = {
  ...storybookAutoSourceParameters,
  args: {
    description: 'Date input description',
  },
}

export const ValueFormat: Story = {
  render: () => <DateInput placeholder="YYYY-MM-DD" valueFormat="YYYY-MM-DD" />,
}

/**
 * Use `dateParser` prop to replace default date parser. Parser function
 * accepts user input (string) and must return Date object
 */
export const DateParser: Story = {
  render: () => (
    <DateInput
      placeholder="Type Christmas or a date"
      dateParser={(input: string) => {
        if (input.toLowerCase() === 'christmas') {
          return new Date('2023-12-25')
        }
        return new Date(input)
      }}
    />
  ),
}

/**
 * Set `clearable` prop to allow removing value from the input. Input will
 * be cleared if user selects the same date in dropdown or clears input value.
 */
export const Clearable: Story = {
  ...storybookAutoSourceParameters,
  args: {
    clearable: true,
  },
}

export const DisabledDates: Story = {
  ...storybookAutoSourceParameters,
  args: {
    // disable Saturdays and Sundays
    excludeDate: (date) => date.getDay() > 5 || date.getDay() === 0,
  },
}

/**
 * Set `minDate` and `maxDate` props to define min and max dates. If date that is
 * after `maxDate` or before `minDate` is entered, then it will be considered invalid
 * and input value will be reverted to last known valid date value.
 */
export const MinMaxDate: Story = {
  render: () => (
    <DateInput
      minDate={new Date()}
      maxDate={dayjs(new Date()).add(1, 'month').toDate()}
    />
  ),
}
