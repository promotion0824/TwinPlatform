import { Icon, IconName, Popover } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'

const Heading = styled.div(({ theme }) => ({
  ...theme.font.body.xs.regular,
  color: theme.color.neutral.fg.muted,
}))

const IntentColoredIcon = styled(Icon)<{ $score: number }>(
  ({ $score, theme }) => ({
    color:
      $score >= 75
        ? theme.color.intent.positive.fg.default
        : $score >= 50
        ? theme.color.intent.notice.fg.default
        : theme.color.intent.negative.fg.default,
  })
)

const PopoverBody = styled.div(({ theme }) => ({
  ...theme.font.body.xs.regular,
  color: theme.color.neutral.fg.default,
}))

const PopoverContainer = styled.div(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s8,
  padding: theme.spacing.s8,
  width: '216px',
}))

const PopoverHeader = styled.div(({ theme }) => ({
  display: 'flex',
  gap: theme.spacing.s16,
  justifyContent: 'space-around',
}))

const Score = styled.div(({ theme }) => ({
  ...theme.font.body.lg.regular,
  color: theme.color.neutral.fg.highlight,
}))

const ScoreRow = styled.div(({ theme }) => ({
  display: 'flex',
  gap: theme.spacing.s2,
}))

const ScoreSection = styled.div({
  alignItems: 'center',
  flexDirection: 'column',
  display: 'flex',
})

const Separator = styled.hr(({ theme }) => ({
  backgroundColor: theme.color.neutral.border.default,
}))

export default function PerformanceScorePopover({
  children,
  comfortScore,
  energyScore,
  opened,
}: {
  children: React.ReactNode
  comfortScore: number
  energyScore: number
  opened: boolean
}) {
  const { t } = useTranslation()

  return (
    <Popover opened={opened} position="top" withArrow>
      <Popover.Target>{children}</Popover.Target>
      <Popover.Dropdown>
        <PopoverContainer>
          <PopoverHeader>
            {[
              ['reports.energy', 'eco', energyScore],
              ['reports.comfort', 'ac_unit', comfortScore],
            ].map(([label, icon, score]: [string, IconName, number]) => (
              <ScoreSection key={label}>
                <Heading>{t(label)}</Heading>
                <ScoreRow>
                  {typeof score === 'number' ? (
                    <>
                      <IntentColoredIcon icon={icon} $score={score} />
                      <Score>{score}%</Score>
                    </>
                  ) : (
                    <Score>--</Score>
                  )}
                </ScoreRow>
              </ScoreSection>
            ))}
          </PopoverHeader>

          <Separator />

          <PopoverBody>
            {t('plainText.performanceScoreExplanation')}
          </PopoverBody>
        </PopoverContainer>
      </Popover.Dropdown>
    </Popover>
  )
}
