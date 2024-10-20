import { useEffect, useRef, useState } from 'react'
import Supercluster from 'supercluster'

import { useMap } from './MapContext'

export default function Clusters({
  features,
  radius = 40,
  maxZoom = 11,
  children,
  /**
   * Called afer the map is resized.
   * The list of siteIds contained in the resized map is provided to the function. */
  onResize,
}) {
  const mapContext = useMap()

  const superclusterRef = useRef()

  const [state, setState] = useState({
    features: undefined,
  })

  function update() {
    const bounds = mapContext.getBounds().toArray().flat()
    const zoom = Math.floor(mapContext.getZoom())

    const nextFeatures = superclusterRef.current.getClusters(bounds, zoom)
    setState({ features: nextFeatures })
  }

  useEffect(() => {
    superclusterRef.current = new Supercluster({
      radius,
      maxZoom,
    })
    superclusterRef.current.load(features)

    mapContext.on('move', update)
    mapContext.on('moveend', update)
    update()

    return () => {
      mapContext.off('move', update)
      mapContext.off('moveend', update)
    }
  }, [])

  useEffect(() => {
    mapContext.resize()
    superclusterRef.current.load(features)
  }, [JSON.stringify(features)])

  useEffect(() => {
    if (!state.features || !onResize) return

    onResize(
      state.features
        .map((feature) => {
          if (feature.properties.cluster) {
            return superclusterRef.current
              .getLeaves(feature.properties.cluster_id, Infinity)
              .map((leaf) => leaf.properties.id)
          }

          return feature.properties.id
        })
        .flat()
    )
  }, [JSON.stringify(state.features)])

  if (state.features == null || features.length === 0) {
    return null
  }

  return children(state.features)
}
