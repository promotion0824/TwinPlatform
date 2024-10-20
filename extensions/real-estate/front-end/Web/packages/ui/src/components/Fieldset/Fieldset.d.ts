import { PropsWithChildren, ReactElement, ComponentProps } from 'react'
import Flex from '../Flex/Flex'

export default function Fieldset(
  props: Pick<ComponentProps<typeof Flex>, 'size' | 'padding' | 'marginTop'> &
    PropsWithChildren<{
      icon?: string
      legend?: string
      required?: boolean
      error?: string
      className?: string
      classNameChildrenCtn?: string
      columnWidth?: 'column' | 'columnSpacingSmall' | 'columnTiny'
      spacing?: 'column' | 'columnSpacingSmall' | 'columnTiny'
      heightSpecial?: 'special'
      scroll?: 'scroll' | boolean
      legendSize?: 'tiny'
    }>
): ReactElement
