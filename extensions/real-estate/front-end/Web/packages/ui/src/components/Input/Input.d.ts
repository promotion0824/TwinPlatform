import { ReactElement, ReactNode, Ref } from 'react'

export default function Input(
  props: HTMLProps<HTMLInputElement> & {
    className?: string
    name?: string
    label?: ReactNode
    placeholder?: string
    preservePlaceholder?: boolean
    value?: string
    icon?: string | ReactElement
    debounce?: boolean
    border?: string
    height?: string
    onChange?: (search: string) => void
    readOnly?: boolean
    disabled?: boolean
    ref?: Ref<HTMLInputElement>
  }
): ReactElement
