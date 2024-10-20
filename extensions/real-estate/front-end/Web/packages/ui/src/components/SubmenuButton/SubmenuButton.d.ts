import React, { ReactElement, HTMLProps } from 'react'

export default function SubmenuButton(
  props: HTMLProps<HTMLElement> & {
    children?: string
    to?: string
    exclude?: string[]
    selected?: boolean
    className?: string
    isPurpleBackground?: boolean
    isGrayBackground?: boolean
  }
): ReactElement
