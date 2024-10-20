import { HTMLProps, ReactElement, ReactNode } from 'react'

export default function Row(
  props: HTMLProps & {
    to?: string
    selected?: boolean
    className?: string
    onClick?: (e) => void
    children?: ReactNode
  }
): ReactElement
