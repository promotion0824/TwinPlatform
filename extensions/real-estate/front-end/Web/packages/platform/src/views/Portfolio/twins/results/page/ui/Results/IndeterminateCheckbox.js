import { forwardRef, useEffect, useRef } from 'react'
import { styled } from 'twin.macro'

const Checkbox = styled.input({
  appearance: 'none',
  height: 13,
  width: 13,
  backgroundColor: '#383838',
  borderRadius: 2,
  display: 'inline-block',
  position: 'relative',

  '&::before': {
    content: '""',
    position: 'absolute',
    margin: 'auto',
    left: 0,
    right: 0,
    bottom: 0,
    overflow: 'hidden',
    top: 0,
  },

  '&:checked': {
    backgroundColor: '#959595',
  },

  '&:checked::before': {
    borderRight: '2px solid #252525',
    borderBottom: '2px solid #252525',
    borderRadius: 1,
    height: '60%',
    width: '35%',
    transform: 'rotate(45deg) translateY(-10%) translateX(-10%)',
  },

  '&:indeterminate, &[aria-checked=mixed]': {
    backgroundColor: '#959595',
  },

  '&:indeterminate::before, &[aria-checked=mixed]::before': {
    backgroundColor: '#252525',
    border: '1px solid #252525',
    borderRadius: 1,
    height: 0,
    width: '50%',
  },
})

const IndeterminateCheckbox = forwardRef(({ indeterminate, ...rest }, ref) => {
  // Interminate checkboxes aren't a behavioural feature of HTML, but they do exist visually.
  // This a the way to get them working.
  const defaultRef = useRef()
  const resolvedRef = ref || defaultRef

  useEffect(() => {
    resolvedRef.current.indeterminate = indeterminate
  }, [resolvedRef, indeterminate])

  return <Checkbox type="checkbox" ref={resolvedRef} {...rest} />
})

export default IndeterminateCheckbox
