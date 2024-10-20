import { forwardRef } from 'react'
import { Checkbox, CheckboxProps } from '../../../inputs/Checkbox'
import styled from 'styled-components'

const BaseInputCheckbox = forwardRef<
  HTMLInputElement,
  CheckboxProps & { touchRippleRef?: unknown } & {
    inputProps?: React.InputHTMLAttributes<HTMLInputElement>
  }
>(({ touchRippleRef, inputProps, ...props }, ref) => {
  // remove touchRippleRef comes from MUI from passing down to label element to avoid warnings
  return (
    <Hitbox>
      <Checkbox {...props} ref={ref} />
    </Hitbox>
  )
})

/**
 * Wrapper component that will increase hitbox of checkbox to make it easier to toggle on/off.
 */
const Hitbox = styled('label')({
  width: '100%',
  height: '100%',
  position: 'absolute',
  top: 0,
  left: 0,
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
})

export default BaseInputCheckbox
