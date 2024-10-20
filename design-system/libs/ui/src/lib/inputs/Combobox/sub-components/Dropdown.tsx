import {
  ComboboxDropdown as MantineComboboxDropdown,
  ComboboxDropdownProps as MantineComboboxDropdownProps,
} from '@mantine/core'
import { forwardRef } from 'react'

import { WillowStyleProps, useWillowStyleProps } from '../../../utils'

export interface DropdownProps
  extends WillowStyleProps,
    Omit<MantineComboboxDropdownProps, keyof WillowStyleProps> {}

/**
 * `Combobox.Dropdown` is a dropdown container which can
 * be used in `Combobox` component.
 */
export const Dropdown = forwardRef<HTMLDivElement, DropdownProps>(
  (props, ref) => (
    <MantineComboboxDropdown
      {...props}
      {...useWillowStyleProps(props)}
      ref={ref}
    />
  )
)
