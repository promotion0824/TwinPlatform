import { Flex, Message, useSize } from '@willow/ui'
import _ from 'lodash'
import { useRef } from 'react'
import { useTranslation } from 'react-i18next'
import { css, styled } from 'twin.macro'
import GraphContent from './Graph/GraphContent'
import Header from './Header/Header'
import { TimeSeriesGraphContext } from './TimeSeriesGraphContext'

export default function EChartsTimeSeriesGraph({
  compact = false,
  enabledDisplayByAsset = false,
  isResetButtonVisible,
  onTimesChange, // function (newTimes, isGraphZoom)
  granularity,
  onTypeChange,
  type,
  times,
  onGranularityChange,
  onReset,
  graphZoom,
  pointsData,
  loadingSitePointIds,
  timeZone,
  shadedDurations,
  diagnosticBoundaries,
}) {
  const { t } = useTranslation()
  const headerRef = useRef()
  const contentRef = useRef()
  const size = useSize(headerRef)

  function getKey(points) {
    const point = points[0]

    if (
      point.data.pointType === 'binary' ||
      point.data.pointType === 'multiState'
    ) {
      return point.pointId
    }

    if (type === 'grouped') {
      return point.data.unit
    }

    if (type === 'stacked') {
      return point.pointId
    }

    if (type === 'shared') {
      return 'shared'
    }

    return `${point.assetId}_${point.data.unit}`
  }

  function getGraphType(points) {
    const point = points[0]

    return point.data.pointType
  }

  const allPoints = pointsData.filter((point) => point.data != null)

  let linePoints = allPoints.filter(
    (point) => point.data.pointType === 'analog'
  )
  if (type === 'grouped') {
    linePoints = _(linePoints)
      .groupBy((point) => point.data.unit)
      .map((group) => group)
      .value()
  } else if (type === 'stacked') {
    linePoints = linePoints.map((point) => [point])
  } else if (type === 'shared') {
    linePoints = _(linePoints)
      .groupBy(() => 'shared')
      .map((group) => group)
      .value()
  } else {
    linePoints = _(linePoints)
      .groupBy((point) => `${point.assetId}_${point.data.unit}`)
      .map((group) => group)
      .value()
  }

  // business logic to prioritize diagnostic points (which is a type of binary point)
  // over analog points and display last added point first
  // https://dev.azure.com/willowdev/Unified/_workitems/edit/92377
  // reference: https://www.figma.com/file/hk0xZCLrNjp7QAsKDW5yGJ/Diagnostics-Exploration?node-id=1297%3A63053&mode=dev
  const nonAnalogPoints = _.reverse(
    allPoints
      .filter((graph) => graph.data.pointType !== 'analog')
      .map((graph) => [graph])
  )
  const graphs = [...nonAnalogPoints, ...linePoints]
    .map((points) => ({
      key: getKey(points),
      type: getGraphType(points),
      lines: points.map((point) => ({
        pointId: point.pointId,
        sitePointId: point.sitePointId,
        name: point.data.pointName,
        assetId: point.assetId,
        assetName: point.assetName,
        unit: point.data.unit,
        color: point.color,
        type: getGraphType(points),
        valueMap: point.data.valueMap,
        data: point.data.timeSeriesData.map((data) => ({
          isDiagnosticPoint: point.data?.isDiagnosticPoint || false,
          state: data.state,
          average: data.average,
          minimum: data.minimum,
          maximum: data.maximum,
          onCount: data.onCount,
          offCount: data.offCount,
          time: new Date(data.timestamp).valueOf(),
          timestamp: data.timestamp,
        })),
      })),
    }))
    .map((group) => ({
      ...group,
      unit: group.lines[0].unit,
    }))

  const context = { headerRef, size, contentRef, times, granularity, timeZone }
  return (
    <TimeSeriesGraphContext.Provider value={context}>
      <NoOverflowFlex fill="content" flex="1">
        <Header
          isResetButtonVisible={isResetButtonVisible}
          onReset={onReset}
          graphZoom={graphZoom}
          compact={compact}
          enableLegend={shadedDurations != null}
        />
        <div
          ref={headerRef}
          css={css`
            margin-bottom: 40px;
          `}
        >
          {graphs.length === 0 ? (
            <NoTrendMessage icon="graph">
              {t('plainText.trendPoints')}
            </NoTrendMessage>
          ) : (
            <GraphContent
              enabledDisplayByAsset={enabledDisplayByAsset}
              loadingSitePointIds={loadingSitePointIds}
              graphs={graphs}
              onTimesChange={onTimesChange}
              granularity={granularity}
              onTypeChange={onTypeChange}
              type={type}
              times={times}
              onGranularityChange={onGranularityChange}
              shadedDurations={shadedDurations}
              diagnosticBoundaries={diagnosticBoundaries}
            />
          )}
        </div>
      </NoOverflowFlex>
    </TimeSeriesGraphContext.Provider>
  )
}

const NoOverflowFlex = styled(Flex)({
  // Otherwise the tooltip above the graph which displays the hovered date can
  // cause a horizontal scrollbar to appear if the user hovers over the
  // rightmost part of the graph on a narrow window.
  overflowX: 'hidden',
  position: 'relative',
})

const NoTrendMessage = styled(Message)({
  left: '50%',
  position: 'absolute',
  top: '50%',
})
