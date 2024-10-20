import { PropsWithChildren, ReactElement, CSSProperties } from 'react'

export default function DropdownContent(
  props: PropsWithChildren<{
    position?: 'top' | 'right' | 'bottom' | 'left'
    useMinWidth?: boolean
    contentClassName?: string
    contentStyle?: CSSProperties
    className?: string
  }>
): ReactElement
