import mapbox from 'mapbox-gl'
import { useEffect, useRef } from 'react'
import { createPortal } from 'react-dom'
import { useMap } from './MapContext'
import styles from './Marker.css'

export default function Marker({
  feature,
  isSelected,
  popup,
  children,
  onClick,
  closeButtonOnPopup = false,
}) {
  const markerRef = useRef()
  const map = useMap()
  const popupRef = useRef()

  useEffect(() => {
    if (!markerRef.current) {
      const el = document.createElement('div')
      el.innerHTML = ''
      el.addEventListener('click', onClick)

      markerRef.current = new mapbox.Marker(el)
        .setLngLat(feature.geometry.coordinates)
        .addTo(map)
    }

    return () => {
      markerRef.current.remove()
      markerRef.current = null
    }
    // Including any dependencies below will cause the marker to be instantly removed.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  useEffect(() => {
    if (isSelected && !popupRef.current && popup) {
      popupRef.current = new mapbox.Popup({
        anchor: 'top',
        closeButton: closeButtonOnPopup,
        offset: 30,
        className: styles.popup,
      })
        .setHTML('')
        .setLngLat(feature.geometry.coordinates)
        .addTo(map)
    }
  }, [closeButtonOnPopup, feature.geometry.coordinates, isSelected, map, popup])

  const markerElement = markerRef.current?.getElement()
  const popupContentElement = popupRef.current
    ?.getElement()
    ?.querySelector('.mapboxgl-popup-content')

  return (
    <>
      {markerElement ? createPortal(children, markerElement) : null}
      {popupContentElement ? createPortal(popup, popupContentElement) : null}
    </>
  )
}
