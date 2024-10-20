import {
  ComboboxOptions as MantineComboboxOptions,
  ComboboxOptionsProps as MantineOptionsProps,
} from '@mantine/core'
import { forwardRef } from 'react'
import { WillowStyleProps, useWillowStyleProps } from '../../../utils'

export interface OptionsProps
  extends WillowStyleProps,
    Omit<MantineOptionsProps, keyof WillowStyleProps> {
  /** Id of the element that should label the options list */
  labelledBy?: string
}

/**
 * `Combobox.Options` is the wrapper component for all `Combobox.Option`.
 */
export const Options = forwardRef<HTMLDivElement, OptionsProps>(
  (props, ref) => (
    <MantineComboboxOptions
      {...props}
      {...useWillowStyleProps(props)}
      ref={ref}
    />
  )
)
