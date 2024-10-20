import { ReactNode, ReactElement, Ref, PropsWithChildren } from 'react'
import { LinkProps } from 'react-router-dom'

export default function Link(
  props: PropsWithChildren<
    { ref?: Ref<HTMLAnchorElement> } & (HTMLElement<'a'> | LinkProps)
  >
): ReactElement
