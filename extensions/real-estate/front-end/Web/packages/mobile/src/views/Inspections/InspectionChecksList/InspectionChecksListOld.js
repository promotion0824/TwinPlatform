import { useState } from 'react'
import { useHistory, useParams } from 'react-router'
import { useApi, Fetch, Spacing, NotFound } from '@willow/mobile-ui'
import List from 'components/List/List'
import { useLayout } from 'providers'
import InspectionCheck from './InspectionCheck'

// eslint-disable-next-line complexity
export default function InspectionChecksListOld() {
  // eslint-disable-line
  const api = useApi()
  const history = useHistory()
  const params = useParams()
  const { setTitle, setShowBackButton } = useLayout()

  const [checksState, setChecksState] = useState({})
  const [activeCheckId, setActiveCheckId] = useState()

  const { inspection = {}, checkRecords = [] } = checksState

  setTitle(inspection.name)
  setShowBackButton(
    true,
    `/sites/${params.siteId}/inspectionZones/${params.inspectionZoneId}/inspections`
  )

  const handleResponse = (response) => {
    const nextActiveCheckId = response.inspection.checks
      .sort((a, b) => a.sortOrder - b.sortOrder)
      .map((check) =>
        response.checkRecords.find(
          (checkRecord) => checkRecord.checkId === check.id
        )
      )
      .filter((checkRecord) => checkRecord != null)
      .find(
        (checkRecord) =>
          checkRecord.status !== 'completed' &&
          checkRecord.status !== 'notRequired'
      )?.checkId

    setActiveCheckId(nextActiveCheckId)
    setChecksState(response)
  }

  // Note: shouldRedirectIfCompleted is always true
  const updateCheckRecords = (shouldRedirectIfCompleted) => {
    api
      .get(
        `/api/sites/${params.siteId}/inspections/${params.inspectionId}/lastRecord`
      )
      .then((response) => {
        setChecksState(response)
        if (shouldRedirectIfCompleted) {
          const isCompleted = response.inspection.checks
            .map((check) =>
              response.checkRecords.find(
                (checkRecord) => checkRecord.checkId === check.id
              )
            )
            .filter((checkRecord) => checkRecord != null)
            .every(
              (checkRecord) =>
                checkRecord.status === 'completed' ||
                checkRecord.status === 'notRequired'
            )

          if (isCompleted) {
            history.push(
              `/sites/${params.siteId}/inspectionZones/${params.inspectionZoneId}/inspections`
            )
          } else {
            const nextActiveCheckId = response.inspection.checks
              .map((check) =>
                response.checkRecords.find(
                  (checkRecord) => checkRecord.checkId === check.id
                )
              )
              .filter((checkRecord) => checkRecord != null)
              .find(
                (checkRecord) =>
                  checkRecord.status !== 'completed' &&
                  checkRecord.status !== 'notRequired'
              )?.checkId

            setActiveCheckId(nextActiveCheckId)
          }
        }
      })
  }

  const sortedChecks =
    inspection.checks?.sort((a, b) => a.sortOrder - b.sortOrder) || []

  return (
    <Fetch
      url={`/api/sites/${params.siteId}/inspections/${params.inspectionId}/lastRecord`}
      cache
      onResponse={handleResponse}
    >
      {() => (
        <Spacing type="content">
          {inspection.checks?.length > 0 && (
            <List
              stretchColumn
              activeIndex={-1}
              data={sortedChecks}
              ListItem={InspectionCheck}
              listItemProps={{
                inspection,
                checkRecords,
                activeCheckId,
                onSetActiveCheckId: setActiveCheckId,
                updateCheckRecords,
              }}
            />
          )}
          {(inspection.checks || [])?.length === 0 && (
            <Spacing>
              <NotFound>No inspection checks found</NotFound>
            </Spacing>
          )}
        </Spacing>
      )}
    </Fetch>
  )
}
