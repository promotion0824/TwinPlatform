import { ThemeProvider, titleCase } from '@willow/common'
import { Site } from '@willow/common/site/site/types'
import { useFeatureFlag } from '@willow/ui'
import { Group, Icon, RingProgress, Stack } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'
import SummaryCount, { SummaryType } from '../../LocationCard/SummaryCount'
import WeatherBadge from '../../LocationCard/WeatherBadge'

const CardBody = styled.div(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s16,
  padding: theme.spacing.s16,
}))

const CloseButton = styled.div(({ theme }) => ({
  alignItems: 'center',
  backgroundColor: theme.color.core.gray.bg.muted.default,
  borderRadius: theme.radius.round,
  color: theme.color.neutral.fg.highlight,
  cursor: 'pointer',
  display: 'flex',
  height: theme.spacing.s20,
  justifyContent: 'center',
  width: theme.spacing.s20,
}))

const HeaderImage = styled.div<{ $imageUrl: string }>(
  ({ $imageUrl, theme }) => ({
    backgroundImage: `url(${$imageUrl})`,
    backgroundPosition: 'center',
    backgroundSize: 'cover',
    display: 'flex',
    flexDirection: 'row-reverse',
    justifyContent: 'space-between',
    minHeight: '150px',
    padding: theme.spacing.s16,
  })
)

const LocationLocation = styled.div(({ theme }) => ({
  ...theme.font.body.xs.regular,
  color: theme.color.neutral.fg.default,
}))

const LocationName = styled.div(({ theme }) => ({
  ...theme.font.heading.lg,
  color: theme.color.neutral.fg.highlight,
}))

const MapCardContainer = styled.div(({ theme }) => ({
  backgroundColor: theme.color.neutral.bg.accent.default,
  borderRadius: theme.radius.r2,
  boxShadow: theme.shadow.s3,
  display: 'flex',
  flexDirection: 'column',
  width: '260px',
}))

const PerformanceHeading = styled.div(({ theme }) => ({
  ...theme.font.body.xs.regular,
  color: theme.color.neutral.fg.muted,
}))

const PerformanceRow = styled.div(({ theme }) => ({
  alignItems: 'center',
  display: 'flex',
  gap: theme.spacing.s4,
}))

const PerformanceScore = styled.div(({ theme }) => ({
  ...theme.font.body.lg.regular,
  color: theme.color.neutral.fg.highlight,
}))

const PerformanceSection = styled.div({
  display: 'flex',
  flexDirection: 'column',
})

export default function MapCard({
  performanceScore,
  site,
  onClose,
}: {
  performanceScore?: number
  site: Site
  onClose?: () => void
}) {
  const featureFlags = useFeatureFlag()
  const {
    i18n: { language },
    t,
  } = useTranslation()

  const showInsightsByPriority = featureFlags.hasFeatureToggle(
    'locationCardInsightsByPriority'
  )

  return (
    <ThemeProvider>
      <MapCardContainer>
        <HeaderImage
          $imageUrl={site.logoUrl || '/public/map-card-placeholder.png'}
        >
          <CloseButton
            onClick={() => {
              // As this is rendered in a portal inside Mapbox's popup, we need to directly remove
              // this element. This is how Mapbox recommends to remove popups internally.
              document.querySelector('.mapboxgl-popup')?.remove()
              onClose?.()
            }}
          >
            <Icon icon="close" size={16} />
          </CloseButton>

          {site.weather && <WeatherBadge siteWeather={site.weather} />}
        </HeaderImage>

        <CardBody>
          <Stack gap={0}>
            <LocationName>{site.name}</LocationName>
            <LocationLocation>
              {site.suburb}, {site.state}
            </LocationLocation>
          </Stack>

          {performanceScore && (
            <PerformanceSection>
              <PerformanceHeading>
                {titleCase({ language, text: t('labels.performance') })}
              </PerformanceHeading>

              <PerformanceRow>
                <RingProgress
                  intent={
                    performanceScore >= 75
                      ? 'positive'
                      : performanceScore >= 50
                      ? 'notice'
                      : 'negative'
                  }
                  size="xs"
                  value={performanceScore}
                />
                <PerformanceScore>{`${performanceScore}%`}</PerformanceScore>
              </PerformanceRow>
            </PerformanceSection>
          )}

          {showInsightsByPriority ? (
            <Stack gap="s4">
              <SummaryCount
                counts={site.insightsStats}
                siteId={site.id}
                summaryType={SummaryType.insights}
                variant="critical"
              />

              <SummaryCount
                counts={site.insightsStats}
                siteId={site.id}
                summaryType={SummaryType.insights}
                variant={showInsightsByPriority ? 'priority' : 'status'}
              />

              <SummaryCount
                counts={site.ticketStatsByStatus}
                siteId={site.id}
                summaryType={SummaryType.tickets}
              />
            </Stack>
          ) : (
            <Group gap="s8">
              <SummaryCount
                counts={site.insightsStatsByStatus}
                hideCount={!site.insightsStatsByStatus}
                showSummaryName={false}
                siteId={site.id}
                summaryType={SummaryType.insights}
              />
              <SummaryCount
                counts={site.ticketStatsByStatus}
                hideCount={!site.ticketStatsByStatus}
                showSummaryName={false}
                siteId={site.id}
                summaryType={SummaryType.tickets}
              />
            </Group>
          )}
        </CardBody>
      </MapCardContainer>
    </ThemeProvider>
  )
}
