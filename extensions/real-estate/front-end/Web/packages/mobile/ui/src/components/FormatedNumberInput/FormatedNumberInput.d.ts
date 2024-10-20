import { PropsWithChildren } from 'react'
import { NumberFormatProps } from 'react-number-format'

export default function FormatedNumberInput(
  props: Omit<NumberFormatProps, 'onValueChange'> &
    PropsWithChildren<{
      error?: string
      width?: string
      icon?: string
      inputType?: string
      className?: string
      iconClassName?: string
      inputClassName?: string
      onChange?: (value: string) => void
      debounce?: () => void
      ref?: Ref
    }>
)
