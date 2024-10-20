import { ReactNode, Ref, ReactElement, HTMLProps } from 'react'

export interface FlexProps extends Omit<HTMLProps<HTMLDivElement>, 'size'> {
  ref?: Ref<HTMLElement>
  children?: ReactNode
  display?: 'inline'
  position?: 'fixed' | 'absolute' | 'relative'
  horizontal?: boolean
  fill?: string
  align?: string
  padding?: string
  flex?: string
  size?: 'tiny' | 'small' | 'medium' | 'large' | 'extraLarge'
  width?: 'page' | 'pageLarge' | 'minContent' | '100%'
  height?: 'medium' | 'large' | '100%' | 'special'
  border?: string
  overflow?: 'auto' | 'hidden' | 'initial'
  whiteSpace?: 'normal' | 'nowrap'
  marginTop?: boolean
  className?: string
}

/**
 * @deprecated Please do not use in new code.
 */
export default function Flex(props: FlexProps): ReactElement
