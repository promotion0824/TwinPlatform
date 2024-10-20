/* eslint-disable @typescript-eslint/no-explicit-any */
import { ReactNode, Ref } from 'react'

export default function Select(props: {
  header?: () => ReactNode
  value?: any
  error?: string
  placeholder?: string
  readOnly?: boolean
  disabled?: boolean
  unselectable?: boolean
  height?: string
  border?: string
  isPillSelect?: boolean
  url?: string
  params?: unknown
  cache?: unknown
  notFound?: unknown
  mock?: unknown
  className?: string
  children?: ReactNode
  onChange?: (nextValue: any) => void
  isFontLight?: boolean
  formatPlaceholder?: boolean
  isMultiSelect: boolean
  /**
   * Whether to do a partial value check in determining if an Option is selected
   * when both selected value and Option's value are object.
   *
   * In this case, a deep equal check is used to compare all properties in selected
   * value and partial properties in the Option's value if key is present in the
   * selected value. - Don't know why this partial check is in place.
   */
  partialValueCheckDisabled?: boolean
  ref: Ref<HTMLElement>
}): ReactNode
