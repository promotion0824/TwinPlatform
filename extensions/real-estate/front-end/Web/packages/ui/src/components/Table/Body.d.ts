import { HTMLProps, ReactElement, ReactNode } from 'react'

export default function Body(
  props: HTMLProps & {
    className?: string
    children?: ReactNode
  }
): ReactElement
