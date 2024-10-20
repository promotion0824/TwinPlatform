import { ComponentProps, PropsWithChildren, ReactElement, Ref } from 'react'
import Dropdown from '../Dropdown/Dropdown'

export default function DatePicker(
  props: Omit<ComponentProps<typeof Dropdown>, 'type'> &
    PropsWithChildren<{
      type?: 'date' | 'date-time' | 'date-time-range' | 'date-range'
      min?: number
      max?: number
      placeholder?: string
      height?: string
      className?: string
      onChange?: (date: string) => void
      ref?: Ref
    }>
): ReactElement
