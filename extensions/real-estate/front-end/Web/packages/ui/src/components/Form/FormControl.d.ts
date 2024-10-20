import { ReactComponentElement, ReactElement, ReactNode } from 'react'

export default function FormControl(props: {
  id?: string
  name?: string
  errorName?: string
  label?: string
  value?: any
  defaultValue?: any
  error?: boolean
  readOnly?: boolean
  disabled?: boolean
  required?: boolean
  onChange?: (val: any, ...args: any[]) => void
  clearError?: () => void
  setData?: () => void
  hiddenLabel?: string
  // Extending children to accept ReactComponentElement as ReactNode in types-react 18
  // don't seems to accept Child Component Function
  children?: ReactNode | ReactComponentElement
}): ReactElement
