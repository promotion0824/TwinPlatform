import { HTMLProps, forwardRef } from 'react'

export interface LabelProps extends HTMLProps<HTMLLabelElement> {
  required?: boolean
}

/**
 * `Label` is used to specify a label for an input element of a form.
 */
export const Label = forwardRef<HTMLLabelElement, LabelProps>(
  ({ className = '', children, required, ...restProps }, ref) => {
    const classes =
      (className ? className + ' ' : '') + 'mantine-InputWrapper-label'
    return (
      <label {...restProps} ref={ref} className={classes}>
        {children}
        {required && <span className="mantine-InputWrapper-required"> *</span>}
      </label>
    )
  }
)
