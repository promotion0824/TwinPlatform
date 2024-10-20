import {
  IconNew,
  OnClickOutside,
  SearchList,
  useAnalytics,
  useFeatureFlag,
  useSnackbar,
  useWindowEventListener,
} from '@willow/ui'
import { Badge, Group, Tabs } from '@willowinc/ui'
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import tw, { styled } from 'twin.macro'

import EChartsMiniTimeSeriesComponent from 'components/EChartsMiniTimeSeries/MiniTimeSeriesComponent'
import {
  MiniTimeSeriesComponent,
  Point,
  useSelectedPoints,
} from 'components/MiniTimeSeries/'
import { useGetEquipment } from 'hooks'
import { useSites } from '../../../../providers/sites/SitesContext'
import useTwinAnalytics from '../useTwinAnalytics'

const PointList = styled.div(({ theme }) => ({
  position: 'absolute',
  boxShadow: '5px 3px 6px #00000029',
  background: theme.color.neutral.bg.panel.default,
  width: '260px',
  zIndex: '10',
  overflow: 'auto',
  border: `1px solid ${theme.color.neutral.border.default}`,
  borderTop: '0',
}))

const PointItem = styled(Point)(({ isSelected }) => [
  isSelected &&
    tw`backgroundColor[var(--theme-color-neutral-bg-accent-default)]`,
])

const Equipments = ({
  tabHeaderRef,
  isFirstLoad,
  onFirstLoad,
  isShown,
  siteId,
  assetId,
  setShown,
}) => {
  const { t } = useTranslation()
  const snackbar = useSnackbar()
  const listRef = useRef()
  const { data: equipment, isLoading } = useGetEquipment(siteId, assetId, {
    onError: () => snackbar.show(t('plainText.errorLoadingEquipment')),
  })
  const { pointIds, loadingPointIds } = useSelectedPoints()
  // This component is shown by default if this is not the first time loading of data and there are no point selected.
  const [shownByDefault] = useState(!isFirstLoad && !pointIds.length)
  const sites = useSites()

  const currentSite = useMemo(
    () => sites.find((site) => site.id === siteId),
    [sites, siteId]
  )

  const positionUI = useCallback(() => {
    if (listRef.current) {
      const relativeEl = tabHeaderRef.current.offsetParent
      listRef.current.style.left = `${relativeEl.offsetLeft}px`
      listRef.current.style.maxHeight = `calc(100% - ${relativeEl.offsetHeight}px - 40px)`
    }
  }, [listRef, tabHeaderRef])

  useWindowEventListener('resize', positionUI)

  useEffect(() => {
    positionUI()
  }, [positionUI])

  useEffect(() => {
    // Check that the points data have been loaded.
    if (!isLoading && equipment) {
      // Show this component when:
      // - The points have not been loaded before and there are no points with featured tags; OR
      // - The points have been loaded before and shownByDefault is true
      if (isFirstLoad) {
        onFirstLoad(true)
        if (!equipment.points.some((point) => point.hasFeaturedTags)) {
          setShown(true)
        }
      } else if (shownByDefault) {
        setShown(true)
      }
    }
  }, [equipment, isLoading, setShown, isFirstLoad, onFirstLoad, shownByDefault])

  const getSelectedIndex = (point) =>
    pointIds.indexOf(`${siteId}_${point.entityId}`)
  const isSelectedPoint = (point) => getSelectedIndex(point) >= 0

  const handleClickout = () => {
    if (isShown) {
      setShown(false)
    }
  }

  return (
    <OnClickOutside targetRefs={[listRef]} onClose={handleClickout}>
      <PointList ref={listRef} style={{ display: isShown ? 'unset' : 'none' }}>
        {equipment ? (
          <SearchList
            inputPlaceholder={t('labels.search')}
            emptyMessage={t('plainText.noResultsFound')}
            items={[...equipment.points].sort(
              (a, b) => getSelectedIndex(b) - getSelectedIndex(a)
            )}
            searchKeys={['externalPointId', 'name']}
            filterFn={isSelectedPoint}
            renderItem={(item) => {
              const sitePointId = `${siteId}_${item.entityId}`
              const isSelected = isSelectedPoint(item)
              const isPointLoading = loadingPointIds.includes(sitePointId)
              return (
                <PointItem
                  enabledAutoSelect={isFirstLoad}
                  isSelected={isSelected}
                  key={sitePointId}
                  site={currentSite}
                  iconProps={
                    isPointLoading
                      ? {
                          // Added margin to compensate different sizing when not loading
                          style: { margin: '0 5px' },
                        }
                      : {
                          icon: isSelected ? 'layersFilled' : 'layers',
                          size: 'large',
                        }
                  }
                  sitePointId={sitePointId}
                  isVisible
                  point={item}
                  equipment={equipment}
                />
              )
            }}
          />
        ) : null}
      </PointList>
    </OnClickOutside>
  )
}

function TimeSeriesTabContent({
  twin,
  isFirstLoad,
  onFirstLoad,
  isShown,
  tabHeaderRef,
  name,
  handleShowList,
}) {
  const { siteID, uniqueID } = twin
  const featureFlags = useFeatureFlag()

  return (
    <>
      <Equipments
        isFirstLoad={isFirstLoad}
        onFirstLoad={onFirstLoad}
        isShown={isShown}
        siteId={siteID}
        assetId={uniqueID}
        tabHeaderRef={tabHeaderRef}
        setShown={handleShowList}
      />
      {featureFlags.hasFeatureToggle('eChartsMiniTimeseries') ? (
        <EChartsMiniTimeSeriesComponent
          siteEquipmentId={`${twin.siteID}_${twin.uniqueID}`}
          equipmentName={name}
          hideEquipments
        />
      ) : (
        <MiniTimeSeriesComponent
          siteEquipmentId={`${twin.siteID}_${twin.uniqueID}`}
          equipmentName={name}
          hideEquipments
        />
      )}
    </>
  )
}

function useTimeSeriesTab({ twin, asset }) {
  const analytics = useAnalytics()
  const { pointIds } = useSelectedPoints()
  const { t } = useTranslation()
  const twinAnalytics = useTwinAnalytics()

  // Track whether the data required are loaded for the first time to determine
  // whether the dropdown menu should be open when user returns to this tab.
  const [isFirstLoad, setFirstLoad] = useState(true)
  const [isShown, setShown] = useState(false)
  const tabHeaderRef = useRef()

  // TODO: These were previously available as props, but were actually passed in.
  // They should be investigated and reassigned/removed.
  const siteId = undefined
  const assetId = undefined

  const handleShowList = useCallback(
    (shown) => {
      setShown(shown)
      if (!shown) {
        analytics.track('Time Series Current View Collapse')
      }
    },
    [setShown, analytics]
  )

  useEffect(
    () => twinAnalytics.trackTimeSeriesViewed(twin),
    // We only track check `twin.id` rather than `twin` in the dependencies to
    // avoid re-firing the event if the user edits the twin. We're not worried
    // if the twin data we send in the event is not perfectly up to date.
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [(twinAnalytics, twin.id)]
  )

  useEffect(() => {
    // Reset isFirstLoad when siteId/assetId has changed
    setFirstLoad(true)
  }, [siteId, assetId])

  // It is possible for a high-level twin to not have associated siteId, e.g. a Campus, or a Land;
  // In this case, we cannot show the time series tab, as siteId is needed to fetch the equipment data.
  if (!twin.siteID || !asset?.hasLiveData) {
    return undefined
  }

  return [
    <Tabs.Tab
      data-testid="twin-timeSeries-tab"
      suffix={
        <Group ref={tabHeaderRef} wrap="nowrap">
          <IconNew
            tw="ml-1 hover:color[#fafafa]"
            onClick={() => handleShowList(!isShown)}
            icon={isShown ? 'layersCollapse' : 'layersExpand'}
            size="small"
          />
          <Badge>{pointIds.length}</Badge>
        </Group>
      }
      value="timeSeries"
    >
      {t('headers.timeSeries')}
    </Tabs.Tab>,
    <Tabs.Panel value="timeSeries">
      <TimeSeriesTabContent
        twin={twin}
        name={asset.name}
        isFirstLoad={isFirstLoad}
        onFirstLoad={setFirstLoad}
        isShown={isShown}
        tabHeaderRef={tabHeaderRef}
        handleShowList={handleShowList}
      />
    </Tabs.Panel>,
  ]
}

export default useTimeSeriesTab
