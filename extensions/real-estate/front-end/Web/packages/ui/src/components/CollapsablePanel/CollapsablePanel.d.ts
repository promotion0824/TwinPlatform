import React, { ReactNode, HTMLProps, ReactElement } from 'react'

export default function CollapsablePanel(
  props: HTMLProps<HTMLElement> & {
    header?: string
    position?: 'left' | 'right'
    icon?: string
    children: ReactNode
    border?: string
    $borderWidth?: string
    width?: string
    noScroll?: boolean
    isOpen?: boolean
    className?: string
    onPanelStateChange?: (isOpen: boolean) => void
  }
): ReactElement
