import { ReactNode, Ref, HTMLProps, ReactElement } from 'react'

export default function Typeahead(
  props: Omit<HTMLProps<HTMLElement>, 'onSelect' | 'onChange'> & {
    ref?: Ref<HTMLElement>
    children?: ReactNode
    icon?: ReactNode
    onChange?: (...arg: any[]) => void
    onSelect?: (...arg: any[]) => void
    noFetch?: boolean
    preservePlaceholder?: boolean
    zIndex?: string
  }
): ReactElement
