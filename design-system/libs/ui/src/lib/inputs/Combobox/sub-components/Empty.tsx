import {
  ComboboxEmpty as MantineComboboxEmpty,
  ComboboxEmptyProps as MantineComboboxEmptyProps,
} from '@mantine/core'
import { forwardRef } from 'react'

import { WillowStyleProps, useWillowStyleProps } from '../../../utils'

export interface EmptyProps
  extends WillowStyleProps,
    Omit<MantineComboboxEmptyProps, keyof WillowStyleProps> {}

/**
 * `Empty` is a text container used to display message when options
 * are not available in combobox dropdown.
 */
export const Empty = forwardRef<HTMLDivElement, EmptyProps>((props, ref) => (
  <MantineComboboxEmpty {...props} {...useWillowStyleProps(props)} ref={ref} />
))
