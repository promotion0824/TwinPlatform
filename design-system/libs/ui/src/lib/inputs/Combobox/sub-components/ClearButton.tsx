import {
  ComboboxClearButton as MantineComboboxClearButton,
  ComboboxClearButtonProps as MantineComboboxClearButtonProps,
} from '@mantine/core'
import { forwardRef } from 'react'

import { WillowStyleProps, useWillowStyleProps } from '../../../utils'

export interface ClearButtonProps
  extends WillowStyleProps,
    Omit<MantineComboboxClearButtonProps, keyof WillowStyleProps | 'size'> {
  onClear: MantineComboboxClearButtonProps['onClear']
}

/**
 * `ClearButton` is a button can be used to clear the input in `Combobox`.
 */
export const ClearButton = forwardRef<HTMLButtonElement, ClearButtonProps>(
  (props, ref) => (
    <MantineComboboxClearButton
      {...props}
      {...useWillowStyleProps(props)}
      ref={ref}
      size="xs"
    />
  )
)
