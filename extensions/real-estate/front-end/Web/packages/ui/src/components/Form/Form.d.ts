import { PropsWithChildren, HTMLProps, ReactElement } from 'react'

export interface FormProps
  extends PropsWithChildren<
    Omit<HTMLProps<HTMLFormElement>, 'onSubmit', 'defaultValue'>
  > {
  defaultValue?: any
  onSubmit?: (obj: any) => void
  onSubmitted?: (obj: any) => void
  readonly?: boolean
  success?: boolean
  className?: string
  preventBlockOnSubmitted?: boolean
  skipErrorSnackbar?: boolean
}

export default function Form(props: FormProps): ReactElement

export { default as FormControl } from './FormControl'
export function useForm(): any
export function useFormControl(): any

export class ValidationError extends Error {
  constructor(message: string | Array | Record<string, unknown>, name: string)
}
