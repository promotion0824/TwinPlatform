import React, { Ref, ReactElement, PropsWithChildren } from 'react'

export default function ButtonContent(
  props: PropsWithChildren<{
    ref?: Ref<HTMLElement>
    type?: string
    color?: string
    to?: string
    href?: string
    icon?: string
    iconSize?: string
    selected?: string
    loading?: string
    successful?: string
    error?: string
    readOnly?: boolean
    disabled?: boolean
    isLink?: boolean
    isColorButton?: boolean
    isIconButton?: boolean
    isBlocked?: boolean
    ripple?: string
    ripplesRef?: string
    height?: string
    width?: string
    className?: string
    iconClassName?: string
    onClick?: () => void
    onPointerDown?: () => void
    onPointerUp?: () => void
    onPointerLeave?: () => void
  }>
): ReactElement
