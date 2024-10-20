import { styled } from 'twin.macro'
import Portal from '../../components/Portal/Portal'
import Snackbar from './Snackbar'
import Toast from './Toast'

interface Snackbar {
  snackbarId: string
  message: string
  isClosing: boolean
  description?: string
  icon?: string
  onClose: () => void
}

interface Toast {
  snackbarId: string
  message: string
  isClosing: boolean
  onClose: () => void
  isError: boolean
  closeButtonLabel: string
  height?: string
  color?: string
}

export default function Snackbars({
  snackbars,
  toasts,
}: {
  snackbars: Snackbar[]
  toasts: Toast[]
}) {
  return (
    <Portal>
      {/**
       * Snackbar is notification which is displayed at top of page and
       * will stack on bottom of each other,
       * and when a snackbar expires, it will disappear from the top of page.
       */}
      <StyledFlex>
        {snackbars.map((snackbar) => (
          <Snackbar key={snackbar.snackbarId} snackbar={snackbar} />
        ))}
      </StyledFlex>

      {/**
       * Toast is notification which is displayed at bottom of the page by default and
       * will stack on top of each other
       * and when a toast expires, it will disappear from the bottom of page.
       */}
      <StyledFlex>
        {toasts.map((toast) => (
          <Toast key={toast.snackbarId} toast={toast} />
        ))}
      </StyledFlex>
    </Portal>
  )
}

const StyledFlex = styled.div({
  display: 'flex',
  marginBottom: '51px',
  flexDirection: 'column-reverse',
  marginLeft: 'auto',
  marginRight: 'auto',
  width: '100%',
  position: 'fixed',
  pointerEvents: 'none',
  zIndex: 'var(--z-snackbars)',
  alignItems: 'center',
  bottom: '0%',
})
