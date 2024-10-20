import { ReactElement, HTMLAttributes, Ref } from 'react'

export default function Text(
  props: HTMLAttributes<
    HTMLSpanElement | HTMLHeadingElement | HTMLLabelElement
  > & {
    children?: ReactNode
    type?: 'h1' | 'h2' | 'h3' | 'h4' | 'label' | 'message'
    size?:
      | 'extraTiny'
      | 'tiny'
      | 'small'
      | 'medium'
      | 'large'
      | 'extraLarge'
      | 'extraExtraLarge'
      | 'huge'
      | 'hugeNew'
      | 'extraHuge'
      | 'massive'
    align?: 'left' | 'right' | 'center'
    color?:
      | 'text'
      | 'light'
      | 'white'
      | 'grey'
      | 'green'
      | 'yellow'
      | 'orange'
      | 'red'
      | 'inherit'
    weight?: 'medium' | 'bold' | 'extraBold'
    whiteSpace?: 'normal' | 'nowrap'
    width?: 'small'
    textTransform?: 'uppercase'
    ref?: Ref<HTMLSpanElement | HTMLHeadingElement | HTMLLabelElement>
  }
): ReactElement
