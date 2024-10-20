import {
  DateInput as MantineDateInput,
  DateInputProps as MantineDateInputProps,
} from '@mantine/dates'
import { forwardRef } from 'react'
import styled, { css } from 'styled-components'

import { CloseButton } from '../../common'
import { BaseProps as TextInputBaseProps } from '../../inputs/TextInput/TextInput'
import { Icon } from '../../misc/Icon'
import {
  CommonInputProps,
  getCommonInputProps,
  getInputPaddings,
} from '../../utils'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'
import { DateValue } from '../DateTimeInput/types'

export interface DateInputProps
  extends WillowStyleProps,
    Omit<
      MantineDateInputProps,
      | keyof WillowStyleProps
      | 'prefix'
      | 'leftSection'
      | 'rightSection'
      | 'rightSectionProps'
    >,
    BaseProps {}

interface BaseProps
  extends CommonInputProps<DateValue>,
    Omit<
      TextInputBaseProps,
      'placeholder' | 'value' | 'onChange' | 'defaultValue' | 'clearable'
    > {
  /**
   * Initial date that is displayed in calendar, used for uncontrolled
   * component
   */
  defaultDate?: MantineDateInputProps['defaultDate']
  /** Date that is displayed in calendar, used for controlled component */
  date?: MantineDateInputProps['date']
  /** Called when date changes */
  onDateChange?: MantineDateInputProps['onDateChange']

  /**
   * number 0-6, 0 – Sunday, 6 – Saturday, defaults to 1 – Monday
   * @default 1
   */
  firstDayOfWeek?: MantineDateInputProps['firstDayOfWeek']
  /** Callback function to determine whether the day should be disabled */
  excludeDate?: MantineDateInputProps['excludeDate']
  /** Maximum possible date */
  maxDate?: MantineDateInputProps['maxDate']
  /** Minimum possible date */
  minDate?: MantineDateInputProps['minDate']
  /** Controls day value rendering */
  renderDay?: MantineDateInputProps['renderDay']
  /** Parses user input to convert it to Date object */
  dateParser?: MantineDateInputProps['dateParser']
  /**
   * Dayjs format to display input value
   * @default "MMMM D, YYYY"
   */
  valueFormat?: MantineDateInputProps['valueFormat']

  /** dayjs locale, defaults to value defined in DatesProvider */
  locale?: MantineDateInputProps['locale']

  /**
   * Determines whether dates outside current month should be hidden,
   * defaults to false
   * @default false
   */
  hideOutsideDates?: MantineDateInputProps['hideOutsideDates']

  /**
   * Determines whether value can be deselected when the user clicks on the
   * selected date in the calendar (only when clearable prop is set), defaults
   * to true if clearable prop is set, false otherwise
   * @default false
   */
  allowDeselect?: MantineDateInputProps['allowDeselect']
  /**
   * Determines whether input value can be cleared, adds clear button to right
   * section, false by default
   * @default false
   */
  clearable?: MantineDateInputProps['clearable']
  /** Props added to clear button */
  clearButtonProps?: MantineDateInputProps['clearButtonProps']

  /** Props added to Popover component */
  popoverProps?: MantineDateInputProps['popoverProps']
}
/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export const BasePropsDiv = forwardRef<HTMLDivElement, BaseProps>(() => <div />)

/**
 * `DateInput` is a free input component that user can type the date in different
 * format and it will be convert into a date. Or user can use the calendar in the
 * popover to select a date.
 */
export const DateInput = forwardRef<HTMLInputElement, DateInputProps>(
  (
    {
      firstDayOfWeek = 1,
      allowDeselect,
      valueFormat = 'MMMM D, YYYY',
      hideOutsideDates = false,
      clearable = false,
      clearButtonProps,
      prefix,
      suffix,
      suffixProps,
      popoverProps,
      labelWidth,
      ...restProps
    },
    ref
  ) => {
    return (
      <StyledDateInput
        nextIcon={<Icon icon="chevron_right" />}
        previousIcon={<Icon icon="chevron_left" />}
        {...restProps}
        {...useWillowStyleProps(restProps)}
        {...getCommonInputProps({ ...restProps, labelWidth })}
        firstDayOfWeek={firstDayOfWeek}
        allowDeselect={allowDeselect}
        hideOutsideDates={hideOutsideDates}
        valueFormat={valueFormat}
        clearable={clearable}
        clearButtonProps={{
          // @ts-expect-error // it works
          component: CloseButton,
          ...clearButtonProps,
        }}
        leftSection={prefix}
        rightSection={suffix}
        rightSectionProps={suffixProps}
        size="xs" /* this impacts prefix and suffix space */
        popoverProps={{
          offset: 1,
          ...popoverProps,
        }}
        ref={ref}
      />
    )
  }
)

const StyledDateInput = styled(MantineDateInput)(
  ({ leftSection, rightSection }) => css`
    .mantine-DateInput-input {
      padding: ${getInputPaddings({ leftSection, rightSection })};
    }
  `
)
