import { Badge, BadgeProps, Icon, IconName } from '@willowinc/ui'
import { forwardRef } from 'react'

export interface NumberTileTrendBadgeProps {
  /** Dictates the color of the badge. */
  sentiment: 'negative' | 'neutral' | 'notice' | 'positive'
  /** The direction of the arrow shown on the badge. */
  trend: 'downwards' | 'sidewards' | 'upwards'
  /** Value shown on the badge. */
  value: string
}

const sentimentColorMap: Record<
  NumberTileTrendBadgeProps['sentiment'],
  BadgeProps['color']
> = {
  negative: 'red',
  neutral: 'gray',
  notice: 'orange',
  positive: 'green',
} as const

const trendIconMap: Record<NumberTileTrendBadgeProps['trend'], IconName> = {
  downwards: 'arrow_downward',
  sidewards: 'arrow_forward',
  upwards: 'arrow_upward',
} as const

export default forwardRef<HTMLDivElement, NumberTileTrendBadgeProps>(
  ({ sentiment, trend, value }, ref) => {
    const color = sentimentColorMap[sentiment]

    return (
      <Badge
        color={color}
        ref={ref}
        prefix={<Icon icon={trendIconMap[trend]} size={16} />}
        size="sm"
        variant="subtle"
      >
        {value}
      </Badge>
    )
  }
)
