import {
  ComboboxHeader as MantineComboboxHeader,
  ComboboxHeaderProps as MantineComboboxHeaderProps,
} from '@mantine/core'
import { forwardRef } from 'react'

import { WillowStyleProps, useWillowStyleProps } from '../../../utils'

export interface HeaderProps
  extends WillowStyleProps,
    Omit<MantineComboboxHeaderProps, keyof WillowStyleProps> {}

/**
 * `Header` is the header container which can be used in `Combobox.Dropdown`.
 */
export const Header = forwardRef<HTMLDivElement, HeaderProps>((props, ref) => (
  <MantineComboboxHeader {...props} {...useWillowStyleProps(props)} ref={ref} />
))
