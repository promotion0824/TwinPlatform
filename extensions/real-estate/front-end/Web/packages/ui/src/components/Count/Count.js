import { styled } from 'twin.macro'

const Count = styled.div((props) => ({
  position: 'absolute',
  top: 0,
  right: 0,
  padding: '0 0.5rem',
  color: '#fff', // non-standard
  fontSize: 'var(--font-tiny)',
  fontWeight: 'var(--font-weight-500)',
  backgroundColor: props.isSelected ? '#733BE9' : '#7F7E7F', // non-standard
  borderRadius: '9999px', // makes shortest dimension fully curved
  animation: 'appear 0.2s',

  '@keyframes appear': {
    '0%': {
      opacity: 0,
      transform: 'scale(0)',
    },
  },
}))
export default Count
