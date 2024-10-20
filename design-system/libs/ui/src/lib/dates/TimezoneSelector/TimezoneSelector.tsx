import { isArray, isFunction } from 'lodash'
import { forwardRef, useMemo } from 'react'

import { Select, SelectItem, SelectProps } from '../../inputs/Select'
import { BaseProps as SelectBaseProps } from '../../inputs/Select/Select'
import { generateTimezones, updateTargetLabel } from './utils'

export interface TimezoneSelectorProps
  extends Omit<SelectProps, 'data' | 'onChange'>,
    BaseProps {}

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export interface BaseProps
  extends Pick<
    SelectBaseProps,
    | 'onChange'
    | 'value'
    | 'defaultValue'
    | 'error'
    | 'errorProps'
    | 'label'
    | 'labelProps'
    | 'description'
    | 'descriptionProps'
    | 'placeholder'
  > {
  /**
   * Default to be a full list of timezones.
   * Can accept an array of timezone options as SelectProps['data'],
   * or a filter function that will be invoked with the default options.
   */
  data?: SelectProps['data'] | ((param: SelectItem[]) => SelectItem[])
}

/**
 * `TimezoneSelector` is a wrapper around `Select` that provides a list of timezones.
 */
export const TimezoneSelector = forwardRef<
  HTMLInputElement,
  TimezoneSelectorProps
>(({ data, label = 'Timezone', ...restProps }, ref) => {
  // not required to be configurable at the moment
  const indicateBrowserTimezone = true

  const timezoneOptions = useMemo(() => {
    const options = isArray(data)
      ? data
      : isFunction(data)
      ? data(generateTimezones())
      : generateTimezones()

    if (indicateBrowserTimezone) {
      return updateTargetLabel(options)
    }

    return options
  }, [data, indicateBrowserTimezone])

  return (
    <Select
      label={label}
      searchable
      data={timezoneOptions}
      ref={ref}
      {...restProps}
    />
  )
})

/**
 * Fix for Storybook ArgTypes not working with Mantine's props.
 * See https://willow.atlassian.net/l/cp/40rrHNJp
 */
export const BasePropsDiv = forwardRef<HTMLDivElement, BaseProps>(() => <div />)
