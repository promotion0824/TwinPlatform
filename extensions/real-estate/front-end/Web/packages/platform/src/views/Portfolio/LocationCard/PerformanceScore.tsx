import { titleCase } from '@willow/common'
import { Group, RingProgress, Stack, useDisclosure } from '@willowinc/ui'
import { forwardRef } from 'react'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'
import PerformanceScorePopover from './PerformanceScorePopover'

const PerformanceScoreSubContainer = styled.div({
  alignItems: 'center',
  display: 'flex',
  flexDirection: 'column',
})

const Score = styled.div<{ $size?: PerformanceScoreProps['size'] }>(
  ({ theme, $size = 'md' }) => ({
    ...($size === 'xs'
      ? theme.font.body.lg.regular
      : theme.font.display.md.light),
    color: theme.color.neutral.fg.default,
  })
)

const ScoreSubheading = styled.div<{
  $size?: PerformanceScoreProps['size']
  $highlight: boolean
}>(({ theme, $size = 'md', $highlight }) => ({
  ...($size === 'xs' ? theme.font.body.xs.regular : theme.font.body.md.regular),
  color: $highlight
    ? theme.color.neutral.fg.default
    : theme.color.neutral.fg.muted,
}))

interface PerformanceScoreProps {
  comfortScore: number
  energyScore: number
  onClick: (event: React.MouseEvent) => void
  performanceScore: number
  size?: 'xs' | 'md'
}

export default function PerformanceScore({
  comfortScore,
  energyScore,
  onClick,
  performanceScore,
  size = 'md',
  ...restProps
}: PerformanceScoreProps) {
  const {
    i18n: { language },
    t,
  } = useTranslation()

  const [popoverOpened, { close: closePopover, open: openPopover }] =
    useDisclosure()

  const ringIntent =
    performanceScore >= 75
      ? 'positive'
      : performanceScore >= 50
      ? 'notice'
      : 'negative'
  const performanceTitle = titleCase({
    language,
    text: t('labels.performance'),
  })
  const PerformanceScoreContent =
    size === 'md' ? MdPerformanceScoreContent : XsPerformanceScoreContent

  return (
    <PerformanceScorePopover
      comfortScore={comfortScore}
      energyScore={energyScore}
      opened={popoverOpened}
    >
      <PerformanceScoreContent
        performanceScore={performanceScore}
        onClick={onClick}
        onMouseEnter={openPopover}
        onMouseLeave={closePopover}
        intent={ringIntent}
        title={performanceTitle}
        hovered={popoverOpened}
        {...restProps}
      />
    </PerformanceScorePopover>
  )
}

interface PerformanceScoreContentProps {
  performanceScore: number
  onClick: (event: React.MouseEvent) => void
  onMouseEnter: () => void
  onMouseLeave: () => void
  intent: 'positive' | 'notice' | 'negative'
  title: string
  hovered: boolean
}

const MdPerformanceScoreContent = forwardRef<
  HTMLDivElement,
  PerformanceScoreContentProps
>(
  (
    {
      performanceScore,
      onClick,
      onMouseEnter,
      onMouseLeave,
      intent,
      title,
      hovered,
      ...restProps
    },
    ref
  ) => (
    <Stack
      gap="s16"
      align="center"
      justify="center"
      css={{ width: 'fit-content', marginLeft: 'auto' }}
      onClick={onClick}
      onMouseEnter={onMouseEnter}
      onMouseLeave={onMouseLeave}
      ref={ref}
      {...restProps}
    >
      <RingProgress icon="apartment" intent={intent} value={performanceScore} />
      <PerformanceScoreSubContainer>
        <Score>{performanceScore}%</Score>
        <ScoreSubheading $highlight={hovered}>{title}</ScoreSubheading>
      </PerformanceScoreSubContainer>
    </Stack>
  )
)

const XsPerformanceScoreContent = forwardRef<
  HTMLDivElement,
  PerformanceScoreContentProps
>(
  (
    {
      intent,
      performanceScore,
      onClick,
      onMouseEnter,
      onMouseLeave,
      title,
      hovered,
      ...restProps
    },
    ref
  ) => (
    <Stack
      onClick={onClick}
      onMouseEnter={onMouseEnter}
      onMouseLeave={onMouseLeave}
      gap={0}
      css={{ width: 'fit-content' }}
      ref={ref}
      {...restProps}
    >
      <ScoreSubheading $size="xs" $highlight={hovered}>
        {title}
      </ScoreSubheading>
      <Group>
        <RingProgress intent={intent} value={performanceScore} size="xs" />
        <Score $size="xs">{performanceScore}%</Score>
      </Group>
    </Stack>
  )
)
