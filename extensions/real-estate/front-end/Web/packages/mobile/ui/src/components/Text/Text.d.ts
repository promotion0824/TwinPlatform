import { HTMLProps, PropsWithChildren } from 'react'

export default function Text(
  props: HTMLProps<HTMLSpanElement> &
    PropsWithChildren<{
      type?: 'h1' | 'h3' | 'h4' | 'label' | 'value' | 'dark'
      color?: string
      className?: string
      whiteSpace?: 'nowrap' | 'normal'
    }>
)
