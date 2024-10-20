import { ReactNode, HTMLProps, ReactElement } from 'react'

export default function Pill(
  props: HTMLProps<HTMLElement> & {
    children?: ReactNode
    color?: string
  }
): ReactElement
