import React, { useCallback, useEffect, useRef, useState } from 'react'
import styled, { keyframes, css } from 'styled-components'
import { useSnackbar } from './SnackbarContext'
import { useTimer } from '../../hooks'
import Button from '../../components/Button/Button'

/**
 * Toast is a variant of snackbar,
 * where the popup will be displayed at the bottom of the page instead of at the top.
 * https://www.figma.com/file/aqZ4k3c0DlOlGe7gaxFNch/Willow-UI-Kit-v1?node-id=9%3A15574
 */
export default function Toast({ toast }) {
  const timer = useTimer()
  const snackbarContext = useSnackbar()
  const snackbarRef = useRef<HTMLDivElement>(null)

  // Setting toast's style maxHeight, to ensure toast exit animation looks smooth.
  const [style, setStyle] = useState<{ maxHeight: number }>()

  const {
    snackbarId,
    isClosing,
    message,
    isError,
    closeButtonLabel,
    height,
    color,
  } = toast

  const hide = useCallback(async () => {
    if (snackbarRef.current)
      setStyle({
        maxHeight: snackbarRef.current.offsetHeight,
      })
    snackbarContext.hide({ snackbarId, isToast: true })
  }, [snackbarContext, snackbarId])

  const close = useCallback(async () => {
    await timer.sleep(200)
    snackbarContext.close({ snackbarId, isToast: true })
  }, [snackbarContext, snackbarId, timer])

  useEffect(() => {
    if (isClosing) {
      close()
    }
  }, [isClosing, close])

  // Initially set up timer that will be used to determine how long the toast will appear.
  useEffect(() => {
    async function start() {
      await timer.setTimeout(5200)
      hide()
    }
    start()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  return (
    <ToastContainer
      ref={snackbarRef}
      style={style} // Set maxHeight to ensure toast's disappearing animation looks smooth, from 64px to 0px.
      $isClosing={isClosing}
      $height={height}
      className="ignore-onclickoutside"
    >
      <StatusIndicator $isError={isError} $height={height} />
      <MessageContainer>
        <Text $height={height} color={color}>
          {message}
        </Text>
      </MessageContainer>
      <ContentContainer>
        <TimerIndicator $height={height} />
      </ContentContainer>
      <CloseButton onClick={hide}>{closeButtonLabel}</CloseButton>
    </ToastContainer>
  )
}

const enterKeyFrames = keyframes`{
  0% {
    opacity: 0;
    transform: translate(0, 50px) scale(0.5);
  }
}`

const exitKeyFrames = keyframes`{
  100% {
    max-height: 0;
    opacity: 0;
    transform: translate(0, 50px) scale(0.5);
  }
}`

const exitAnimation = css`
  ${exitKeyFrames} 0.2s ease forwards;
`
const enterAnimation = css`
  ${enterKeyFrames} 0.2s ease forwards;
`
const ToastContainer = styled.div<{ $isClosing: boolean; $height: string }>`
  display: flex;
  flex-direction: row;
  align-items: start;

  width: 384px;
  height: ${({ $height }) => $height ?? '64px'};
  background: #1c1c1c;

  filter: drop-shadow(0px 4px 4px rgba(0, 0, 0, 0.25));

  border-radius: 2px;

  margin-top: var(--padding-large);
  pointer-events: all;

  animation: ${({ $isClosing }) =>
    $isClosing ? exitAnimation : enterAnimation};

  z-index: ${({ $isClosing }) =>
    $isClosing ? `calc(var(--z-snackbars) - 1)` : `var(--z-snackbars)`};
`

const StatusIndicator = styled.div<{ $isError: boolean; $height: string }>(
  ({ $isError, $height }) => ({
    width: '1.04%',
    height: $height ?? '64px',
    background: $isError ? '#FC2D3B' : '#383838',
    borderRadius: '2px 0px 0px 2px',
  })
)

const ContentContainer = styled.div({
  borderWidth: '1px 0',
  borderStyle: 'solid',
  borderColor: '#383838',
  width: '79.43%',
  height: '100%',
})

const barEnterKeyframe = keyframes`{
    0% {
        transform: scaleX(0);

      }
  }
`

const barEnterAnimation = css`
  ${barEnterKeyframe} 5.2s linear
`

const TimerIndicator = styled.div<{ $height: string }>`
  width: 100%;
  height: ${({ $height }) => $height ?? '62px'};
  position: relative;
  top: -0.1px;
  background: #252525;

  animation: ${barEnterAnimation};

  transform-origin: left;
  transition: all 0.5s ease;
`

const MessageContainer = styled.div({
  position: 'absolute',
  zIndex: 'var(--z-snackbars)',
})

const Text = styled.div<{ $height: string; color?: string }>(
  ({ theme, $height, color }) => ({
    ...theme.font.body.md,
    width: '305px',
    height: $height ?? '64px',
    display: 'flex',
    alignItems: 'center',
    paddingLeft: 24,
    color: color ?? theme.color.neutral.fg.default,

    textOverflow: 'ellipsis',
    overflow: 'hidden',
  })
)

const CloseButton = styled(Button)({
  width: '19.53%',
  height: '100%',
  background: '#1C1C1C',

  border: '1px solid #383838',
  borderRadius: '0 2px 2px 0',

  color: '#8779E2',
  font: '600 12px/20px Poppins',

  display: 'flex',
  justifyContent: 'center',
  alignItems: 'center',

  zIndex: 'var(--z-snackbars)',

  '&:hover': { background: '#2B2B2B', color: '#8779E2 !important' },
})
