import {
  InputBase as MantineInputBase,
  InputBaseProps as MantineInputBaseProps,
  createPolymorphicComponent,
} from '@mantine/core'
import { forwardRef } from 'react'

import {
  WillowStyleProps,
  getCommonInputProps,
  useWillowStyleProps,
} from '../../../utils'
import { BaseProps } from '../../TextInput/TextInput'

export interface InputBaseProps
  extends WillowStyleProps,
    Omit<
      MantineInputBaseProps,
      | keyof WillowStyleProps
      | 'leftSection'
      | 'prefix'
      | 'rightSectionProps'
      | 'rightSection'
      | 'rightSectionPointerEvents'
    >,
    BaseProps {
  suffixPointerEvents?: MantineInputBaseProps['rightSectionPointerEvents']
}

/** `Combobox.InputBase` is a polymorphic input component. */
export const InputBase = createPolymorphicComponent<'input', InputBaseProps>(
  forwardRef<HTMLInputElement, InputBaseProps>(
    (
      {
        description,
        error,
        prefix,
        suffix,
        suffixProps,
        suffixPointerEvents,
        ...restProps
      },
      ref
    ) => {
      return (
        <MantineInputBase
          {...getCommonInputProps({ description, error })}
          {...restProps}
          {...useWillowStyleProps(restProps)}
          leftSection={prefix}
          rightSection={suffix}
          rightSectionProps={suffixProps}
          rightSectionPointerEvents={suffixPointerEvents}
          ref={ref}
          size="xs"
        />
      )
    }
  )
)
