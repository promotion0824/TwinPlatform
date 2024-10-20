import {
  InputPlaceholder as MantineInputPlaceholder,
  InputPlaceholderProps as MantineInputPlaceholderProps,
} from '@mantine/core'
import { forwardRef } from 'react'

import { WillowStyleProps, useWillowStyleProps } from '../../../utils'

export interface InputPlaceholderProps
  extends WillowStyleProps,
    Omit<MantineInputPlaceholderProps, keyof WillowStyleProps> {}

/**
 * `Combobox.InputPlaceholder` could be used to style placeholder text inside
 * `Combobox` component when placeholder props is not an option.
 */
export const InputPlaceholder = forwardRef<
  HTMLDivElement,
  InputPlaceholderProps
>((props, ref) => (
  <MantineInputPlaceholder
    {...props}
    {...useWillowStyleProps(props)}
    ref={ref}
  />
))
