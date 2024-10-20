import { useParams } from 'react-router'
import { Fetch } from '@willow/ui'
import ZoneContent from './ZoneContent'

export default function Zone() {
  const params = useParams()

  return (
    <Fetch
      name="zone"
      url={`/api/sites/${params.siteId}/inspectionZones/${params.zoneId}/inspections`}
    >
      {(zone) => <ZoneContent zone={zone} />}
    </Fetch>
  )
}
