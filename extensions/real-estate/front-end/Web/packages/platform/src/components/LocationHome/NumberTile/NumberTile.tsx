import { Box, Group, Sparkline, SparklineProps, Tooltip } from '@willowinc/ui'
import classNames from 'classnames'
import { forwardRef } from 'react'
import styled from 'styled-components'
import {
  ArrowIcon,
  InteractiveTile,
  InteractiveTileProps,
  MutedIcon,
} from '../common'
import NumberTileTrendBadge, {
  NumberTileTrendBadgeProps,
} from './NumberTileTrendBadge'

export interface NumberTileProps
  extends Omit<InteractiveTileProps, 'children' | 'onClick' | 'title'> {
  /** Description shown in a tooltip next to the label. */
  description?: string
  /** Label displayed as the heading of the tile. */
  label: string
  /**
   * Function called when tile is clicked.
   * Also causes an arrow icon to be displayed when using a touch device.
   */
  onClick?: () => void
  /**
   * Size of the tile.
   * @default 'small'
   */
  size?: 'large' | 'small'
  /**
   * Trend information to be displayed on the tile as a badge.
   */
  trendingInfo?: NumberTileTrendBadgeProps
  /** Units of value being displayed. */
  unit: string
  /** Main value shown on the tile. */
  value: string
  /**
   * Sparkline chart information to be displayed at the bottom of the tile.
   */
  sparkline?: SparklineProps
}

const StyledGroup = styled(Group)(({ theme }) => ({
  '&.small': {
    alignItems: 'flex-end',
    gap: theme.spacing.s8,
  },

  '&.large': {
    display: 'block',
  },
}))

const StyledArrowIcon = styled(ArrowIcon)({
  marginLeft: 'auto',
})

const Label = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.default,
  overflow: 'hidden',
  textOverflow: 'ellipsis',
  whiteSpace: 'nowrap',
}))

const Unit = styled.div<{ $size: NumberTileProps['size'] }>(
  ({ $size, theme }) => ({
    ...theme.font.body.md.regular,
    color: theme.color.neutral.fg.default,
    lineHeight: $size === 'small' ? '22px' : '25px',
  })
)

const Value = styled.div<{ $size: NumberTileProps['size'] }>(
  ({ $size, theme }) => ({
    // There's currently an issue with the medium font tokens, so using the light ones
    // and overriding the font weight.
    ...($size === 'small'
      ? theme.font.display.sm.light
      : theme.font.display.lg.light),
    color: theme.color.neutral.fg.default,
    fontWeight: theme.font.display.lg.medium.fontWeight,
  })
)

export const NumberTile = forwardRef<HTMLDivElement, NumberTileProps>(
  (
    {
      description,
      label,
      onClick,
      size = 'small',
      trendingInfo,
      unit,
      value,
      sparkline,
      ...restProps
    },
    ref
  ) => (
    <InteractiveTile onClick={onClick} ref={ref} title={label} {...restProps}>
      <Group align="flex-start" gap="s4" wrap="nowrap">
        <Label>{label}</Label>

        {description && (
          <Tooltip label={description} multiline w={250} withinPortal>
            <MutedIcon icon="info" />
          </Tooltip>
        )}

        {onClick && <StyledArrowIcon />}
      </Group>

      <StyledGroup
        className={classNames('number-tile-body', { [size]: true })}
        align="flex-end"
        gap="s8"
        wrap="nowrap"
      >
        <Group align="flex-end" gap="s4" mr="auto" wrap="nowrap">
          <Value className="value" $size={size}>
            {value}
          </Value>
          <Unit className="unit" $size={size}>
            {unit}
          </Unit>
        </Group>
        {trendingInfo && <NumberTileTrendBadge {...trendingInfo} />}
      </StyledGroup>
      {sparkline && size === 'large' && (
        <Box className="number-tile-sparkline" h={36}>
          <Sparkline {...sparkline} />
        </Box>
      )}
    </InteractiveTile>
  )
)
