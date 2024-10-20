import { PropsWithChildren, ReactElement } from 'react'

export default function FormControl(
  props: PropsWithChildren<{
    labelId?: string
    label?: string
    name?: string
    /**
     * Error message
     */
    error?: string
    /**
     * Name of form's field (via useForm) with error.
     */
    errorName?: string
    value?: any
    initialValue?: any
    showError?: boolean
    readOnly?: boolean
    required?: boolean
    onChange?: (value: any, options?: any[]) => void
  }>
): ReactElement
