/* eslint-disable complexity */
import { useRef, useState, useMemo, useLayoutEffect, useEffect } from 'react'
import {
  useAnalytics,
  Flex,
  FloatingPanel,
  FloatingPanelSection,
  Icon,
  Input,
  Text,
  Progress,
  useSnackbar as legacyUseSnackbar,
} from '@willow/ui'
import { useSnackbar } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { useGetEquipment } from 'hooks'
import { styled, css } from 'twin.macro'
import { useQueryClient } from 'react-query'
import { useSites } from '../../../providers/sites/SitesContext'
import Point from './Point'
import { useSelectedPoints } from '../SelectedPointsContext'

const StyledFloatingPanel = styled(FloatingPanel)({
  flexShrink: '1',
  minWidth: '240px',
  maxWidth: '240px',
})

export default function Equipments({ siteId, assetId, twinInfo }) {
  const analytics = useAnalytics()
  const sites = useSites()
  const { t } = useTranslation()
  const legacySnackbar = legacyUseSnackbar()
  const snackbar = useSnackbar()
  const queryClient = useQueryClient()
  const {
    isInsightPointsLoading,
    insightPoints = [],
    impactScorePoints = [],
    diagnosticPoints = [],
    twinId,
    twinName,
    siteInsightId,
    isAnyDiagnosticSelected,
    diagnosticStart,
    diagnosticEnd,
  } = twinInfo ?? {}

  const equipmentsRef = useRef()
  const [search, setSearch] = useState('')
  const {
    isLoading: isLoadingPoint,
    pointIds: selectedPointIds,
    onSelectPoint,
  } = useSelectedPoints()

  const getSelectedIndex = (point) =>
    selectedPointIds.indexOf(`${siteId}_${point.entityId}`)

  const site = useMemo(
    () => sites.find((layoutSite) => layoutSite.id === siteId),
    [siteId]
  )

  const { data: equipment, isLoading } = useGetEquipment(siteId, assetId, {
    onError: () => legacySnackbar.show(t('plainText.errorLoadingEquipment')),
  })

  // if insight points are present, they will be turned on by default and equipments points are turned off;
  const points = useMemo(() => {
    const insightRelatedPointsExist =
      insightPoints.length > 0 || impactScorePoints.length > 0
    const insightPointsIds = insightPoints.map((p) => p?.entityId)
    const equipmentPointsNotFromInsight = (equipment?.points ?? [])
      .filter((point) => insightPointsIds.indexOf(point?.entityId) < 0)
      .map((p) => ({
        ...p,
        defaultOn: !insightRelatedPointsExist && p?.defaultOn,
      }))

    const combinedPoints = [
      ...diagnosticPoints,
      ...insightPoints,
      ...impactScorePoints,
      ...equipmentPointsNotFromInsight,
    ]

    // Limit number of points on initial Load to 20.
    // Link : https://dev.azure.com/willowdev/Unified/_workitems/edit/130072
    return combinedPoints.reduce(
      ({ pointsArray, count }, point) => {
        let nextCount = count
        if (nextCount === 20) {
          pointsArray.push({ ...point, defaultOn: false })
          return {
            pointsArray,
            count: nextCount,
          }
        }

        if (point.defaultOn) {
          nextCount += 1
        }

        pointsArray.push(point)

        return {
          pointsArray,
          count: nextCount,
        }
      },
      {
        pointsArray: [],
        count: 0,
      }
    ).pointsArray
  }, [diagnosticPoints, equipment?.points, impactScorePoints, insightPoints])

  // need to remove points from graph if they are not available;
  // selectPointIds are the point ids ever loaded into context,
  // and point from points are available points to be displayed;
  // this logic is only relevant when displaying points in InsightNode
  // as user can navigate from 1 insight to another insight
  // and we want to ensure to remove the line graphs that isn't
  // relevant to the new insight
  useLayoutEffect(() => {
    if (siteInsightId) {
      for (const pointId of selectedPointIds) {
        const point = points.find((p) => `${siteId}_${p.entityId}` === pointId)
        if (point == null) {
          onSelectPoint(pointId, false)
        }
      }
    }
  }, [siteInsightId])

  // Calculating the failed sensor points API count
  const failedSensorPointsCount = useMemo(
    () =>
      queryClient
        .getQueryCache()
        .queries.filter(
          ({ queryKey, state }) =>
            queryKey.includes('points') && state.status === 'error'
        ).length ?? 0,
    [queryClient.isFetching()]
  )

  // Showing the snackbar when failed sensor points API count is present and the API calls are completed
  useEffect(() => {
    if (failedSensorPointsCount > 0 && !queryClient.isFetching()) {
      snackbar.show({
        id: 'failedSensorPoints',
        title: t('headers.timeSeries'),
        description:
          failedSensorPointsCount > 1
            ? t('interpolation.multipleTimeSeriesError', {
                count: failedSensorPointsCount,
              })
            : t('plainText.timeSeriesError'),
        intent: 'notice',
      })
    }
  }, [failedSensorPointsCount])

  return (
    <StyledFloatingPanel
      ref={equipmentsRef}
      header={
        <Flex horizontal fill="header" align="middle" padding="0 large 0 0">
          <Text>{t('plainText.currentSelection')}</Text>
          {(isLoadingPoint || isInsightPointsLoading) && (
            <Icon icon="progress" size="small" />
          )}
        </Flex>
      }
      defaultIsOpen={false}
      onClose={() => analytics.track('Time Series Current View Collapse')}
    >
      <Flex size="medium">
        <Flex>
          <Input
            icon="search"
            placeholder={t('labels.search')}
            debounce
            value={search}
            onChange={setSearch}
          />
        </Flex>
        <Flex size="medium">
          {equipment || twinId ? (
            <FloatingPanelSection
              key={equipment?.id ?? twinId}
              css={css`
                & > div {
                  white-space: wrap;
                }
                & button {
                  height: auto;
                }
              `}
              header={
                <div
                  css={css`
                    display: flex;
                    flex-direction: column;
                  `}
                >
                  {site && (
                    <Text size="tiny" color="grey">
                      {site.name}
                    </Text>
                  )}
                  <Text>{equipment?.name ?? twinName}</Text>
                </div>
              }
              onClose={() =>
                analytics.track('Time Series Collapse Current Selected Asset')
              }
            >
              {points
                .sort((a, b) => getSelectedIndex(b) - getSelectedIndex(a))
                .map((point) => {
                  const sitePointId = `${siteId}_${point.entityId}`
                  return (
                    <Point
                      key={sitePointId}
                      site={site}
                      sitePointId={sitePointId}
                      equipment={equipment ?? { id: twinInfo.twinId }}
                      isVisible={
                        search === '' ||
                        getSelectedIndex(point) >= 0 ||
                        (point?.externalPointId ?? '')
                          .toLowerCase()
                          .includes(search.toLowerCase()) ||
                        point.name.toLowerCase().includes(search.toLowerCase())
                      }
                      point={point}
                      type={point.type}
                      times={
                        isAnyDiagnosticSelected &&
                        diagnosticStart &&
                        diagnosticEnd
                          ? [diagnosticStart, diagnosticEnd]
                          : undefined
                      }
                      isAnyDiagnosticSelected={isAnyDiagnosticSelected}
                    />
                  )
                })}
            </FloatingPanelSection>
          ) : isLoading || isInsightPointsLoading ? (
            <Progress />
          ) : null}
        </Flex>
      </Flex>
    </StyledFloatingPanel>
  )
}
