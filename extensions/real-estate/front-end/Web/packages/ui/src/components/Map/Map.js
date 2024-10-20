import ErrorBoundary from 'components/ErrorBoundary/ErrorBoundary'
import Map from './Map/Map'

export { useMap } from './Map/MapContext'
export { default as Clusters } from './Map/Clusters'
export { default as Marker } from './Map/Marker'

export default function MapComponent({ containerRef, ...rest }) {
  return (
    <ErrorBoundary>
      <Map containerRef={containerRef} {...rest} />
    </ErrorBoundary>
  )
}
