import FeatureLayer from '@arcgis/core/layers/FeatureLayer'
import MapImageLayer from '@arcgis/core/layers/MapImageLayer'
import { Message, Progress } from '@willow/ui'
import { useEffect, useRef } from 'react'
import { useTranslation } from 'react-i18next'
import useEsriAuth, { tokenMissingError } from '../hooks/useEsriAuth'
import styles from './Map.css'
import { Basemap } from './types'
import {
  addWidgetsToView,
  createMapView,
  createWebMap,
} from './utils/gisMapUtils'

const MapView = ({
  site,
  layers,
  basemap,
}: {
  layers?: Array<{
    title: string
    type: string
    id: string
    url: string
  }>
  site: { id: string; webMapId: string }
  basemap: Basemap
}) => {
  const { t } = useTranslation()
  const mapRef = useRef<HTMLDivElement>(null)
  const esriAuthQuery = useEsriAuth(site.id)
  const webMapId = site?.webMapId

  useEffect(() => {
    if (esriAuthQuery.data !== true || webMapId == null) {
      return
    }

    // create a new Map and new MapView instance, then add widgets
    // such as layers list, base map source, and base map gallery to the view
    const map = createWebMap(webMapId, { basemap })
    const view = createMapView(map, mapRef.current)

    // Each ArcGIS map has its own set of layers. If we are given a set of
    // layers, we ignore the map's inbuilt layers and display the layers we are
    // given, otherwise we leave the map as is.
    // Note: this functionality is no longer used, in favour of letting the user
    // set the webMapId of the site. We may want to remove it.
    if (layers != null) {
      view.when(async () => {
        map.layers.forEach((existingLayer) => {
          map.layers.remove(existingLayer)
        })

        for (const layer of layers) {
          const LayerClass =
            layer.type === 'Map Service' ? MapImageLayer : FeatureLayer
          map.layers.add(new LayerClass({ url: layer.url }))
        }
      })
    }

    addWidgetsToView(view, map)

    return () => {
      if (view) {
        view?.destroy()
      }
    }
  }, [basemap, esriAuthQuery.data, webMapId])

  return esriAuthQuery.status === 'error' ? (
    <Message icon="error">
      {esriAuthQuery.error?.message === tokenMissingError
        ? t('plainText.tokenIsMissing')
        : t('plainText.errorOccurred')}
    </Message>
  ) : ['loading', 'idle'].includes(esriAuthQuery.status) ? (
    <Progress />
  ) : (
    <div
      role="application"
      aria-label="gis map"
      className={styles.webmap}
      ref={mapRef}
    />
  )
}

export default MapView
