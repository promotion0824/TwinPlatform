import { useEffect, useRef } from 'react'
import ReactDOMServer from 'react-dom/server'
import cx from 'classnames'
import mapbox from 'mapbox-gl'
import { Icon } from '@willow/ui'
import { usePortfolio } from '../../PortfolioContext'
import { useMap } from './MapContext'
import Site from '../../Site/Site'
import styles from './Marker.css'

export default function Marker({ site, showStatus }) {
  const map = useMap()
  const portfolio = usePortfolio()

  const markerRef = useRef()

  const isSelected = site.id === portfolio.selectedSite?.id

  useEffect(() => {
    const cxClassName = cx(styles.marker, {
      [styles.hasError]: site.errors > 0,
      [styles.selected]: isSelected,
    })

    function handleClick() {
      portfolio.selectSite(site)
    }

    const el = document.createElement('div')
    el.innerHTML = ReactDOMServer.renderToString(
      <div className={cxClassName}>
        <Icon icon="site" />
      </div>
    )
    el.addEventListener('click', handleClick)

    const popup = new mapbox.Popup({
      closeButton: false,
      offset: 30,
    }).setHTML(
      ReactDOMServer.renderToString(
        <Site site={site} showFavorite={false} showStatus={showStatus} />
      )
    )

    markerRef.current = new mapbox.Marker(el)
      .setLngLat(site.location)
      .setPopup(popup)
      .addTo(map)

    return () => {
      el.removeEventListener('click', handleClick)
      markerRef.current.remove()
    }
  }, [portfolio.selectedSite?.id])

  useEffect(() => {
    if (isSelected) {
      markerRef.current?.togglePopup()
    }
  }, [])

  return null
}
