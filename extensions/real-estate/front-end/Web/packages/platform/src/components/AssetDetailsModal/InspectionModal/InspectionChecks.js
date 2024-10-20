import { Flex, Tabs, Tab, Progress } from '@willow/ui'
import CheckHistory from '../../../views/Command/Inspections/InspectionHistory/CheckHistory/CheckHistory'
import InspectionCheckHistoryDatePicker from '../../../views/Command/Inspections/InspectionHistory/InspectionCheckHistoryDatePicker/InspectionCheckHistoryDatePicker'
import useGetCheckHistory from '../../../hooks/Inspections/useGetCheckHistory'
import styles from '../../../views/Command/Inspections/InspectionHistory/InspectionChecks/InspectionChecks.css'
import { ErrorMessage } from '../../Insights/InsightNode/shared'

export default function InspectionChecks({
  inspection,
  times,
  onTimesChange,
  siteId,
  onTabChange,
  isGraphActive,
  selectedCheckId,
}) {
  const {
    isLoading,
    data: checkRecordsHistory = [],
    isSuccess,
    isError,
  } = useGetCheckHistory(
    {
      siteId,
      startDate: times[0],
      endDate: times[1],
      checkId: selectedCheckId !== 'all' ? selectedCheckId : undefined,
      inspectionId: inspection.id,
    },
    {
      enabled: !!selectedCheckId,
    }
  )

  return (
    <Flex fill="header" padding="small 0">
      <Tabs className={styles.tabs} includeQueryStringForSelectedTab>
        {inspection.checks.map((check) => (
          <Tab
            key={check.id}
            header={check.name}
            onClick={() => {
              onTabChange(check)
            }}
          >
            {isLoading && <Progress />}
            {isError && <ErrorMessage />}
            {isSuccess && (
              <CheckHistory
                inspection={inspection}
                check={check}
                isGraphActive={isGraphActive}
                checkRecordsHistory={checkRecordsHistory}
                times={times}
              />
            )}
          </Tab>
        ))}
        <Flex horizontal align="right" flex={1} padding="0 0 0 medium">
          <InspectionCheckHistoryDatePicker
            times={times}
            onTimesChange={onTimesChange}
          />
        </Flex>
      </Tabs>
    </Flex>
  )
}
