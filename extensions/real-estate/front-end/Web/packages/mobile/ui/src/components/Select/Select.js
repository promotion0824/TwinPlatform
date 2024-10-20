import { forwardRef } from 'react'
import { useFormControl, withFormControl } from 'components/Form/Form'
import Label from 'components/Label/Label'
import SelectComponent from './Select/Select'

export { default as Option } from './Select/Option'

export default withFormControl()(
  forwardRef((props, forwardedRef) => {
    const {
      labelId,
      label,
      labelClassName,
      name,
      value,
      readOnly,
      onChange,
      ...rest
    } = props

    const control = useFormControl()

    function handleChange(nextValue) {
      onChange(nextValue)
    }

    return (
      <Label
        ref={forwardedRef}
        labelId={labelId}
        className={labelClassName}
        label={label}
      >
        {(labelContext) => (
          <SelectComponent
            {...rest}
            id={labelContext.id}
            value={value}
            disabled={control.disabled}
            readOnly={control.readOnly}
            onChange={handleChange}
          />
        )}
      </Label>
    )
  })
)
