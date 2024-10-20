import { titleCase } from '@willow/common'
import { useScopeSelector } from '@willow/ui'
import { Group, Tooltip, Tracker, TrackerProps } from '@willowinc/ui'
import { forwardRef } from 'react'
import { useTranslation } from 'react-i18next'
import { useHistory } from 'react-router'
import styled from 'styled-components'
import routes from '../../../routes'
import { ArrowIcon, InteractiveTile, MutedIcon } from '../common'

export interface PerformanceTileProps {
  /** Average score over the entire time period. (1-100) */
  averageScore: number
  /**
   * Override the thresholds on the tracker where the colors change.
   * @default { positiveThreshold: 100, noticeThreshold: 75 }
   */
  intentThresholds?: TrackerProps['intentThresholds']
  /** Scores to be displayed on the tracker. */
  performanceScores: Array<{
    /** Date for the performance score. (Jan 01) */
    label: string
    /** Performance score. (1-100) */
    value: number
  }>
}

const Heading = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.default,
}))

const StyledArrowIcon = styled(ArrowIcon)({
  marginLeft: 'auto',
})

const Score = styled.div(({ theme }) => ({
  ...theme.font.display.lg.medium,
  color: theme.color.neutral.fg.default,
}))

const ScoreSuffix = styled(Heading)(({ theme }) => ({
  lineHeight: theme.spacing.s24,
}))

const TrackerDate = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.muted,
}))

export const PerformanceTile = forwardRef<HTMLDivElement, PerformanceTileProps>(
  (
    {
      averageScore,
      intentThresholds = {
        positiveThreshold: 75,
        noticeThreshold: 50,
      },
      performanceScores,
      ...restProps
    },
    ref
  ) => {
    const {
      i18n: { language },
      t,
    } = useTranslation()

    const history = useHistory()
    const { location } = useScopeSelector()

    const goToDashboard = () =>
      history.push(
        `${routes.dashboards_scope__scopeId(
          location?.twin.id
        )}?category=Operational&selectedDashboard=Building+KPI`
      )

    const header = titleCase({
      language,
      text: t('labels.overallBuildingPerformance'),
    })

    return (
      <InteractiveTile
        onClick={goToDashboard}
        ref={ref}
        title={header}
        {...restProps}
      >
        <Group gap="s4" wrap="nowrap">
          <Heading>{header}</Heading>

          <Tooltip
            label={t('plainText.performanceScoreExplanation')}
            multiline
            withinPortal
            w={300}
          >
            <MutedIcon icon="info" />
          </Tooltip>

          <StyledArrowIcon />
        </Group>

        <Group align="flex-end" gap="s4">
          <Score>{averageScore}</Score>
          <ScoreSuffix>%</ScoreSuffix>
        </Group>

        <Tracker data={performanceScores} intentThresholds={intentThresholds} />

        <Group justify="space-between">
          <TrackerDate>{performanceScores[0].label}</TrackerDate>
          <TrackerDate>{performanceScores.at(-1)?.label}</TrackerDate>
        </Group>
      </InteractiveTile>
    )
  }
)
