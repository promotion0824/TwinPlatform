import { HTMLProps, forwardRef } from 'react'

export interface ErrorProps extends HTMLProps<HTMLDivElement> {}

/**
 * `Error` displays error message for form component.
 */
export const Error = forwardRef<HTMLDivElement, ErrorProps>(
  ({ className, ...restProps }, ref) => {
    const classes =
      (className ? className + ' ' : '') + 'mantine-InputWrapper-error'
    return <div {...restProps} ref={ref} className={classes} />
  }
)
