import {
  HTMLProps,
  PropsWithChildren,
  ReactElement,
  ReactNode,
  Ref,
} from 'react'

export default function useButton(
  props: Omit<HTMLProps<HTMLButtonElement>, 'size'> &
    PropsWithChildren<{
      type?: string
      to?: string
      href?: string
      icon?: string
      iconSize?: string
      color?: string
      size?: string
      width?: string
      disabled?: boolean
      loading?: boolean
      success?: boolean
      error?: boolean
      selected?: boolean
      readOnly?: boolean
      preventDefault?: boolean
      ripple?: boolean
      tabIndex?: string
      className?: string
      iconClassName?: string
      children: ReactNode
      onClick?: () => void
      onPointerDown?: () => void
      onPointerUp?: () => void
      onPointerLeave?: () => void
      ref?: Ref
    }>
): ReactElement
