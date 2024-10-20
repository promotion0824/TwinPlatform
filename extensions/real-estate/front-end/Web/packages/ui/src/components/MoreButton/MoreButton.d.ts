import {
  ComponentProps,
  PropsWithChildren,
  ReactElement,
  ReactEventHandler,
} from 'react'
import Button from '../Button/Button'

export default function MoreButton(
  props: PropsWithChildren<
    ComponentProps<typeof Button> & {
      type?: 'vertical'
      className?: string
      iconClassName?: string
      showTooltipArrow?: boolean
      onClick?: (event: ReactEventHandler) => void
    }
  >
): ReactElement

export { default as MoreDropdownButton } from './MoreDropdownButton'
