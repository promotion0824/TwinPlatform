import { useMap, Marker } from '@willow/ui'
import SiteIcon from '../../SiteIcon/SiteIcon'

export default function GroupMarker({ feature }) {
  const map = useMap()

  function handleClick() {
    map.flyTo({
      center: feature.geometry.coordinates,
      zoom: Math.min(14, Math.floor(Math.max(0, map.getZoom())) + 10),
      speed: 4,
    })
  }

  return (
    <Marker feature={feature} onClick={handleClick}>
      <SiteIcon
        size="medium"
        color="primary"
        value={feature.properties.point_count}
      />
    </Marker>
  )
}
