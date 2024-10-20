import { PagedSiteResult, Site, titleCase } from '@willow/common'
import {
  getContainmentHelper,
  useFeatureFlag,
  useScopeSelector,
} from '@willow/ui'
import { Badge, Stack } from '@willowinc/ui'
import { MouseEvent, forwardRef, useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { useHistory } from 'react-router'
import styled from 'styled-components'
import routes from '../../../routes'
import LocationCardImage from './LocationCardImage'
import PerformanceScore from './PerformanceScore'
import SummaryCount, { SummaryType } from './SummaryCount'

const { ContainmentWrapper, getContainerQuery } = getContainmentHelper()

const BREAKPOINTS_LARGE = 'width >= 540px'
const BREAKPOINTS_NOT_LARGE = 'width < 540px'
const BREAKPOINTS_SMALL = 'width <= 400px'

const Card = styled.div<{ $isSelected: boolean }>(({ $isSelected, theme }) => ({
  background: $isSelected
    ? theme.color.neutral.bg.accent.activated
    : theme.color.neutral.bg.accent.default,
  border: `1px solid ${theme.color.neutral.border.default}`,
  borderRadius: theme.radius.r2,
  boxShadow: theme.shadow.s2,
  cursor: 'pointer',
  display: 'flex',
  gap: theme.spacing.s16,
  overflow: 'hidden',
  padding: theme.spacing.s16,
  flexDirection: 'row',

  '&:hover': {
    background: theme.color.neutral.bg.accent.hovered,
  },

  [getContainerQuery(BREAKPOINTS_SMALL)]: {
    flexDirection: 'column',
  },
}))

const CardBody = styled.div(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s12,
}))

const LocationLocation = styled.div(({ theme }) => ({
  ...theme.font.body.xs.regular,
  color: theme.color.neutral.fg.default,
}))

const LocationName = styled.div(({ theme }) => ({
  ...theme.font.heading.lg,
  color: theme.color.neutral.fg.highlight,
}))

interface LocationCardProps {
  isSelected?: boolean
  scores?: {
    comfort: number
    energy: number
    performance: number
  }
  site: Site | PagedSiteResult
}

type PerformanceScoreProps = {
  comfortScore: number
  energyScore: number
  performanceScore: number
  onClick: (event: MouseEvent<HTMLButtonElement>) => void
}

export default forwardRef<HTMLDivElement, LocationCardProps>(
  ({ isSelected = false, scores, site, ...restProps }, ref) => {
    const featureFlags = useFeatureFlag()
    const history = useHistory()
    const { isScopeSelectorEnabled, scopeLookup } = useScopeSelector()
    const {
      i18n: { language },
      t,
    } = useTranslation()

    const showInsightsByPriority = featureFlags.hasFeatureToggle(
      'locationCardInsightsByPriority'
    )

    const performanceScoreProps = useMemo(
      () =>
        scores
          ? {
              comfortScore: scores.comfort,
              energyScore: scores.energy,
              performanceScore: scores.performance,
              onClick: (event: MouseEvent<HTMLButtonElement>) => {
                event.stopPropagation()

                const basePath = isScopeSelectorEnabled
                  ? routes.dashboards_scope__scopeId(
                      scopeLookup[site.id].twin.id
                    )
                  : routes.dashboards_sites__siteId(site.id)

                history.push(
                  `${basePath}?category=Operational&selectedDashboard=Building+KPI`
                )
              },
            }
          : {},
      [scores, site.id, isScopeSelectorEnabled, scopeLookup, history]
    )

    return (
      <Card
        $isSelected={isSelected}
        onClick={() => {
          if (isScopeSelectorEnabled) {
            const scopeId = scopeLookup[site.id].twin.id
            history.push(routes.home_scope__scopeId(scopeId))
          } else {
            history.push(routes.sites__siteId(site.id))
          }
        }}
        ref={ref}
        {...restProps}
      >
        <LocationCardImage site={site} />

        <CardBody>
          <div>
            <LocationName>{site.name}</LocationName>
            <LocationLocation>
              {site.suburb}, {site.state}
            </LocationLocation>
          </div>

          {site.status && site.status !== 'Operations' && (
            <Badge variant="dot">
              {titleCase({
                language,
                text: t(`plainText.${site.status.toLowerCase()}`),
              })}
            </Badge>
          )}

          <Stack gap={0} justify="end" css={{ flexGrow: 1 }}>
            {scores && scores.performance >= 0 && (
              <PerformanceScore
                {...(performanceScoreProps as PerformanceScoreProps)}
                size="xs"
                css={({ theme }) => ({
                  marginBottom: theme.spacing.s4,
                  [getContainerQuery(BREAKPOINTS_LARGE)]: {
                    display: 'none',
                  },
                })}
              />
            )}

            <Stack gap="s4" css={{ marginTop: 'auto' }}>
              {showInsightsByPriority && (
                <SummaryCount
                  counts={site.insightsStats}
                  siteId={site.id}
                  summaryType={SummaryType.insights}
                  variant="critical"
                />
              )}

              <SummaryCount
                counts={
                  showInsightsByPriority
                    ? site.insightsStats
                    : site.insightsStatsByStatus
                }
                hideCount={
                  !showInsightsByPriority && !site.insightsStatsByStatus
                }
                siteId={site.id}
                summaryType={SummaryType.insights}
                variant={showInsightsByPriority ? 'priority' : 'status'}
              />

              <SummaryCount
                counts={site.ticketStatsByStatus}
                hideCount={!showInsightsByPriority && !site.ticketStatsByStatus}
                siteId={site.id}
                summaryType={SummaryType.tickets}
              />
            </Stack>
          </Stack>
        </CardBody>

        {scores && scores.performance >= 0 && (
          <PerformanceScore
            {...(performanceScoreProps as PerformanceScoreProps)}
            css={{
              [getContainerQuery(BREAKPOINTS_NOT_LARGE)]: {
                display: 'none',
              },
            }}
          />
        )}
      </Card>
    )
  }
)

export { ContainmentWrapper }
