import { isEqual } from 'lodash'
import { useEffect, useState } from 'react'
import { ErrorBoundary, Panel } from '@willow/ui'
import KpiMap from './Map/KpiMap'
import { usePortfolio } from '../PortfolioContext'

export default function MapComponent({ containerRef }) {
  const {
    baseMapSites,
    filteredSites,
    buildingScores,
    filters,
    selectedSite,
    setShouldFilterByMap,
    setShouldResetMapBounds,
    setSiteIdsOnMap,
    shouldFilterByMap,
    shouldResetMapBounds,
    handleResetMapClick,
  } = usePortfolio()

  const [sites, setSites] = useState([])

  useEffect(() => {
    const newSites = (shouldFilterByMap ? baseMapSites : filteredSites).filter(
      (site) => site.location
    )

    if (!isEqual(sites, newSites)) setSites(newSites)
  }, [baseMapSites, filteredSites, shouldFilterByMap, sites])

  return (
    <Panel $borderWidth="0" fill="header hidden">
      <ErrorBoundary>
        <KpiMap
          sites={sites}
          containerRef={containerRef}
          buildingScores={buildingScores}
          filters={filters}
          selectedSite={selectedSite}
          setShouldFilterByMap={setShouldFilterByMap}
          setShouldResetMapBounds={setShouldResetMapBounds}
          setSiteIdsOnMap={setSiteIdsOnMap}
          shouldFilterByMap={shouldFilterByMap}
          shouldResetMapBounds={shouldResetMapBounds}
          onResetMapClick={handleResetMapClick}
        />
      </ErrorBoundary>
    </Panel>
  )
}
