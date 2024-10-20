import { PagedSiteResult, Site } from '@willow/common/site/site/types'
import { Icon } from '@willowinc/ui'
import styled from 'styled-components'
import WeatherBadge from './WeatherBadge'

const BackgroundImage = styled.div<{ $imageUrl: string }>(
  ({ $imageUrl, theme }) => ({
    backgroundColor: theme.color.neutral.bg.panel.default,
    backgroundImage: `url(${$imageUrl})`,
    backgroundPosition: 'center',
    backgroundSize: 'cover',
    borderRadius: theme.radius.r4,
    height: '150px',
    minWidth: '150px',
  })
)

const BuildingType = styled.div(({ theme }) => ({
  ...theme.font.body.xs.regular,
  color: theme.color.neutral.fg.highlight,
  display: 'flex',
  gap: theme.spacing.s4,
}))

const Container = styled.div(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  height: '100%',
  justifyContent: 'space-between',
  padding: theme.spacing.s8,
}))

const Gradient = styled.div(({ theme }) => ({
  background:
    'linear-gradient(180deg, rgba(0, 0, 0, 0.70) 0%, rgba(0, 0, 0, 0.00) 25%, rgba(0, 0, 0, 0.00) 75%, rgba(0, 0, 0, 0.70) 100%)',
  borderRadius: theme.radius.r4,
  height: '100%',
}))

function LocationCardImageOverlay({ site }: { site: Site | PagedSiteResult }) {
  return (
    <Container>
      <BuildingType>
        <Icon icon="apartment" size={16} />
        <div>{site.type}</div>
      </BuildingType>

      {site.weather && <WeatherBadge siteWeather={site.weather} />}
    </Container>
  )
}

export default function LocationCardImage({
  site,
}: {
  site: Site | PagedSiteResult
}) {
  return site.logoUrl ? (
    <BackgroundImage $imageUrl={site.logoUrl}>
      <Gradient>
        <LocationCardImageOverlay site={site} />
      </Gradient>
    </BackgroundImage>
  ) : (
    <BackgroundImage $imageUrl="/public/location-card-placeholder.png">
      <LocationCardImageOverlay site={site} />
    </BackgroundImage>
  )
}
