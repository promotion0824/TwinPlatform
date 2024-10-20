import {
  CSSProperties,
  ComponentProps,
  PropsWithChildren,
  ReactElement,
  ReactNode,
  Ref,
} from 'react'
import Button from '../Button/Button'

export type DropdownProps = ComponentProps<typeof Button> & {
  header?: ReactNode
  position?: string
  useMinWidth?: boolean
  contentClassName?: string
  contentStyle?: CSSProperties
  onIsOpenChange?: (isOpen: boolean) => void
  ref?: Ref<HTMLElement>
  /** update the z-index of the DropdownContent which renders inside a Portal */
  zIndex?: number
}

export default function Dropdown(
  props: PropsWithChildren<DropdownProps>
): ReactElement

type DropdownButtonProps = ComponentProps<typeof Button> & {
  closeOnClick?: boolean
}

export function DropdownButton(props: DropdownButtonProps): ReactElement

export function useDropdown(): unknown
