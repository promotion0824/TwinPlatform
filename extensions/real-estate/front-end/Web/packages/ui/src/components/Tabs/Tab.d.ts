import { ReactNode, ReactElement } from 'react'

export default function Tab(props: {
  autoFocus?: boolean
  header?: ReactNode
  count?: number
  persist?: boolean
  selected?: boolean
  to?: string
  type?: 'modal'
  children?: ReactNode
  onClick?: () => void
}): ReactElement
