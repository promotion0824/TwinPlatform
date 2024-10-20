import { styled } from 'twin.macro'

/**
 * A little almost-circular box with a number in it.
 */
const CountChip = styled.div<{ isExpanded?: boolean }>(({ isExpanded }) => ({
  color: '#7e7e7e',
  backgroundColor: isExpanded ? 'black' : '#383838',
  borderRadius: '15px',
  padding: '0 6px',
  font: 'normal 500 10px Poppins',
  display: 'inline-flex',
  alignItems: 'center',
  textAlign: 'center',
}))

export default CountChip
