import { useEffect } from 'react'
import { Progress, useAnalytics } from '@willow/ui'
import { GroupedSensors } from '../../../../components/GroupedSensors'
import MissingPoint from '../../../../components/GroupedSensors/MissingPoints'
import TabContent from '../shared/TabContent'
import useTwinAnalytics from '../useTwinAnalytics'
import useGroupedSensors from './useGroupedSensors'
import useLiveDataPoints from '../hooks/useGetLiveDataPoints'
import { useSites } from '../../../../providers'

const SensorsTab = ({
  twin,
  count = 0,
  selectTimeSeriesTab,
  missingSensors,
}) => {
  const analytics = useAnalytics()
  const twinAnalytics = useTwinAnalytics()
  const { data: groups, isLoading } = useGroupedSensors(
    twin?.siteID,
    twin?.uniqueID
  )
  const { data: liveDataPoints, isLoading: isLoadingLiveData } =
    useLiveDataPoints(twin?.siteID, twin?.uniqueID)
  const sites = useSites()
  const site = sites.find((nextSite) => nextSite.id === twin?.siteID)

  const handleTogglePoint = (point: { name: string }, isSelected: boolean) => {
    analytics.track(
      isSelected
        ? 'Point Selected Sensors tab'
        : 'Point Deselected Sensors tab',
      {
        name: point.name,
        site: site?.name,
      }
    )
    if (isSelected) {
      selectTimeSeriesTab()
    }
  }

  useEffect(() => {
    if (twin) {
      twinAnalytics.trackSensorsViewed({ twin, count })
    }
  }, [twin, twinAnalytics, count])

  return (
    <TabContent>
      {isLoading ? (
        <Progress />
      ) : (
        <>
          {count > 0 &&
            groups != null &&
            Object.keys(groups)
              // Sort by groups key, with key containing hostedBy id first,
              // followed by key containing only connector id.
              // key "_" - no connector - will always be last in the list.
              .sort((keyA, keyB) => {
                if (
                  keyA === '_' ||
                  (keyA.startsWith('_') && !keyB.startsWith('_'))
                ) {
                  return 1
                } else if (
                  keyB === '_' ||
                  (!keyA.startsWith('_') && keyB.startsWith('_'))
                ) {
                  return -1
                }
                return keyA < keyB ? -1 : 1
              })
              .map((key) => {
                const points = groups[key]
                const [, connectorId] = key.split('_')
                return (
                  <GroupedSensors
                    key={key}
                    hostedBy={points[0].device}
                    connector={
                      connectorId
                        ? {
                            id: connectorId,
                            name: points[0].connectorName,
                          }
                        : undefined
                    }
                    points={points}
                    isLoadingLiveData={isLoadingLiveData}
                    liveDataPoints={liveDataPoints}
                    onTogglePoint={handleTogglePoint}
                  />
                )
              })}
          {missingSensors.map((item) => (
            <MissingPoint key={item} name={item} />
          ))}
        </>
      )}
    </TabContent>
  )
}

export default SensorsTab
