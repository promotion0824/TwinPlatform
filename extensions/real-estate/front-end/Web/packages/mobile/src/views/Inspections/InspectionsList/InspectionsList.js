import { useState, useEffect } from 'react'
import { useParams } from 'react-router'
import { useQuery } from 'react-query'
import { useApi, Fetch, Spacing, NotFound } from '@willow/mobile-ui'
import List from 'components/List/List'
import { useLayout } from 'providers'
import Inspection from './Inspection'
import { useInspectionRecords } from '../InspectionChecksList/InspectionRecordsContext'

export default function InspectionsList() {
  const params = useParams()
  const api = useApi()
  const { setTitle, setShowBackButton } = useLayout()

  // The code to get the unsynced inspection record statuses from
  // the store is temporarily disabled. This is because useInspectionRecords
  // crashes if a zustand provider does not exist, and this provider does not exist
  // if the inspectionsOfflineMode feature flag is disabled. Conditionally executing
  // the hook will require a little bit more work.
  // const inspectionRecordsContext = useInspectionRecords()

  const [inspectionStatuses, setInspectionStatuses] = useState({})

  useQuery(
    ['inspectionZones', params.inspectionZoneId],
    () =>
      api.get(
        `/api/sites/${params.siteId}/inspectionZones/${params.inspectionZoneId}`,
        undefined,
        { cache: true }
      ),
    {
      onSuccess: (data) => {
        setTitle(data.name)
      },
    }
  )

  // See comment above about why this is commented out.
  // useEffect(() => {
  //   // On page load, we load inspection statuses from the IndexedDb so if we
  //   // have unsynced inspections, we will show the statuses as they exist
  //   // locally, not possibly out-of-date statuses from the server.
  //   inspectionRecordsContext.getInspectionStatuses().then(setInspectionStatuses)
  // }, [inspectionRecordsContext])

  setShowBackButton(true, `/sites/${params.siteId}/inspectionZones`)

  return (
    <Fetch
      url={`/api/sites/${params.siteId}/inspectionZones/${params.inspectionZoneId}/inspections`}
      cache
    >
      {(inspections) => {
        const sortedInspections = (
          inspections?.sort((a, b) => a.sortOrder - b.sortOrder) ?? []
        ).map((inspection) => ({
          ...inspection,
          // If there's a status in the IndexedDb, use that, otherwise use the
          // status from the server.
          checkRecordSummaryStatus:
            inspectionStatuses[inspection.id] ??
            inspection.checkRecordSummaryStatus,
        }))

        return (
          <Spacing type="content">
            {sortedInspections.length > 0 && (
              <List
                stretchColumn
                activeIndex={-1}
                data={sortedInspections}
                ListItem={Inspection}
              />
            )}
            {sortedInspections.length === 0 && (
              <Spacing>
                <NotFound>No inspections found</NotFound>
              </Spacing>
            )}
          </Spacing>
        )
      }}
    </Fetch>
  )
}
