import { debounce } from 'lodash'
import mapbox from 'mapbox-gl'
import 'mapbox-gl/dist/mapbox-gl.css'
import { useEffect, useRef, useState } from 'react'
import styles from './Map.css'
import { MapContext } from './MapContext'

export default function Map({
  bounds,
  children,
  onUpdate,
  containerRef = null,
}) {
  const mapRef = useRef()
  const mapElementRef = useRef()
  const isMountedRef = useRef(true)

  const [hasLoaded, setHasLoaded] = useState(false)
  const [markerHeight, setMarkerHeight] = useState(0)
  const [popupHeight, setPopupHeight] = useState(0)

  // These need to be calculated and stored in state as their height is 0 when they are
  // first checked, as they haven't been rendered yet.
  const marker = containerRef.current?.querySelector('.mapboxgl-marker')
  const popup = containerRef.current?.querySelector('.mapboxgl-popup-content')

  if (markerHeight === 0 && marker && marker.offsetHeight !== markerHeight) {
    setMarkerHeight(marker.offsetHeight)
  }

  useEffect(() => {
    if (!popup) {
      setPopupHeight(0)
    } else if (popupHeight === 0 && popup.offsetHeight !== popupHeight) {
      setPopupHeight(popup.offsetHeight)
    }
  }, [popup, popupHeight])

  useEffect(() => {
    if (bounds != null && mapRef.current != null) {
      const markerAndPopupHeight = markerHeight + popupHeight
      const yOffset =
        containerRef.current?.offsetHeight > markerAndPopupHeight
          ? -(markerAndPopupHeight / 2)
          : 0

      if (bounds?.length === 1) {
        mapRef.current.flyTo({
          center: bounds[0],
          offset: [0, yOffset],
          zoom: Math.max(14, mapRef.current.getZoom()),
          speed: 4,
        })
      }
      if (bounds?.length > 1) {
        mapRef.current.fitBounds(new mapbox.LngLatBounds(...bounds), {
          offset: [0, yOffset],
          padding: 100,
          speed: 4,
        })
      }
    }
  }, [bounds, containerRef, markerHeight, popupHeight])

  useEffect(() => {
    mapbox.accessToken =
      'pk.eyJ1IjoicmFkYW0iLCJhIjoiY2tlOWQ0OG1kMDJ6NjJ0czl5NGNsZnJvdiJ9.c1vnmJzq_NxZBH3PWYY4sg'

    const map = new mapbox.Map({
      container: mapElementRef.current,
      style: 'mapbox://styles/mapbox/dark-v10',
      fitBoundsOptions: {
        padding: 100,
      },
      ...(bounds == null && {
        zoom: 0.1,
      }),
      ...(bounds?.length === 1 && {
        center: bounds[0],
        zoom: 14,
      }),
      ...(bounds?.length > 1 && {
        bounds,
      }),
    })

    map.on('load', () => {
      if (isMountedRef.current) {
        setHasLoaded(true)
        // Add a slight delay to make sure the debounce is avoided
        setTimeout(() => map.resize(), 200)
      }
    })

    map.on('move', (e) => onUpdate?.(e))
    map.on('moveend', (e) => onUpdate?.(e))

    // eslint-disable-next-line no-underscore-dangle
    map._canvas.setAttribute('role', 'application') // Default role for canvas is "region" which is invalid

    mapRef.current = map

    return () => {
      isMountedRef.current = false
    }
  }, [])

  // eslint-disable-next-line consistent-return
  useEffect(() => {
    if (containerRef && containerRef.current) {
      // Add a debounce when resizing the panel container to
      // avoid too many map resizes which has a flashing visual effect
      const handleResize = debounce(() => {
        mapRef.current.resize()
      }, 100)

      const resizeObserver = new ResizeObserver(() => {
        handleResize()
      })

      // map will only listen to browser window resizing but not container resizing
      resizeObserver.observe(containerRef.current)

      return () => {
        resizeObserver.disconnect()
        handleResize.cancel() // Cancel any pending debounced calls
      }
    }
  }, [containerRef])

  return (
    <MapContext.Provider value={mapRef.current}>
      <div ref={mapElementRef} className={styles.map} />
      {hasLoaded && children}
    </MapContext.Provider>
  )
}
