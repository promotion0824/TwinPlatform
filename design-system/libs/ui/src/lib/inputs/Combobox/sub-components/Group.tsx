import {
  ComboboxGroup as MantineComboboxGroup,
  ComboboxGroupProps as MantineComboboxGroupProps,
} from '@mantine/core'
import { forwardRef } from 'react'

import { WillowStyleProps, useWillowStyleProps } from '../../../utils'

export interface GroupProps
  extends WillowStyleProps,
    Omit<MantineComboboxGroupProps, keyof WillowStyleProps> {
  /** Label of the group */
  label?: MantineComboboxGroupProps['label']
}

/**
 * `Combobox.Group` is the wrapper component to group `Combobox.option`s.
 */
export const Group = forwardRef<HTMLDivElement, GroupProps>((props, ref) => (
  <MantineComboboxGroup {...props} {...useWillowStyleProps(props)} ref={ref} />
))
