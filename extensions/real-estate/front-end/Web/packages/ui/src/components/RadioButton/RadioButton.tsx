import { ChangeEvent } from 'react'
import styled from 'styled-components'

const RadioButton = ({
  value,
  checked,
  onChange,
}: {
  value: string
  checked: boolean
  onChange: (e: ChangeEvent<HTMLInputElement>) => void
}) => <Input type="radio" value={value} checked={checked} onChange={onChange} />

export default RadioButton

const Input = styled.input({
  position: 'relative',
  appearance: 'none',
  cursor: 'pointer',

  borderRadius: '50%',
  width: '1rem',
  height: '1rem',

  border: '2px solid var(--lighter)',
  transition: '0.2s all linear',
  marginRight: '0.5rem',

  display: 'inline-grid',
  placeContent: 'center',

  '&::before': {
    content: '""',
    width: '0.5rem',
    height: '0.5rem',
    borderRadius: '50%',
    boxShadow: 'inset 1rem 1rem var(--lighter)',

    transform: 'scale(0)',
    transition: '200ms transform ease-in-out',
  },

  '&:checked::before': {
    transform: 'scale(1)',
  },
})
