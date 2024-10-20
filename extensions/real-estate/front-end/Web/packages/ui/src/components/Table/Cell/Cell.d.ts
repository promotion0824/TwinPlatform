import { HTMLProps, ReactElement, ReactNode } from 'react'

export default function Cell(
  props: HTMLProps & {
    type?: 'td' | 'fill' | 'none'
    align?: 'left' | 'center' | 'right' | 'top' | 'middle' | 'bottom'
    className?: string
    children?: ReactNode
  }
): ReactElement
