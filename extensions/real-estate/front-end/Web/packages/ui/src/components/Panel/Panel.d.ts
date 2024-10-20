import { ReactNode, HTMLProps, ReactElement, ComponentProps } from 'react'
import Flex from '../Flex/Flex'

export default function Panel(
  props: HTMLProps<HTMLElement> &
    ComponentProps<typeof Flex> & {
      horizontal?: boolean
      fill?: string
      $borderWidth?: string
      children?: ReactNode
    }
): ReactElement
