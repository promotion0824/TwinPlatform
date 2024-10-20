import { styled } from 'twin.macro'

export { default as FieldSet } from './FieldSet'

export const LabelText = styled.span({
  font: 'normal 500 11px/18px Poppins',
  color: '#959595',
})

export const FieldValidationText = styled.div({
  color: 'var(--red)',
  marginTop: 7,
  font: '400 11px/16px Poppins',
})
