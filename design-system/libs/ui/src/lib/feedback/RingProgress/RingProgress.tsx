import { Box, BoxProps } from '@mantine/core'
import { useTheme } from '@willowinc/theme'
import { forwardRef } from 'react'
import styled from 'styled-components'
import { Intent } from '../../common'
import { Icon, IconName } from '../../misc/Icon'
import {
  WillowStyleProps,
  useWillowStyleProps,
} from '../../utils/willowStyleProps'

export interface RingProgressProps
  extends WillowStyleProps,
    Omit<BoxProps, keyof WillowStyleProps> {
  /**
   * Icon to be displayed inside the ring.
   * Only displayed in the large size ring.
   * Will take priority over `showValue`.
   */
  icon?: IconName
  /**
   * Dictates the color of the ring.
   * @default 'primary'
   */
  intent?: Intent
  /**
   * Shows the value inside the ring.
   * Only displayed in the large size ring.
   * Won't be displayed if an icon is provided.
   * @default false
   */
  showValue?: boolean
  /**
   * Size of the ring.
   * @default 'lg'
   */
  size?: 'xs' | 'lg'
  /** Amount of the ring to the filled in. */
  value: number
}

const LabelContainer = styled.div<{ $intent: Intent }>(
  ({ $intent, theme }) => ({
    alignItems: 'center',
    backgroundColor: theme.color.intent[$intent].bg.subtle.default,
    borderRadius: theme.radius.round,
    color: theme.color.intent[$intent].fg.default,
    display: 'flex',
    justifyContent: 'center',
    position: 'absolute',

    // Only the large size needs to be supported
    height: '48px',
    width: '48px',
  })
)

const ProgressValue = styled.div(({ theme }) => ({
  ...theme.font.body.lg.semibold,
}))

/**
 * `RingProgress` displays feedback on progress using a ring.
 */
export const RingProgress = forwardRef<HTMLDivElement, RingProgressProps>(
  (
    {
      icon,
      intent = 'primary',
      showValue = false,
      size = 'lg',
      value,
      ...restProps
    },
    ref
  ) => {
    const theme = useTheme()

    const strokeWidth = size === 'xs' ? 3 : 4
    const width = size === 'xs' ? 16 : 48
    const radius = (width - strokeWidth) / 2
    const circumference = radius * 2 * Math.PI

    return (
      <Box
        ref={ref}
        style={{ height: width, position: 'relative', width: width }}
        {...restProps}
        {...useWillowStyleProps(restProps)}
      >
        {size === 'lg' && (icon || showValue) && (
          <LabelContainer $intent={intent}>
            {icon ? (
              <Icon icon={icon} size={24} />
            ) : (
              <ProgressValue>{value}</ProgressValue>
            )}
          </LabelContainer>
        )}

        <svg
          height={width}
          width={width}
          style={{ transform: 'rotate(-90deg)' }}
        >
          <circle
            cx={width / 2}
            cy={width / 2}
            fill="transparent"
            r={radius}
            stroke={theme.color.core.gray.bg.subtle.default}
            strokeWidth={strokeWidth}
          />
          <circle
            cx={width / 2}
            cy={width / 2}
            fill="transparent"
            r={radius}
            stroke={theme.color.intent[intent].fg.default}
            strokeDasharray={circumference}
            strokeDashoffset={circumference * ((100 - value) / 100)}
            strokeLinecap="round"
            strokeWidth={strokeWidth}
          />
        </svg>
      </Box>
    )
  }
)
