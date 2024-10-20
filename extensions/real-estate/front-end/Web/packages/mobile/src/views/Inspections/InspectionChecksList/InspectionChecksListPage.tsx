import { useParams } from 'react-router'
import InspectionChecksList from './InspectionChecksList'

export default function InspectionChecksListPage() {
  const params = useParams<{
    siteId?: string
    inspectionId?: string
    inspectionZoneId?: string
  }>()

  if (
    params.siteId == null ||
    params.inspectionId == null ||
    params.inspectionZoneId == null
  ) {
    throw new Error(
      'InspectionChecksListPage requires siteId, inspectionId and inspectionZoneId query params'
    )
  }

  return (
    <InspectionChecksList
      siteId={params.siteId}
      inspectionId={params.inspectionId}
      inspectionZoneId={params.inspectionZoneId}
    />
  )
}
