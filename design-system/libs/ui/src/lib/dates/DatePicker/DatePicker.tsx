import {
  DatePickerType,
  DatePicker as MantineDatePicker,
  DatePickerProps as MantineDatePickerProps,
} from '@mantine/dates'
import { forwardRef } from 'react'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

export interface DatePickerProps<Type extends DatePickerType = 'default'>
  extends WillowStyleProps,
    Omit<MantineDatePickerProps<Type>, keyof WillowStyleProps>,
    BaseProps<Type> {}

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export interface BaseProps<Type extends DatePickerType = 'default'> {
  type?: MantineDatePickerProps<Type>['type']
  /** Value for controlled component */
  value?: MantineDatePickerProps<Type>['value']
  /** Called when value changes */
  onChange?: MantineDatePickerProps<Type>['onChange']
  defaultValue?: MantineDatePickerProps<Type>['defaultValue']

  /** Maximum possible date */
  maxDate?: MantineDatePickerProps<Type>['maxDate']
  /** Minimum possible date */
  minDate?: MantineDatePickerProps<Type>['minDate']
  /** Initial date that is displayed, used for uncontrolled component */
  defaultDate?: MantineDatePickerProps<Type>['defaultDate']

  allowSingleDateInRange?: MantineDatePickerProps<Type>['allowSingleDateInRange']
  ref?: React.ForwardedRef<HTMLDivElement>
}

interface DatePickerComponent
  extends React.ForwardRefExoticComponent<DatePickerProps<DatePickerType>> {
  <Type extends DatePickerType>(props: DatePickerProps<Type>): JSX.Element
}

/**
 * `DatePicker` is a base component for DateTimeInput.
 */
export const DatePicker = forwardRef<
  HTMLDivElement,
  DatePickerProps<DatePickerType>
>((props, ref) => (
  <MantineDatePicker ref={ref} {...props} {...useWillowStyleProps(props)} />
)) as DatePickerComponent

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export const BasePropsDiv = forwardRef<HTMLDivElement, BaseProps>(() => <div />)
