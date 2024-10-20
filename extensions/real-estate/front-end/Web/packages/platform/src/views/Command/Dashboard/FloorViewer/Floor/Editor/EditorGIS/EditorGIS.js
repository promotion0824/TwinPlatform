import { Flex } from '@willow/ui'
import MapView from '@willow/common/gis/view/MapView'
import { Basemap } from '@willow/common/gis/view/types'

export default function EditorGIS({ site, layers }) {
  return (
    <Flex fill="header" height="100%" position="relative">
      <MapView site={site} layers={layers} basemap={Basemap.dark_gray_vector} />
    </Flex>
  )
}
