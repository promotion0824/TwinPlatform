import {
  ComboboxFooter as MantineComboboxFooter,
  ComboboxFooterProps as MantineComboboxFooterProps,
} from '@mantine/core'
import { forwardRef } from 'react'

import { WillowStyleProps, useWillowStyleProps } from '../../../utils'

export interface FooterProps
  extends WillowStyleProps,
    Omit<MantineComboboxFooterProps, keyof WillowStyleProps> {}

/**
 * `Combobox.Footer` is a styled container can be used in `Combobox.Dropdown`.
 */
export const Footer = forwardRef<HTMLDivElement, FooterProps>((props, ref) => (
  <MantineComboboxFooter {...props} {...useWillowStyleProps(props)} ref={ref} />
))
