import { useParams } from 'react-router'
import { Fetch, Flex, Tabs, Tab, api, Message } from '@willow/ui'
import { Loader } from '@willowinc/ui'
import { useQuery } from 'react-query'
import { useTranslation } from 'react-i18next'
import 'twin.macro'
import { qs } from '@willow/common'
import CheckHistory from '../CheckHistory/CheckHistory'
import InspectionCheckHistoryDatePicker from '../InspectionCheckHistoryDatePicker/InspectionCheckHistoryDatePicker'
import ExportCsvButton from '../ExportCsvButton/ExportCsvButton'
import styles from './InspectionChecks.css'
import getInspectionsPath from '../../getInspectionsPath.ts'

/** To create a check history url with zone and site query params
 * from current URL.
 */
export const getCheckHistoryUrl = ({ siteId, inspectionId, checkId }) =>
  qs.createUrl(
    getInspectionsPath(siteId, {
      pageName: 'checks',
      pageItemId: checkId,
      inspectionId,
    }),
    { zone: qs.get('zone'), site: qs.get('site') }
  )

export default function InspectionChecks({
  inspection,
  isGraphActive,
  times,
  onTimesChange,
}) {
  const params = useParams()

  const activeCheck = inspection.checks.find(
    (check) => check.id === params.checkId
  )

  return (
    <Flex fill="header" padding="small 0">
      <Tabs className={styles.tabs} includeQueryStringForSelectedTab>
        {inspection.checks.map((check) => (
          <Tab
            key={check.id}
            header={check.name}
            to={getCheckHistoryUrl({
              siteId: inspection.siteId,
              inspectionId: inspection.id,
              checkId: check.id,
            })}
          >
            <Fetch
              name="inspection-check-history"
              url={`/api/sites/${inspection.siteId}/inspections/${params.inspectionId}/checks/${params.checkId}/history`}
              params={{
                startDate: times[0],
                endDate: times[1],
              }}
            >
              {(checkRecordsHistory) => (
                <CheckHistory
                  inspection={inspection}
                  check={activeCheck}
                  isGraphActive={isGraphActive}
                  checkRecordsHistory={checkRecordsHistory}
                  times={times}
                />
              )}
            </Fetch>
          </Tab>
        ))}
        <Flex horizontal align="right" flex={1} padding="0 0 0 medium">
          <ExportCsvButton
            times={times}
            inspection={inspection}
            check={activeCheck}
          />
          <InspectionCheckHistoryDatePicker
            times={times}
            onTimesChange={onTimesChange}
          />
        </Flex>
      </Tabs>
    </Flex>
  )
}

/** This is a simple separation from InspectionChecks, it will only provide one CheckHistory. */
export function InspectionCheck({ inspection, isGraphActive, times }) {
  const params = useParams()
  const { t } = useTranslation()

  const activeCheck = inspection.checks.find(
    (check) => check.id === params.checkId
  )

  const { isError, isLoading, isSuccess, data } = useQuery(
    ['checkHistory', params.checkId, times[0], times[1]],
    async () => {
      const response = await api.get(
        qs.createUrl(`/inspections/${params.inspectionId}/checks/history`, {
          startDate: times[0],
          endDate: times[1],
          siteId: inspection.siteId,
          checkId: params.checkId !== 'all' ? params.checkId : undefined,
        })
      )
      return response.data
    },
    { enabled: !!params.checkId }
  )

  return isError ? (
    <Message tw="h-full" icon="error">
      {t('plainText.errorOccurred')}
    </Message>
  ) : isLoading ? (
    <div tw="h-full w-full flex items-center">
      <Loader tw="m-0 mr-auto ml-auto" />
    </div>
  ) : isSuccess ? (
    <CheckHistory
      inspection={inspection}
      check={activeCheck}
      isGraphActive={isGraphActive}
      checkRecordsHistory={data}
      times={times}
    />
  ) : (
    <></>
  )
}
