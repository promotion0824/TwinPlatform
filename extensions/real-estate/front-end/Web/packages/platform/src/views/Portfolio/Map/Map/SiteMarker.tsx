import { Site } from '@willow/common/site/site/types'
import { Marker } from '@willow/ui'
import { RingProgress } from '@willowinc/ui'
import { memo } from 'react'
import { usePortfolio } from '../../PortfolioContext'
import MapCard from './MapCard'

/**
 * A marker to be placed on map and is created using mapbox-gl library;
 * when none of the client's site has performance score, display a
 * light green and slightly transparent circle with site icon in middle.
 * otherwise, depends on performance score, display colored and slightly
 * transparent circle with site icon in middle.
 *
 * The component is memoized to skip re-render to avoid pulse animation
 * to trigger too often (e.g. when user zoom in and zoom out on map)
 */
export default memo(SiteMarker)

function SiteMarker({
  feature,
}: {
  feature: {
    properties: Site
    geometry: { coordinates: [number, number] }
  }
}) {
  const {
    buildingScores,
    selectSite,
    selectedSite,
    isBuildingScoresLoading: isLoading,
  } = usePortfolio()

  const { properties: site } = feature
  const isSelected = selectedSite?.id === site.id
  const score = buildingScores.find(
    (buildingScore) => buildingScore?.siteId === site.id
  )?.performance
  const roundedScore = score ? Math.floor(score * 100) : undefined

  return (
    <Marker
      feature={feature}
      isSelected={isSelected}
      popup={
        <MapCard
          performanceScore={roundedScore}
          site={site}
          onClose={() => selectSite(undefined)}
        />
      }
      onClick={() => {
        if (!isSelected) {
          selectSite(site)
        } else {
          selectSite(undefined)
        }
      }}
    >
      {!roundedScore || isLoading ? (
        <RingProgress icon="apartment" value={0} />
      ) : (
        <RingProgress
          icon="apartment"
          intent={
            roundedScore >= 75
              ? 'positive'
              : roundedScore >= 50
              ? 'notice'
              : 'negative'
          }
          value={roundedScore}
        />
      )}
    </Marker>
  )
}
