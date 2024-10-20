import {
  HTMLProps,
  PropsWithChildren,
  ComponentProps,
  Ref,
  ReactElement,
} from 'react'
import FormControl from '../FormNew/FormControl'

export default function TextArea(
  props: ComponentProps<typeof FormControl> &
    HTMLProps<HTMLTextAreaElement> &
    PropsWithChildren<{
      ref?: Ref
      debounce?: () => void
      label?: string
    }>
): ReactElement
