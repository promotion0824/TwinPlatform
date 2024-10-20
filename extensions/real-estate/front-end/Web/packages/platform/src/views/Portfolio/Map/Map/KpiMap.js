import _ from 'lodash'
import { useEffect, useMemo, useState, Fragment } from 'react'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import { v4 as uuidv4 } from 'uuid'
import { Map, Clusters } from '@willow/ui'
import { Button } from '@willowinc/ui'

import getBounds from './getBounds'
import GroupMarker from './GroupMarker'
import SiteMarker from './SiteMarker'

const ResetButton = styled(Button)(({ theme }) => ({
  marginRight: theme.spacing.s8,
  marginTop: theme.spacing.s8,
  position: 'absolute',
  right: 0,
  zIndex: theme.zIndex.overlay,
}))

export default function KpiMap({
  sites,
  showResetButton = true,
  className = undefined,
  containerRef,
  buildingScores,
  filters,
  selectedSite,
  setShouldFilterByMap,
  setShouldResetMapBounds,
  onResetMapClick,
  setSiteIdsOnMap,
  shouldFilterByMap,
  shouldResetMapBounds,
}) {
  const { t } = useTranslation()

  const sitesIds = JSON.stringify(sites.map((site) => site.id))

  const [bounds, setBounds] = useState(() => getBounds(sites))

  function resetMapBounds() {
    setBounds(getBounds(sites))
  }

  useEffect(() => {
    if (shouldResetMapBounds) {
      setShouldResetMapBounds(false)
      resetMapBounds()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [shouldResetMapBounds])

  useEffect(() => {
    resetMapBounds()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [sitesIds])

  useEffect(() => {
    if (selectedSite?.location != null) {
      setBounds([selectedSite.location])
    }
  }, [selectedSite?.location])

  const clustersFeatures = sites.map((site) => ({
    type: 'Feature',
    properties: site,
    geometry: {
      type: 'Point',
      coordinates: [...site.location],
    },
    performance: buildingScores?.find((score) => score.siteId === site.id)
      ?.score,
  }))

  // This component is memoized to prevent the map from re-rendering when it's scrolled
  // and sites/clusters are moved off the map. Once it's in scrolling mode, it should only need
  // to re-render if sites (or sitesIds) changes, which is what clustersFeatures is built from.
  const clusters = useMemo(
    () => (
      <Clusters
        key={filters?.selectedMetric || uuidv4()}
        features={clustersFeatures}
        onResize={(mapFilteredSiteIds) => {
          setSiteIdsOnMap(mapFilteredSiteIds)
          if (!shouldFilterByMap) setShouldFilterByMap(true)
        }}
      >
        {(features) => {
          const orderedFeatures = _.orderBy(
            features,
            (feature) => selectedSite?.id === feature.properties.id
          )

          return orderedFeatures.map((feature) => (
            <Fragment
              key={
                feature.properties.cluster_id != null
                  ? `cluster_id_${feature.properties.cluster_id}`
                  : feature.properties.id
              }
            >
              {feature.properties.cluster ? (
                <GroupMarker feature={feature} />
              ) : (
                <SiteMarker feature={feature} />
              )}
            </Fragment>
          ))
        }}
      </Clusters>
    ),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [
      filters?.selectedMetric,
      selectedSite?.id,
      setShouldFilterByMap,
      shouldFilterByMap,
      sitesIds,
    ]
  )

  return (
    <div className={className}>
      {showResetButton && (
        <ResetButton kind="secondary" onClick={onResetMapClick}>
          {t('plainText.reset')}
        </ResetButton>
      )}
      <Map bounds={bounds} containerRef={containerRef}>
        {clusters}
      </Map>
    </div>
  )
}
