import {
  HTMLProps,
  PropsWithChildren,
  ComponentProps,
  Ref,
  ReactElement,
} from 'react'
import FormControl from '../FormNew/FormControl'

export default function Input(
  props: ComponentProps<typeof FormControl> &
    HTMLProps<HTMLInputElement> &
    PropsWithChildren<{
      ref?: Ref
      debounce?: () => void
      label?: string
      readOnly?: boolean
    }>
): ReactElement
