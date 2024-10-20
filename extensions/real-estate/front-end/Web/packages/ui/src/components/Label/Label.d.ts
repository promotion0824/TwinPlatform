import { ReactNode, HTMLProps, ReactElement } from 'react'

export default function Label(
  props: HTMLProps<HTMLElement> & {
    id?: string
    label?: string
    error?: any
    readOnly?: boolean
    disabled?: boolean
    value?: any
    required?: boolean
    hasFocus?: boolean
    className?: string
    labelClassName?: string
    children?: ReactNode
    hiddenLabel?: boolean
  }
): ReactElement
