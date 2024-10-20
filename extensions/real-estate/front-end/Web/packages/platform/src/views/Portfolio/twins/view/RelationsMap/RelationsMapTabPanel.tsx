import { useEffect } from 'react'
import { useHistory, useParams } from 'react-router'
import { Ontology } from '@willow/common/twins/view/models'
import { ModelOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import { Progress } from '@willow/ui'

import useRelationsMap from './hooks/useRelationsMap'
import routes from '../../../../../routes'
import RelationsMap from './RelationsMap'
import { TwinWithIds } from './types'
import useTwinAnalytics from '../../useTwinAnalytics'

export default function RelationsMapTabPanel({
  initialTwin,
  modelsOfInterest,
  ontology,
}: {
  initialTwin: TwinWithIds
  modelsOfInterest: ModelOfInterest[]
  ontology: Ontology
}) {
  const history = useHistory()
  const analytics = useTwinAnalytics()
  const { siteId } = useParams<{ siteId?: string }>()
  /**
   * Multi-tenant API calls to query about relatedTwins still requires a siteId,
   * so siteId will be defined in the URL; if siteId is null, we are in a single tenant environment.
   */
  const isSingleTenant = siteId == null

  const {
    graph,
    direction,
    graphState,
    selectedTwinId,
    isTwinOverlayVisible,
    closeTwinOverlay,
    setSelectedTwinId,
    toggleTwinExpansion,
    toggleModelExpansion,
    isTwinExpanded,
  } = useRelationsMap(initialTwin, ontology, isSingleTenant)

  useEffect(() => {
    analytics.trackRelationsMapViewed(initialTwin)
  }, [analytics, initialTwin.id])

  return graph != null ? (
    <RelationsMap
      graph={graph}
      direction={direction}
      graphState={graphState}
      ontology={ontology}
      selectedTwinId={selectedTwinId}
      isTwinOverlayVisible={isTwinOverlayVisible}
      onTwinClick={(twin) => {
        setSelectedTwinId(twin.id)
      }}
      onToggleNodeExpansionClick={(twin, expandDirection) => {
        if (isTwinExpanded(twin.id, expandDirection)) {
          analytics.trackShrinkRelationsMap()
        } else {
          analytics.trackGrowRelationsMap()
        }

        toggleTwinExpansion(twin.id, expandDirection)
      }}
      onModelClick={toggleModelExpansion}
      onGoToTwinClick={(twinId) => {
        if (twinId !== initialTwin.id) {
          analytics.trackTwinViewedViaRelationsMap()
          history.push(
            `${routes.portfolio_twins_view__siteId__twinId(
              initialTwin.siteID,
              twinId
            )}?rightTab=relationsMap`
          )
        }
      }}
      onCloseTwinOverlay={closeTwinOverlay}
      modelsOfInterest={modelsOfInterest}
    />
  ) : (
    <Progress />
  )
}
