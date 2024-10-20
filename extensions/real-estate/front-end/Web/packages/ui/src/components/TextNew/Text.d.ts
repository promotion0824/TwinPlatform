import { ReactNode, ReactElement } from 'react'

export default function Text(
  props: HTMLProps<HTMLElement> & {
    type?: 'label' | 'group'
    color?: 'dark'
    textTransform?: 'uppercase'
    whiteSpace?: 'nowrap'
    className?: string
    children?: ReactNode
  }
): ReactElement
