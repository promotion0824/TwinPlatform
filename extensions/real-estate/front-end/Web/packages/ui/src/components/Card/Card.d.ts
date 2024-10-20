import { HTMLProps, ReactNode, ReactElement, PropsWithChildren } from 'react'
import { ButtonProps } from '../Button/Button'

export default function Card(props: {
  children?: ReactNode
  header?: string
  selected?: boolean
  className?: string
  onClick?: () => void
}): ReactElement

export function CardButton(
  props: HTMLProps<HTMLButtonElement> & PropsWithChildren<ButtonProps>
): ReactElement
