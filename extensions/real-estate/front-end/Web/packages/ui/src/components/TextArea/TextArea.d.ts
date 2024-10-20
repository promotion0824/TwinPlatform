import { ReactElement, ReactNode, Ref } from 'react'

export default function TextArea(
  props: HTMLProps<HTMLTextAreaElement> & {
    className?: string
    name?: string
    label?: ReactNode
    placeholder?: string
    value?: string
    debounce?: boolean
    border?: string
    height?: string
    onChange?: (search: string) => void
    readOnly?: boolean
    disabled?: boolean
    ref?: Ref<HTMLInputElement>
  }
): ReactElement
