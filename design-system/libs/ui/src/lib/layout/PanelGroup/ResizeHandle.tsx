import { PanelResizeHandle } from 'react-resizable-panels'
import styled from 'styled-components'
import { Icon } from '../../misc/Icon'
import {
  PanelGroupContextProps,
  usePanelGroupContext,
} from './PanelGroupContext'

export interface ResizeHandleProps {
  className?: string
  disabled?: boolean
  id?: string
}

const Handle = styled.div<{
  $isVertical: boolean
}>(({ $isVertical, theme }) => ({
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',

  ...($isVertical
    ? {
        width: '100%',
        height: theme.spacing.s8,
      }
    : {
        width: theme.spacing.s8,
        height: '100%',
      }),
}))

const IconContainer = styled.div(({ theme }) => ({
  alignContent: 'center',
  background: theme.color.neutral.bg.accent.default,
  border: `1px solid ${theme.color.neutral.border.default}`,
  borderRadius: theme.radius.r2,
  color: theme.color.intent.secondary.fg.default,
  display: 'flex',
  flexWrap: 'wrap',
  height: theme.spacing.s6,
  zIndex: theme.zIndex.overlay,
}))

const StyledIcon = styled(Icon)({
  '&&': {
    fontVariationSettings: '"wght" 200, "opsz" 40',
  },
})

const StyledPanelResizeHandle = styled(PanelResizeHandle)<{
  $gapSize: PanelGroupContextProps['gapSize']
  $isVertical: boolean
}>(({ $gapSize, $isVertical, theme }) => {
  const size = $gapSize === 'medium' ? theme.spacing.s8 : theme.spacing.s4

  return {
    ...($isVertical
      ? {
          height: size,
        }
      : {
          width: size,
        }),

    ...($gapSize === 'small'
      ? {
          '&:hover': {
            backgroundColor: theme.color.state.disabled.fg,
            borderRadius: theme.radius.r4,
          },
        }
      : undefined),

    margin: $gapSize === 'medium' ? `0 ${theme.spacing.s2}` : '0',

    '&:where(:last-child, :not([aria-controls]))': {
      display: 'none',
    },

    '&[data-resize-handle-active="pointer"]': {
      backgroundColor: theme.color.state.disabled.fg,
      borderRadius: theme.radius.r4,

      '> div': {
        display: 'none',
      },
    },
  }
})

/**
 * `ResizeHandle` is handle that allows user to drag and resize a panel.
 *
 * @see https://github.com/bvaughn/react-resizable-panels/tree/main/packages/react-resizable-panels#panelresizehandle
 */
const ResizeHandle = (props: ResizeHandleProps) => {
  const { gapSize, isVertical } = usePanelGroupContext()

  return (
    <StyledPanelResizeHandle
      $gapSize={gapSize}
      $isVertical={isVertical}
      {...props}
    >
      {!props.disabled && (
        <Handle $isVertical={isVertical}>
          {gapSize === 'medium' && (
            <IconContainer
              css={{
                transform: !isVertical ? 'rotate(90deg)' : undefined,
              }}
            >
              <StyledIcon icon="drag_handle" />
            </IconContainer>
          )}
        </Handle>
      )}
    </StyledPanelResizeHandle>
  )
}

export default ResizeHandle
