import { ReactNode, ReactElement } from 'react'

export default function BackButton(props: {
  children?: ReactNode
  to?: string
  href?: string
  className?: string
  onClick?: () => void
  disabled?: boolean
}): ReactElement
