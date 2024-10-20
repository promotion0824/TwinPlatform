import React, {
  Ref,
  CSSProp,
  HTMLProps,
  ReactElement,
  PropsWithChildren,
  ComponentProps,
  MouseEventHandler,
} from 'react'
import Button from './Button/Button'

export type ButtonProps = ComponentProps<typeof Button> & {
  ref?: Ref<HTMLElement>
  css?: CSSProp
  isLink?: boolean
  color?: string
  href?: string
  icon?: string
  iconSize?: string
}

export default function Button(
  props: HTMLProps<HTMLElement> & PropsWithChildren<ButtonProps>
): ReactElement
