import { ReactNode, HTMLProps, ReactElement, CSSProp } from 'react'

/**
 * @deprecated
 * Please use `Button` with prefix `Icon` from '@willowinc/ui' instead.
 */
export default function AddButton(
  props: HTMLProps<HTMLElement> & {
    children: ReactNode
    className?: string
    onClick: () => void

    // ...rest props are used in the Button component
    ref?: Ref<HTMLElement>
    css?: CSSProp
    isLink?: boolean
    color?: string
    href?: string
  }
): ReactElement
