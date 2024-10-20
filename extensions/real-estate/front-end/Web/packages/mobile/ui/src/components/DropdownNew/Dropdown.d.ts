import { ComponentProps, PropsWithChildren, Ref } from 'react'
import Button from '../Button/Button'

export default function Dropdown(
  props: ComponentProps<typeof Button> &
    PropsWithChildren<{
      'data-segment'?: string
      disabled?: boolean
      readOnly?: boolean
      isOpen?: boolean
      value?: string
      dropdownIcon?: string
      dropdownIconSize?: string
      dropdownRef?: Ref
      showIcon?: boolean
      className?: string
      dropdownIconClassName?: string
      onChange?: () => void
      ref: Ref
    }>
)
