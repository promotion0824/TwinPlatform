import { useEffect } from 'react'
import cx from 'classnames'
import { useAnalytics, Button, Flex, Icon, Text } from '@willow/ui'
import { PointType } from '@willow/common/insights/insights/types'
import styles from './Point.css'
import useSensorPoint from '../hooks/useSensorPoint'
import { useSelectedPoints } from '../SelectedPointsContext'

// This function calculates the end date based on the start and end date, with a maximum of 365 days.
// If the difference between the start and end date is greater than 365 days, it returns the end date
// that is at most 365 days days from the start date.
function formattedEndDate(start = '', end = '') {
  const startDate = new Date(start).valueOf()
  const endDate = new Date(end).valueOf()
  const diffInDays = (endDate - startDate) / (1000 * 60 * 60 * 24)
  if (!Number.isNaN(diffInDays) && diffInDays > 365) {
    return new Date(new Date(start).getTime() + 365 * 24 * 60 * 60 * 1000)
  }
  return end ?? new Date()
}

export default function Point(props) {
  const {
    site,
    sitePointId,
    equipment,
    isVisible,
    point,
    iconProps = {},
    className,
    style,
    enabledAutoSelect = true,
    // unlike other points, diagnostic points'
    // live data comes along with the point themselves
    type,
    times = [],
    isAnyDiagnosticSelected,
  } = props

  const analytics = useAnalytics()
  const selectedPoints = useSelectedPoints()

  const siteId = sitePointId.split('_')[0]
  const color = selectedPoints.pointColorMap[sitePointId]
  const isSelected = selectedPoints.pointIds.includes(sitePointId)
  const isDiagnosticPoint = type === PointType.DiagnosticPoint
  const [controlledStart, controlledEnd] = times
  // Setting the interval to null when user clicks on Monitor button in Diagnostic section
  const interval = isAnyDiagnosticSelected
    ? null
    : selectedPoints.params?.interval
  const isControlled = controlledStart && controlledEnd
  const [start, end] = isControlled
    ? times
    : [selectedPoints.params?.start, selectedPoints.params?.end]
  const endDate = formattedEndDate(start, end)

  useEffect(() => {
    if (enabledAutoSelect && point.defaultOn) {
      selectedPoints.onSelectPoint(sitePointId)
      analytics.track('Point Selected', {
        name: point.name,
        site: site.name,
      })
    }
  }, [])

  const handleClick = () => {
    analytics.track(isSelected ? 'Point Deselected' : 'Point Selected', {
      name: point.name,
      site: site.name,
    })
    selectedPoints.onSelectPoint(sitePointId, !isSelected)
  }

  const isImpactScorePoint = point.type === PointType.ImpactScorePoint

  const { sensorPoint, isLoading } = useSensorPoint(
    isImpactScorePoint
      ? `/sites/${siteId}/livedata/impactScores/${point.entityId}`
      : `/sites/${siteId}/points/${point.entityId}/liveData`,

    isControlled
      ? {
          ...selectedPoints.params,
          start: controlledStart,
          end: endDate,
          interval,
        }
      : {
          ...selectedPoints.params,
          end: endDate,
          interval,
        },
    {
      // live data for diagnostic points are returned along with the point themselves
      enabled: isSelected && type !== PointType.DiagnosticPoint,
      onError: () => selectedPoints.onRemovePoint(sitePointId),
    }
  )

  useEffect(() => {
    if (sensorPoint != null && isSelected && !isDiagnosticPoint) {
      selectedPoints.onLoadedPoint({
        data: isImpactScorePoint
          ? {
              ...sensorPoint,
              pointName: point.name,
              // point.timeSeriesData returned by `/sites/${siteId}/points/${point.entityId}/liveData`
              // is called "TimeSeriesAnalogData", so it is always an analog point
              // so add pointType and unit to the data since it is not returned by the API
              pointId: point.entityId,
              pointType: 'analog',
              unit: point.unit,
            }
          : sensorPoint,
        pointId: point.entityId,
        sitePointId,
        siteAssetId: `${siteId}_${equipment.id}`,
        assetId: equipment.id,
        pointEntityId: point.entityId,
        color,
      })
    }

    // boolean type live data comes from live data endpoint looks like:
    // {
    //   timeSeriesData: [
    //    {
    //       onCount: 0,
    //       offCount: 1,
    //       timestamp: '2022-12-04T22:00:00.000Z',
    //     },
    //   ],
    //   pointId: '50b063f3-e0a2-4ee2-bd40-7b4672a628b5',
    //   pointEntityId: '50b063f3-e0a2-4ee2-bd40-7b4672a628b5',
    //   pointName: 'exhaust air fan fault sensor',
    //   pointType: 'binary',
    //   unit: 'Bool',
    // }
    // so we need to transform diagnostic point live data to the same format
    if (
      isDiagnosticPoint &&
      isSelected &&
      point?.occurrenceLiveData?.timeSeriesData
    ) {
      const pointEntityId = point?.entityId ?? ''
      selectedPoints.onLoadedPoint({
        data: {
          timeSeriesData: (point?.occurrenceLiveData?.timeSeriesData ?? []).map(
            (oneData) => ({
              ...oneData,
              timestamp: oneData.start,
              // if the point is faulty, onCount is 1, otherwise offCount is 1
              onCount: oneData.isFaulty ? 1 : 0,
              offCount: oneData.isFaulty ? 0 : 1,
            })
          ),
          pointId: pointEntityId,
          pointEntityId,
          pointName: point.name,
          pointType: 'binary',
          unit: 'Bool',
          isDiagnosticPoint: true,
        },
        pointId: pointEntityId,
        sitePointId: `${siteId}_${pointEntityId}`,
        siteAssetId: `${siteId}_${point?.pointTwinId}`,
        assetId: point?.pointTwinId,
        pointEntityId,
        color,
      })
    }
  }, [
    sensorPoint,
    isSelected,
    isDiagnosticPoint,
    point?.occurrenceLiveData?.timeSeriesData,
  ])

  useEffect(() => {
    if (isLoading) selectedPoints.onLoadPoint(sitePointId)
  }, [isLoading])

  const icon = isSelected ? 'eye-open' : 'eye-close'

  return (
    <Button
      className={cx(
        styles.point,
        {
          [styles.isSelected]: isSelected,
          [styles.isVisible]: isVisible,
        },
        className
      )}
      style={style}
      onClick={handleClick}
    >
      <Flex horizontal fill="content" align="middle" size="medium" width="100%">
        <Icon
          icon={isLoading ? 'progress' : icon}
          size="tiny"
          className={styles.icon}
          style={{ color: isSelected && !isLoading ? color : undefined }}
          {...iconProps}
        />

        <Flex>
          <Text type="message" color="grey" size="tiny">
            {point.externalPointId}
          </Text>
          <Text>{point.name}</Text>
        </Flex>
      </Flex>
    </Button>
  )
}
