import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import {
  ALL_LOCATIONS,
  DocumentTitle,
  ScopeSelectorWrapper,
  useAnalytics,
  useDateTime,
  useDuration,
  useFeatureFlag,
  useScopeSelector,
  useUser,
} from '@willow/ui'
import { getDateTimeRange } from '@willow/ui/components/DatePicker/DatePicker/QuickRangeOptions.tsx'
import {
  PageTitle,
  PageTitleItem,
  Panel,
  PanelContent,
  PanelGroup,
  useDisclosure,
} from '@willowinc/ui'
import _ from 'lodash'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useHistory, useLocation } from 'react-router'
import { Link } from 'react-router-dom'
import styled, { css } from 'styled-components'

import {
  replaceTimeZoneForDateTimeRange,
  TimeSeriesProvider,
  usePointSelector,
} from 'components/TimeSeries/'
import { useTimeSeries } from 'components/TimeSeries/TimeSeriesContext'
import TimeSeriesGraph, {
  getDefaultGranularity,
} from 'components/TimeSeriesGraph'
import {
  getTimeZone,
  useTimeZoneInfo,
} from 'components/TimeZoneSelect/useGetTimeZones.ts'
import { useSites } from 'providers'

import EChartsTimeSeriesGraph from '../../components/EChartsTimeSeriesGraph/TimeSeriesGraph'
import routes from '../../routes'
import { LayoutHeader } from '../Layout'
import HeaderWithTabs from '../Layout/Layout/HeaderWithTabs'
import SearchResultsProvider from '../Portfolio/twins/results/page/state/SearchResults'
import AssetSelectorModal from './AssetSelectorModal/AssetSelectorModal'
import EChartsTimeSeriesHeader from './EChartsTimeSeriesHeader'
import TimeSeriesHeader, { TimeSeriesButtonControls } from './TimeSeriesHeader'
import TimeSeriesSearchModal from './TimeSeriesSearchModal/TimeSeriesSearchModal'

const LayoutHeaderContainer = styled.div({
  alignItems: 'center',
  display: 'flex',
  height: '100%',
})

function TimeSeriesComponent({
  setIsAssetSelectorModalOpen,
  onQuickRangeChange,
  state,
  quickRange,
  timeZoneOption,
  timeZone,
  onTimeZoneChange,
  onTimesChange,
  graphZoom,
  isResetButtonVisible,
  onReset,
  setState,
  isAssetSelectorModalOpen,
  onClose,
}) {
  const featureFlags = useFeatureFlag()
  const scopeSelector = useScopeSelector()
  const { t } = useTranslation()
  const analytics = useAnalytics()
  const timeSeries = useTimeSeries()
  const history = useHistory()

  const [
    { modalTab, modalAssetId, insightId, ticketId, insightTab = 'summary' },
    setSearchParams,
  ] = useMultipleSearchParams([
    'modalTab',
    'modalAssetId',
    'insightId',
    'ticketId',
    'insightTab',
  ])
  // This useMemo prevents the useEffect below from getting stuck in a loop when
  // selectedSiteIds is updated, as the array would be considered a different object
  // without the useMemo hook.
  const selectedSiteIds = useMemo(
    () =>
      scopeSelector.location?.twin?.id === ALL_LOCATIONS
        ? []
        : scopeSelector.location?.children?.length
        ? scopeSelector.descendantSiteIds
        : [scopeSelector.location?.twin?.siteId],
    [
      scopeSelector.descendantSiteIds,
      scopeSelector.location?.children?.length,
      scopeSelector.location?.twin?.id,
      scopeSelector.location?.twin?.siteId,
    ]
  )

  useEffect(() => {
    timeSeries.setSelectedSiteIds(selectedSiteIds)
    // TODO: Update the search context too, once this has been updated to support scopes fully
  }, [selectedSiteIds, timeSeries])

  const handleModalTabChange = (newModalTab) => {
    setSearchParams({ modalTab: newModalTab })
  }

  const handleAssetChange = ({
    modalAssetId: newModalAssetId,
    modalTab: newModalTab,
  }) => {
    setSearchParams({
      modalAssetId: newModalAssetId,
      modalTab: newModalTab,
      insightId: undefined,
    })
  }

  const handleInsightIdChange = (newInsightId) => {
    setSearchParams({ insightId: newInsightId })
  }

  const handleTicketIdChange = (newTicketId) => {
    setSearchParams({ ticketId: newTicketId })
  }

  const handleInsightTabChange = useCallback(
    (newInsightTab) => {
      setSearchParams({ insightTab: newInsightTab })
    },
    [setSearchParams]
  )

  const [
    isSearchModalOpen,
    { open: openSearchModal, close: closeSearchModal },
  ] = useDisclosure(
    featureFlags.hasFeatureToggle('timeSeriesSearchModal') &&
      state.siteEquipmentIds?.length === 0
  )

  function handlePanelCollapse(collapsed) {
    analytics.track(
      `Time Series Current View ${collapsed ? 'Collapse' : 'Expanded'}`
    )
  }

  const [pointerSelectorContent, pointerSelectorFooter] = usePointSelector({
    modalTab,
    modalAssetId,
    insightId,
    onModalTabChange: handleModalTabChange,
    onAssetChange: handleAssetChange,
    onInsightIdChange: handleInsightIdChange,
    setIsAssetSelectorModalOpen,
    selectedTicketId: ticketId,
    onSelectedTicketIdChange: handleTicketIdChange,
    insightTab,
    onInsightTabChange: handleInsightTabChange,
    openSearchModal,
  })

  return (
    <>
      <DocumentTitle
        scopes={[t('headers.timeSeries'), scopeSelector.locationName]}
      />
      {featureFlags.hasFeatureToggle('scopeSelector') && (
        <LayoutHeader>
          <SearchResultsProvider>
            <LayoutHeaderContainer>
              <ScopeSelectorWrapper
                onLocationChange={(location) => {
                  const { twin } = location
                  history.push(
                    !twin?.id || twin?.id === ALL_LOCATIONS
                      ? routes.timeSeries
                      : routes.timeSeries_scope__scopeId(twin.id)
                  )
                }}
              />
            </LayoutHeaderContainer>
          </SearchResultsProvider>
        </LayoutHeader>
      )}
      <HeaderWithTabs
        titleRow={[
          <PageTitle key="pageTitle">
            <PageTitleItem>
              <Link to={window.location.pathname + window.location.search}>
                {t('headers.timeSeries')}
              </Link>
            </PageTitleItem>
          </PageTitle>,
          <TimeSeriesButtonControls
            key="timeSeriesButtonControls"
            timeZoneOption={timeZoneOption}
          />,
        ]}
        css={{ borderBottom: 'none' }}
      />

      <PanelGroup
        resizable
        css={css(({ theme }) => ({
          padding: theme.spacing.s16,
        }))}
      >
        <StyledPanel
          collapsible
          onCollapse={handlePanelCollapse}
          title={t('headers.twins')}
          footer={pointerSelectorFooter}
          defaultSize={25}
          css={{ minWidth: 240 }}
        >
          <PanelContent css={{ height: '100%' }}>
            {pointerSelectorContent}
          </PanelContent>
        </StyledPanel>
        <StyledPanel
          title={t('headers.timeSeries')}
          headerControls={
            featureFlags.hasFeatureToggle('eChartsTimeSeries') ? (
              <EChartsTimeSeriesHeader
                onQuickRangeChange={onQuickRangeChange}
                times={state.times}
                type={state.type}
                granularity={state.granularity}
                quickRange={quickRange}
                onTimesChange={onTimesChange}
                timeZoneOption={timeZoneOption}
                timeZone={timeZone}
                onTimeZoneChange={onTimeZoneChange}
              />
            ) : (
              <TimeSeriesHeader
                onQuickRangeChange={onQuickRangeChange}
                times={state.times}
                type={state.type}
                granularity={state.granularity}
                quickRange={quickRange}
                onTimesChange={onTimesChange}
                timeZoneOption={timeZoneOption}
                timeZone={timeZone}
                onTimeZoneChange={onTimeZoneChange}
              />
            )
          }
        >
          <PanelContent css={{ height: '100%' }}>
            {featureFlags.hasFeatureToggle('eChartsTimeSeries') ? (
              <EChartsTimeSeriesGraph
                enabledDisplayByAsset
                pointsData={timeSeries.points}
                loadingSitePointIds={timeSeries.loadingSitePointIds}
                graphZoom={graphZoom}
                isResetButtonVisible={isResetButtonVisible}
                onReset={onReset}
                onTimesChange={(newTimes, isGraphZoom) =>
                  onTimesChange(newTimes, true, isGraphZoom)
                }
                granularity={state.granularity}
                type={state.type}
                times={state.times}
                onTypeChange={(type) =>
                  setState((prevState) => ({
                    ...prevState,
                    type,
                  }))
                }
                onGranularityChange={(granularity) =>
                  setState((prevState) => ({
                    ...prevState,
                    granularity,
                  }))
                }
                timeZone={timeZone}
              />
            ) : (
              <TimeSeriesGraph
                enabledDisplayByAsset
                pointsData={timeSeries.points}
                loadingSitePointIds={timeSeries.loadingSitePointIds}
                graphZoom={graphZoom}
                isResetButtonVisible={isResetButtonVisible}
                onReset={onReset}
                onTimesChange={(newTimes, isGraphZoom) =>
                  onTimesChange(newTimes, true, isGraphZoom)
                }
                granularity={state.granularity}
                type={state.type}
                times={state.times}
                onTypeChange={(type) =>
                  setState((prevState) => ({
                    ...prevState,
                    type,
                  }))
                }
                onGranularityChange={(granularity) =>
                  setState((prevState) => ({
                    ...prevState,
                    granularity,
                  }))
                }
                timeZone={timeZone}
              />
            )}
          </PanelContent>
        </StyledPanel>
      </PanelGroup>
      <TimeSeriesSearchModal
        opened={isSearchModalOpen}
        onClose={closeSearchModal}
      />
      {isAssetSelectorModalOpen && <AssetSelectorModal onClose={onClose} />}
    </>
  )
}

const defaultQuickRange = '7D'

export default function TimeSeries() {
  const featureFlags = useFeatureFlag()
  const analytics = useAnalytics()
  const dateTime = useDateTime()
  const duration = useDuration()
  const user = useUser()
  const sites = useSites()
  const history = useHistory()
  const location = useLocation()
  const [quickRange, setQuickRange] = useState(
    user.options?.timeSeriesState
      ? user.options.timeSeriesState.quickSelectTimeRange
      : defaultQuickRange
  )

  const [timeZoneOption, setTimeZoneOption] = useState(() => {
    const userTimeZoneOption = user.options?.timeSeriesState?.timeZoneOption
    const timeZoneSite = userTimeZoneOption?.siteId
      ? sites.find((site) => site.id === userTimeZoneOption.siteId)
      : undefined

    // Construct timeZoneOption from site if site-timeZone option is used in case
    // site's timeZone has been updated.
    return timeZoneSite
      ? { siteId: timeZoneSite.id, timeZoneId: timeZoneSite.timeZoneId }
      : userTimeZoneOption
  })
  const timeZoneInfo = useTimeZoneInfo(timeZoneOption?.timeZoneId)

  const timeZone = timeZoneInfo && getTimeZone(timeZoneInfo)

  const [state, setState] = useState(() => {
    const defaultTimes = quickRange
      ? getDateTimeRange(dateTime.now(timeZone), quickRange)
      : user.options?.timeSeriesState?.times

    const importedAssets = user.options?.timeSeriesImport?.assets || []
    const importedSensors = user.options?.timeSeriesImport?.sensors || []

    if (user.options?.timeSeriesState) {
      const timeSeriesState = { ...user.options.timeSeriesState }
      const equipmentIds = timeSeriesState.siteEquipmentIds || []
      const pointIds = timeSeriesState.sitePointIds || []

      timeSeriesState.siteEquipmentIds = _.uniq([
        ...equipmentIds,
        ...importedAssets,
      ])

      timeSeriesState.sitePointIds = _.uniq([...pointIds, ...importedSensors])

      return {
        ...timeSeriesState,
        times: defaultTimes,
      }
    }

    return {
      siteId: sites[0].id,
      times: defaultTimes,
      type: 'asset',
      siteEquipmentIds: importedAssets,
      sitePointIds: importedSensors,
      granularity: duration(getDefaultGranularity(defaultTimes)).toISOString(),
      name: undefined /* FavoritePreset Name */,
      kind: undefined /*  FavoritePreset Type - 'Personal' or 'Site' */,
    }
  })

  const [userSelectedEquipmentIds, setUserSelectedEquipmentIds] = useState([])
  const [graphZoom, setGraphZoom] = useState(false)

  const [isAssetSelectorModalOpen, setIsAssetSelectorModalOpen] = useState(
    !featureFlags.hasFeatureToggle('timeSeriesSearchModal') &&
      state.siteEquipmentIds?.length === 0
  )
  const [isResetButtonVisible, setIsResetButtonVisible] = useState(false)
  // The previous quick select options before graphZoom is applied so we can go back to these options when reset.
  const [prevQuickSelect, setPrevQuickSelect] = useState(null)

  useEffect(() => {
    analytics.track('Time Series Viewed', { context: 'Full Time Series View' })
  }, [analytics])

  useEffect(() => {
    user.saveOptions('timeSeriesImport', {
      assets: [],
      sensors: [],
    })

    user.saveOptions('timeSeriesState', {
      ...state,
      quickSelectTimeRange: quickRange,
      timeZoneOption,
      timeZone,
    })
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [state, quickRange, timeZoneOption, timeZone])

  useEffect(() => {
    if (location.pathname === routes.timeSeries_addTwin) {
      setIsAssetSelectorModalOpen(true)
    }
  }, [location])

  function removeUserSelectedEquipmentId(siteEquipmentId) {
    setUserSelectedEquipmentIds((prevIds) =>
      prevIds.filter((prevEquipmentId) => prevEquipmentId !== siteEquipmentId)
    )
  }

  function addUserSelectedEquipmentId(siteEquipmentId) {
    setUserSelectedEquipmentIds((prevIds) => [...prevIds, siteEquipmentId])
  }

  function handleTimesChange(times, sendAnalyticEvent, isGraphZoom = false) {
    let nextTimes = times
    if (times.length === 0) {
      // Handles reset - Reset timeZone, quick range and time range (based on timeZone & quick range).
      setTimeZoneOption(null)
      handleQuickRangeChange(defaultQuickRange)
      nextTimes = getDateTimeRange(dateTime.now(), defaultQuickRange)
    }

    if (sendAnalyticEvent) {
      const diffTime = Math.abs(dateTime(nextTimes[1]) - dateTime(nextTimes[0]))
      const range = Math.ceil(diffTime / (1000 * 60 * 60 * 24))
      const eventName = isGraphZoom
        ? 'Time Series Graph Zoom'
        : 'Time Series Calendar Date Range'
      analytics.track(eventName, { range_of_days: range })
    }

    if (isGraphZoom && !graphZoom) {
      setGraphZoom(true)
      setPrevQuickSelect({ quickRange, times: state.times })
      setIsResetButtonVisible(true)
      setQuickRange(null)
    }
    setState((prevState) => ({
      ...prevState,
      times: nextTimes,
      granularity: duration(getDefaultGranularity(nextTimes)).toISOString(),
    }))
  }

  function handleQuickRangeChange(selectedQuickRange) {
    setGraphZoom(false)
    setQuickRange(selectedQuickRange)
    setIsResetButtonVisible(false)
  }

  function handleTimeZoneChange(nextTimeZoneOption, nextTimeZoneInfo) {
    analytics.track('DateTimeRange Timezone changed')
    setTimeZoneOption(nextTimeZoneOption)

    const nextTimeZone = nextTimeZoneInfo && getTimeZone(nextTimeZoneInfo)
    const times = replaceTimeZoneForDateTimeRange(
      state.times,
      timeZone,
      nextTimeZone
    )

    setState({
      ...state,
      times,
    })
  }

  function handleReset() {
    if (prevQuickSelect) {
      handleQuickRangeChange(prevQuickSelect.quickRange)
      handleTimesChange(prevQuickSelect.times)
    }
    setGraphZoom(false)
    analytics.track('Time Series Reset')
  }
  // AssetSelector Modal Functions
  function onClose() {
    history.replace(routes.timeSeries)
    setIsAssetSelectorModalOpen(false)
  }

  return (
    <TimeSeriesProvider
      state={state}
      setState={setState}
      times={state.times}
      type={state.type}
      granularity={state.granularity}
      siteAssetIds={state.siteEquipmentIds}
      sitePointIds={state.sitePointIds}
      quickRange={quickRange}
      timeZoneOption={timeZoneOption}
      timeZone={timeZone}
      setTimeRange={handleQuickRangeChange}
      setTimeZoneOption={setTimeZoneOption}
      onSiteAssetIdsChange={(fn) =>
        setState((prevState) => ({
          ...prevState,
          siteEquipmentIds: fn(prevState.siteEquipmentIds),
        }))
      }
      onSitePointIdsChange={(sitePointIds) =>
        setState((prevState) => ({
          ...prevState,
          sitePointIds,
        }))
      }
      userSelectedEquipmentIds={userSelectedEquipmentIds}
      addUserSelectedEquipmentId={addUserSelectedEquipmentId}
      removeUserSelectedEquipmentId={removeUserSelectedEquipmentId}
    >
      <TimeSeriesComponent
        setIsAssetSelectorModalOpen={setIsAssetSelectorModalOpen}
        state={state}
        quickRange={quickRange}
        onQuickRangeChange={handleQuickRangeChange}
        timeZoneOption={timeZoneOption}
        timeZone={timeZone}
        onTimeZoneChange={handleTimeZoneChange}
        onTimesChange={handleTimesChange}
        graphZoom={graphZoom}
        isResetButtonVisible={isResetButtonVisible}
        onReset={handleReset}
        setState={setState}
        isAssetSelectorModalOpen={isAssetSelectorModalOpen}
        onClose={onClose}
      />
    </TimeSeriesProvider>
  )
}

const StyledPanel = styled(Panel)(
  ({ theme }) => css`
    background-color: ${theme.color.neutral.bg.panel.default};
  `
)
