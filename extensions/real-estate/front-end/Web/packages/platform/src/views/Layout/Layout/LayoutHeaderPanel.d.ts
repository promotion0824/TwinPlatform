import React, { ReactNode, HTMLProps, ReactElement } from 'react'

export default function LayoutHeaderPanel(
  props: HTMLProps<HTMLElement> & {
    children: ReactNode
    fill?: string
    overflow?: string
    align?: string
  }
): ReactElement
