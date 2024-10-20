import { forwardRef } from 'react'
import styled from 'styled-components'
import { Icon } from '../../misc/Icon'
import { Tooltip } from '../../overlays/Tooltip'
import { rem } from '../../utils'
import { MetricTrend, MetricTrendProps } from './MetricTrend'

export interface MetricCardProps {
  /** An optional description that is displayed in a tooltip next to the title. */
  description?: string
  /**
   * Show a thousands separator in the metric value.
   * @default true
   */
  showThousandsSeparator?: boolean
  /** The name of the metric. */
  title: string
  /** The amount the trend has changed by, expressed as a percentage. */
  trendDifference: MetricTrendProps['difference']
  /** The direction of the trend's current value compared to its previous value. */
  trendDirection: MetricTrendProps['direction']
  /**
   * The feeling that the trend's change should convey.
   * @default neutral
   */
  trendSentiment: MetricTrendProps['sentiment']
  /** The value of the trend. */
  trendValue: MetricTrendProps['value']
  /** Optionally display the type of units being measured. */
  units?: string
  /** The value of the metric. */
  value: number
}

const MetricCardContainer = styled.div(({ theme }) => ({
  background: theme.color.neutral.bg.panel.default,
  border: `1px solid ${theme.color.neutral.border.default}`,
  borderRadius: theme.radius.r4,
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s8,
  padding: `${theme.spacing.s12} ${theme.spacing.s16}`,
}))

const MetricTitle = styled.div(({ theme }) => ({
  ...theme.font.heading.sm,
  color: theme.color.neutral.fg.default,
  display: 'flex',
  gap: theme.spacing.s4,
  overflow: 'hidden',
  textOverflow: 'ellipsis',
}))

const MetricValue = styled.div(({ theme }) => ({
  // TODO: The token below doesn't currently exist, so the font styles are being provided
  // inline. They can be removed once this font token has been added.
  // ...theme.font.display.xl.medium,
  fontFamily: 'Poppins, Arial, sans-serif',
  fontSize: theme.spacing.s32,
  fontWeight: 500,
  lineHeight: theme.spacing.s40,

  color: theme.color.neutral.fg.default,
  overflow: 'hidden',
  textOverflow: 'ellipsis',
}))

const MetricValueContainer = styled.div(({ theme }) => ({
  alignItems: 'baseline',
  display: 'flex',
  gap: theme.spacing.s4,
}))

const MetricValueUnits = styled.div(({ theme }) => ({
  // TODO: The token below doesn't currently exist, so the font styles are being provided
  // inline. They can be removed once this font token has been added.
  // ...theme.font.display.sm.regular,
  fontFamily: 'Poppins, Arial, sans-serif',
  fontSize: theme.spacing.s20,
  fontWeight: 400,
  lineHeight: rem(28),
}))

const TooltipIcon = styled(Icon)({
  cursor: 'default',
})

/**
 * `MetricCard` is a component used to display metrics within dashboards.
 */
export const MetricCard = forwardRef<HTMLDivElement, MetricCardProps>(
  (
    {
      description,
      showThousandsSeparator = true,
      title,
      trendDifference,
      trendDirection,
      trendSentiment = 'neutral',
      trendValue,
      units,
      value,
      ...restProps
    },
    ref
  ) => {
    return (
      <MetricCardContainer {...restProps} ref={ref}>
        <MetricTitle>
          <div>{title}</div>
          {description && (
            <Tooltip label={description} position="top" withArrow withinPortal>
              <TooltipIcon filled icon="info" />
            </Tooltip>
          )}
        </MetricTitle>
        <MetricValueContainer>
          <MetricValue>
            {showThousandsSeparator ? value.toLocaleString('en-US') : value}
          </MetricValue>
          {units && <MetricValueUnits>{units}</MetricValueUnits>}
        </MetricValueContainer>
        <MetricTrend
          difference={trendDifference}
          direction={trendDirection}
          sentiment={trendSentiment}
          showThousandsSeparator={showThousandsSeparator}
          value={trendValue}
        />
      </MetricCardContainer>
    )
  }
)
