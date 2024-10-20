import { HTMLProps, PropsWithChildren, ReactElement } from 'react'

export default function TypeaheadButton(
  props: Omit<HTMLProps<HTMLButtonElement>, 'value'> &
    PropsWithChildren<{
      value: any
    }>
): ReactElement
