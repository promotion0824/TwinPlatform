import {
  Indicator as MantineIndicator,
  IndicatorProps as MantineIndicatorProps,
} from '@mantine/core'
import { useTheme } from '@willowinc/theme'
import { forwardRef } from 'react'
import styled, { css } from 'styled-components'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

export interface IndicatorProps
  extends WillowStyleProps,
    Omit<
      MantineIndicatorProps,
      keyof WillowStyleProps | 'label' | 'withBorder' | 'position'
    > {
  /**
   * Color intent for Indicator
   * @default primary
   */
  intent?: 'primary' | 'secondary' | 'negative' | 'positive' | 'notice'
  /** Specify the content of Indicator. */
  children: React.ReactNode
  /**
   * Determines whether indicator should have border
   * @default false
   */
  hasBorder?: boolean
  /** Indicator label */
  label?: React.ReactNode
  /**
   * Indicator position relative to child element
   * @default top-end
   */
  position?:
    | 'bottom-end'
    | 'bottom-start'
    | 'top-end'
    | 'top-start'
    | 'bottom-center'
    | 'top-center'
    | 'middle-center'
    | 'middle-end'
    | 'middle-start'
}

/**
 * `Indicator` is a customizable visual cue designed to represent and convey different intents or states within the application.
 */
export const Indicator = forwardRef<HTMLDivElement, IndicatorProps>(
  (
    {
      intent = 'primary',
      hasBorder = false,
      position = 'top-end',
      label,
      ...restProps
    },
    ref
  ) => {
    const color = useColors(intent)
    return (
      <StyledIndicator
        color={color}
        {...restProps}
        {...useWillowStyleProps(restProps)}
        withBorder={hasBorder}
        position={position}
        label={label}
        ref={ref}
      />
    )
  }
)

const useColors = (intent: Exclude<IndicatorProps['intent'], undefined>) => {
  const theme = useTheme()
  const colorTable = {
    primary: theme.color.intent.primary.bg.bold.default,
    secondary: theme.color.intent.secondary.bg.bold.default,
    negative: theme.color.intent.negative.bg.bold.default,
    positive: theme.color.intent.positive.bg.bold.hovered,
    notice: theme.color.intent.notice.bg.bold.default,
  }

  return colorTable[intent]
}

const StyledIndicator = styled(MantineIndicator)(
  ({ theme, label }) => css`
    .mantine-Indicator-indicator {
      ${theme.font.body.xs.regular};
      border-color: ${theme.color.neutral.bg.base.default};

      &[data-with-border='true'] {
        width: 14px;
        height: 14px;
      }

      ${label &&
      css`
        height: auto;
        padding: 0 ${theme.spacing.s6};
      `}
    }
  `
)
