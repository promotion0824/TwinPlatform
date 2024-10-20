import { Icon } from '@willowinc/ui'
import { forwardRef } from 'react'
import { styled } from 'styled-components'

interface CountSummaryProps extends React.HTMLAttributes<HTMLDivElement> {
  count: number
  summaryType: 'insights' | 'tickets'
  onClick?: (event: React.MouseEvent) => void
  label: string
  intent?: 'secondary' | 'negative'
}

const iconMap = {
  insights: 'emoji_objects',
  tickets: 'assignment',
} as const

const CountSummary = forwardRef<HTMLDivElement, CountSummaryProps>(
  (
    { count, onClick, summaryType, label, intent = 'secondary', ...rest },
    ref
  ) => (
    <Container
      onClick={onClick}
      $count={count}
      $negative={intent === 'negative'}
      ref={ref}
      role="button"
      {...rest}
    >
      <SummaryIcon icon={iconMap[summaryType]} />
      {label && <SummaryLabel>{label}</SummaryLabel>}
    </Container>
  )
)

const Container = styled.div<{ $count: number; $negative: boolean }>(
  ({ $count, theme, $negative }) => ({
    maxWidth: 'fit-content',
    alignItems: 'center',
    cursor: 'pointer',
    display: 'flex',
    gap: theme.spacing.s2,

    '&:hover': {
      'div, span': {
        color: theme.color.neutral.fg.highlight,
      },
    },

    ...($negative &&
      $count > 0 && {
        'div, span': {
          color: theme.color.intent.negative.fg.default,
        },

        '&:hover': {
          'div, span': {
            color: theme.color.intent.negative.fg.hovered,
          },
        },
      }),
  })
)

const SummaryIcon = styled(Icon)(({ theme }) => ({
  // Cannot use style props here as parent needs to override the color
  color: theme.color.neutral.fg.default,
}))

const SummaryLabel = styled.div(({ theme }) => ({
  ...theme.font.body.lg.regular,
  color: theme.color.neutral.fg.muted,
  whiteSpace: 'nowrap',
}))

export default CountSummary
