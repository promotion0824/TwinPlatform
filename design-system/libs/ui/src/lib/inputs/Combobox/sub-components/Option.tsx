import {
  ComboboxOption as MantineComboboxOption,
  ComboboxOptionProps as MantineComboboxOptionProps,
} from '@mantine/core'
import { forwardRef } from 'react'

import { WillowStyleProps, useWillowStyleProps } from '../../../utils'

export interface OptionProps
  extends WillowStyleProps,
    Omit<MantineComboboxOptionProps, keyof WillowStyleProps> {
  /** Option value */
  value: string

  /** Determines whether the option is selected */
  active?: boolean

  /** Determines whether the option can be selected */
  disabled?: boolean

  /** Determines whether item is selected, useful for virtualized comboboxes */
  selected?: boolean
}

/**
 * `Combobox.Option` is a styled option which can be used in `Combobox`.
 */
export const Option = forwardRef<HTMLDivElement, OptionProps>((props, ref) => (
  <MantineComboboxOption {...props} {...useWillowStyleProps(props)} ref={ref} />
))
