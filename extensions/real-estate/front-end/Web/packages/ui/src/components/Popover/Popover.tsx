import { styled } from 'twin.macro'
import {
  useCallback,
  useRef,
  useState,
  CSSProperties,
  useLayoutEffect,
  MutableRefObject,
  ReactNode,
} from 'react'

import { Portal } from '@willow/ui'

export default function Popover({
  position,
  target,
  children,
}: {
  position: string
  target: MutableRefObject<HTMLElement>
  children: ReactNode
}) {
  const tooltipRef = useRef<HTMLDivElement>(null)
  const arrowRef = useRef<HTMLDivElement>(null)

  const [state, setState] = useState<{
    position: string
    style?: CSSProperties
    arrowStyle?: CSSProperties
  }>({
    position: position ?? 'top',
    style: undefined,
    arrowStyle: undefined,
  })

  const reset = useCallback(() => {
    const rect = target?.current?.getBoundingClientRect?.()
    const tooltipRect = tooltipRef?.current?.getBoundingClientRect?.()
    const arrowRect = arrowRef?.current?.getBoundingClientRect?.()

    if (rect == null || tooltipRect == null || arrowRect == null) {
      return
    }

    let nextPosition = position
    if (position == null) {
      nextPosition =
        rect.top - tooltipRect.height - 16 < 0 &&
        rect.bottom + tooltipRect.height + 16 < window.innerHeight
          ? 'bottom'
          : 'top'
    }

    const top =
      nextPosition === 'top' ? rect.top - tooltipRect.height : rect.bottom

    const left = rect.left + rect.width / 2 - tooltipRect.width / 2
    const maxLeft = window.innerWidth - tooltipRect.width - 16
    const nextLeft = Math.min(left, maxLeft)

    const arrowLeft =
      rect.left + rect.width / 2 - arrowRect.width / 2 - nextLeft

    setState({
      position: nextPosition,
      style: {
        top,
        left: nextLeft,
      },
      arrowStyle: {
        left: arrowLeft,
      },
    })
  }, [tooltipRef, arrowRef, target, position])

  useLayoutEffect(reset, [reset]) // use useLayoutEffect over useEffect to avoid initial flashing

  return (
    // Using a portal helps us sidestep some z-index issues.
    <Portal>
      <Container ref={tooltipRef} style={state.style} position={position}>
        <Content>{children}</Content>
        <Arrow ref={arrowRef} style={state.arrowStyle} position={position} />
      </Container>
    </Portal>
  )
}

const backgroundColor = '#1c1c1c'

const Container = styled.div<{ position: string }>(({ position }) => ({
  position: 'fixed',
  transform: position === 'top' ? 'translate(0, -12px)' : 'translate(0, 12px)',
}))

const Content = styled.div(({ theme }) => ({
  backgroundColor,
  border: `1px solid ${theme.color.neutral.border.default}`,
  borderRadius: 4,
  boxShadow: '0px 10px 20px #0000004D',
  maxWidth: '350px',
  minWidth: '200px',
  overflow: 'hidden',

  padding: '1em',
}))

const Arrow = styled.div<{ position: string }>(({ position, theme }) => ({
  position: 'absolute',
  width: 18,
  height: 8,

  // The arrow outline
  '&::before': {
    ...(position === 'top'
      ? {
          borderColor: `${theme.color.neutral.border.default} transparent transparent transparent`,
          borderWidth: '8px 9px 0 9px',
        }
      : {
          borderColor: `transparent transparent ${backgroundColor} transparent`,
          borderWidth: '0 9px 8px 9px',
          bottom: 0,
        }),
  },

  // The arrow body
  '&::after': {
    left: 2,
    ...(position === 'top'
      ? {
          borderColor: `${backgroundColor} transparent transparent transparent`,
          borderWidth: '6px 7px 0 7px',
        }
      : {
          borderColor: `transparent transparent ${theme.color.neutral.bg.panel.default} transparent`,
          borderWidth: '0 7px 6px 7px',
          bottom: 0,
        }),
  },

  '&::before, &::after': {
    borderStyle: 'solid',
    content: "''",
    position: 'absolute',
  },

  [position === 'top' ? 'bottom' : 'top']: -7,
}))
