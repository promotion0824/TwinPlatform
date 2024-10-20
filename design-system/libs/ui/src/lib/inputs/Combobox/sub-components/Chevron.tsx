import {
  ComboboxChevron as MantineComboboxChevron,
  ComboboxChevronProps as MantineComboboxChevronProps,
} from '@mantine/core'
import { forwardRef } from 'react'

import { WillowStyleProps, useWillowStyleProps } from '../../../utils'

export interface ChevronProps
  extends WillowStyleProps,
    Omit<MantineComboboxChevronProps, keyof WillowStyleProps> {}

/**
 * `Combobox.Chevron` is the svg component can be used as suffix of `Combobox.InputBase`.
 */
export const Chevron = forwardRef<SVGSVGElement, ChevronProps>((Props, ref) => (
  <MantineComboboxChevron
    {...Props}
    {...useWillowStyleProps(Props)}
    ref={ref}
    size="xs"
  />
))
