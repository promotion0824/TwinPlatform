import MapView from '@arcgis/core/views/MapView'
import WebMap from '@arcgis/core/WebMap'
import LayerList from '@arcgis/core/widgets/LayerList'
import BasemapGallery from '@arcgis/core/widgets/BasemapGallery'
import PortalBasemapsSource from '@arcgis/core/widgets/BasemapGallery/support/PortalBasemapsSource'
import Config from '@arcgis/core/config'

const setWidgetStyles = (widget, styles) => {
  if (widget && widget.domNode && widget.domNode.style) {
    Object.keys(styles).forEach((e) => {
      if (widget.domNode.style) {
        // eslint-disable-next-line no-param-reassign
        widget.domNode.style[`${e}`] = styles[e]
      }
    })
  }
}

export const createWebMap = (id, mapProperties) =>
  new WebMap({
    portalItem: {
      id,
    },
    ...mapProperties,
  })

export const createMapView = (map, ref) =>
  new MapView({
    container: ref,
    map,
  })

/**
 * add layer lists widget, base map sources widget, and
 * base map gallery to the MapView
 */
export const addWidgetsToView = (view, map) => {
  view.when(() => {
    const layerList = new LayerList({
      view,
      id: 'layer-list-widget',
      listItemCreatedFunction: (event) => {
        const { item } = event
        if (item.layer.type !== 'group') {
          item.panel = {
            content: 'legend',
            open: false,
          }
        }
      },
    })

    // Add widget to the top right corner of the view
    view.ui.add(layerList, 'top-right')

    setWidgetStyles(layerList, {
      width: '300px',
      visibility: 'visible',
      borderRadius: '5px',
      padding: '1px',
    })

    const basemapsSource = new PortalBasemapsSource({
      portal: {
        url: Config.portalUrl,
      },
      updateBasemapsCallback: (basemaps) => {
        const savedBasemapTitle = 'dark-gray'
        if (savedBasemapTitle) {
          const savedBasemap = basemaps.find(
            (bm) => bm.portalItem.title === savedBasemapTitle
          )
          if (savedBasemap) {
            map.basemap = savedBasemap
          }
        }
        return basemaps
      },
    })

    const basemap = new BasemapGallery({
      id: 'basemap-gallery-widget',
      view,
      source: basemapsSource,
    })

    view.ui.add(basemap, { position: 'bottom-left' })
  })
}
