import { HTMLProps, ReactElement, ReactNode } from 'react'

export default function Table(
  props: HTMLProps & {
    items?: any[]
    notFound?: string
    className?: string
    tableClassName?: string
    tableStyle?: string
    onTimesSortChange?: () => void
    children?: ReactNode
    isLoading?: boolean
    isError?: boolean
  }
): ReactElement

export function Head(
  props: HTMLProps & {
    type?: string
    isVisible?: boolean
  }
): ReactElement

export { default as Body } from './Body'
export { default as Cell } from './Cell/Cell'
export { default as Row } from './Row'
export function useTable(): any
