import { HTMLProps, ReactElement, ReactNode } from 'react'

export default function Message(
  props: HTMLProps & {
    icon?: string
    className?: string
    children?: ReactNode
  }
): ReactElement
