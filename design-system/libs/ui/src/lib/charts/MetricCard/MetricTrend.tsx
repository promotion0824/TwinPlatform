import { forwardRef } from 'react'
import styled from 'styled-components'
import { Icon } from '../../misc/Icon'

export interface MetricTrendProps {
  /** The amount the trend has changed by, expressed as a percentage. */
  difference: number
  /** The direction of the trend's current value compared to its previous value. */
  direction: 'downwards' | 'sidewards' | 'upwards'
  /**
   * The feeling that the trend's change should convey.
   * @default neutral
   */
  sentiment: 'negative' | 'neutral' | 'notice' | 'positive'
  /**
   * Show a thousands separator in the metric value.
   * @default true
   */
  showThousandsSeparator?: boolean
  /** The value of the trend. */
  value: number
}

const MetricTrendContainer = styled.div<{
  $sentiment: MetricTrendProps['sentiment']
}>(({ $sentiment, theme }) => ({
  display: 'flex',
  gap: theme.spacing.s2,

  color:
    $sentiment === 'negative'
      ? theme.color.intent.negative.fg.default
      : $sentiment === 'notice'
      ? theme.color.intent.notice.fg.default
      : $sentiment === 'positive'
      ? theme.color.intent.positive.fg.default
      : theme.color.intent.secondary.fg.default,
}))

const MetricTrendValue = styled.div(({ theme }) => ({
  ...theme.font.body.md.semibold,
}))

const MetricTrendDifference = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
}))

export const MetricTrend = forwardRef<HTMLDivElement, MetricTrendProps>(
  (
    {
      difference,
      direction,
      sentiment = 'neutral',
      showThousandsSeparator = true,
      value,
      ...restProps
    },
    ref
  ) => {
    return (
      <MetricTrendContainer $sentiment={sentiment} {...restProps} ref={ref}>
        <Icon
          icon={
            direction === 'downwards'
              ? 'arrow_downward'
              : direction === 'sidewards'
              ? 'arrow_forward'
              : 'arrow_upward'
          }
        />

        <MetricTrendValue>
          {showThousandsSeparator ? value.toLocaleString('en-US') : value}
        </MetricTrendValue>

        <MetricTrendDifference>({difference}%)</MetricTrendDifference>
      </MetricTrendContainer>
    )
  }
)
