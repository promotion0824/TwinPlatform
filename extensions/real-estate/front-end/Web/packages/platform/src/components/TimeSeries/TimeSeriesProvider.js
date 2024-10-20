import { useAnalytics, useDateTime, useFeatureFlag } from '@willow/ui'
import { useTheme } from '@willowinc/ui'
import _ from 'lodash'
import { useSites } from 'providers'
import { useEffect, useState } from 'react'
import Assets from './Assets/Assets'
import Points from './Points/Points'
import { TimeSeriesContext } from './TimeSeriesContext'
import colors from './colors.json'

export default function TimeSeriesProvider({
  times,
  type,
  granularity,
  quickRange,
  timeZoneOption,
  timeZone,
  siteAssetIds,
  sitePointIds,
  onSitePointIdsChange,
  children,
  userSelectedEquipmentIds,
  addUserSelectedEquipmentId,
  removeUserSelectedEquipmentId,
  state,
  setState,
  setTimeRange,
  setTimeZoneOption,
}) {
  const analytics = useAnalytics()
  const sites = useSites()
  const theme = useTheme()
  const featureFlags = useFeatureFlag()

  const colorPalette = featureFlags.hasFeatureToggle('eChartsTimeSeries')
    ? Object.values(theme.color.data.qualitative)
    : colors

  function createAsset(siteAssetId) {
    const siteId = siteAssetId.split('_')[0]
    const assetId = siteAssetId.split('_')[1]

    return {
      siteAssetId,
      siteId,
      assetId,
    }
  }

  function createPoint(sitePointId) {
    const siteId = sitePointId.split('_')[0]
    const pointId = sitePointId.split('_')[1]

    return {
      sitePointId,
      siteId,
      pointId,
    }
  }

  const dateTime = useDateTime()
  const [activeAssetId, setActiveAssetId] = useState()
  const [now, setNow] = useState()
  const [assets, setAssets] = useState([])
  const [loadingSitePointIds, setLoadingSitePointIds] = useState([])

  // Limit number of points on initial Load to 20.
  // Link : https://dev.azure.com/willowdev/Unified/_workitems/edit/130072
  const [points, setPoints] = useState(() =>
    sitePointIds.slice(0, 20).map((sitePointId) => createPoint(sitePointId))
  )
  // Time-series data has intentionally been separated from selected points state "points" to prevent unwanted
  // state changes of parent component during "updatePoint" function and "onSitePointIdsChange" callback
  const [pointsData, setPointsData] = useState({})
  const [selectedSiteIds, setSelectedSiteIds] = useState([])

  // Filter out undefined values, which is what the old Site Selector provides for All Sites
  const allSitesSelected = !selectedSiteIds.filter((s) => s).length
  const nextAssets = assets.filter(
    (asset) => allSitesSelected || selectedSiteIds.includes(asset.siteId)
  )

  const nextPoints = points
    .filter((point) =>
      nextAssets
        .flatMap((asset) => asset.data?.points ?? [])
        .some((asset) => asset.sitePointId === point.sitePointId)
    )
    .map((point) => {
      const pointAsset = nextAssets.find((asset) =>
        (asset.data?.points ?? []).some(
          (assetPoint) => assetPoint.sitePointId === point.sitePointId
        )
      )

      return {
        ...point,
        siteAssetId: pointAsset?.siteAssetId,
        assetId: pointAsset?.assetId,
        assetName: pointAsset?.data?.name,
      }
    })
    .map((point, i) => ({
      ...point,
      data: pointsData[point.sitePointId],
      color:
        pointsData[point.sitePointId] != null
          ? colorPalette[i % colorPalette.length]
          : undefined,
    }))

  useEffect(() => {
    setAssets((prevAssets) => {
      const siteAssetsToAdd = siteAssetIds
        .filter(
          (siteAssetId) =>
            !prevAssets.some(
              (prevAsset) => prevAsset.siteAssetId === siteAssetId
            )
        )
        .map((siteAssetId) => createAsset(siteAssetId))
      // adding new site to beginning of array
      return [...siteAssetsToAdd, ...prevAssets].filter((prevAsset) =>
        siteAssetIds.includes(prevAsset.siteAssetId)
      )
    })
  }, [siteAssetIds])

  useEffect(() => {
    if (assets.length && assets.every((asset) => asset.data != null)) {
      const allSitePointIds = assets.flatMap((asset) =>
        asset.data.points.map((point) => point.sitePointId)
      )

      const nextSitePointIds = _([
        ...sitePointIds,
        ...points.map((point) => point.sitePointId),
      ])
        .filter((sitePointId) => allSitePointIds.includes(sitePointId))
        .uniq()
        .map((sitePointId) => createPoint(sitePointId))
        .value()

      setPoints(nextSitePointIds)
    }

    const siteIds = assets.map((asset) => asset.siteId)
    // Update timeZoneOption to use timeZoneId if the existing selected
    // timeZoneOption is site specific but that site is no longer selected.
    if (timeZoneOption?.siteId && !siteIds.includes(timeZoneOption?.siteId)) {
      setTimeZoneOption({ timeZoneId: timeZoneOption.timeZoneId })
    }
  }, [assets])

  useEffect(() => {
    const nextSitePointIds = points.map((point) => point.sitePointId)

    onSitePointIdsChange(nextSitePointIds)
  }, [points])

  useEffect(() => {
    setNow(dateTime.now().format())
  }, [times])

  function addSitePoint(sitePointId) {
    setPoints((prevPoints) => [...prevPoints, createPoint(sitePointId)])
  }

  function removeSitePoints(sitePointIds) {
    setPoints((prevPoints) =>
      prevPoints.filter(
        (prevPoint) => !sitePointIds.includes(prevPoint.sitePointId)
      )
    )
  }

  /** Adds the asset if it hasn't already been added to Time Series. */
  function upsertAsset(siteEquipmentId) {
    if (!state.siteEquipmentIds.includes(siteEquipmentId)) {
      setState((prevState) => ({
        ...prevState,
        siteEquipmentIds: [...prevState.siteEquipmentIds, siteEquipmentId],
      }))
    }
  }

  const context = {
    times,
    now,
    type,
    activeAssetId,
    granularity,
    quickRange,
    timeZoneOption,
    assets: nextAssets,
    points: nextPoints,
    loadingSitePointIds,
    removeSitePoints,
    setActiveAssetId,
    setAssets,
    state,
    selectedSiteIds,
    setSelectedSiteIds,
    setState,
    setTimeRange,
    setTimeZoneOption,
    timeZone,

    reset: () => {
      setPoints([])
      setState((prevState) => ({
        ...prevState,
        siteEquipmentIds: [],
        sitePointIds: [],
      }))
    },

    openClickedAsset(assetId) {
      if (activeAssetId === assetId) {
        setActiveAssetId()
      } else {
        setActiveAssetId(assetId)
      }
    },

    updateAsset(siteAssetId, data) {
      setActiveAssetId(data.id)
      // Auto-select points with hasFeaturedTags
      const priorityPoints = data.points.filter((x) => x.hasFeaturedTags)
      // If the user has really manually clicked on new asset in AssetSelector
      if (userSelectedEquipmentIds.some((x) => x === siteAssetId)) {
        if (priorityPoints.length > 0) {
          setPoints((prevPoints) => [
            ...prevPoints,
            ...priorityPoints.map((x) => createPoint(x.sitePointId)),
          ])
          for (let i = 0; i < priorityPoints.length; i++) {
            const priorityPoint = priorityPoints[i]
            const site = sites.find(
              (layoutSite) => layoutSite.id === priorityPoint.siteId
            )
            analytics.track('Point Selected', {
              name: priorityPoint.name,
              Site: site,
            })
          }
        }
      }
      removeUserSelectedEquipmentId(siteAssetId)

      setAssets((prevAssets) =>
        prevAssets.map((prevAsset) =>
          prevAsset.siteAssetId === siteAssetId
            ? {
                ...prevAsset,
                data,
              }
            : prevAsset
        )
      )
    },

    toggleSitePointId(sitePointId) {
      setPoints((prevPoints) =>
        prevPoints.some((prevPoint) => prevPoint.sitePointId === sitePointId)
          ? prevPoints.filter(
              (prevPoint) => prevPoint.sitePointId !== sitePointId
            )
          : [...prevPoints, createPoint(sitePointId)]
      )
    },

    updatePoint(sitePointId, data) {
      context.removeLoadingSitePointId(sitePointId)

      setPointsData((prevPointsData) => ({
        ...prevPointsData,
        [sitePointId]: data,
      }))
    },

    isPointsLoading() {
      return loadingSitePointIds.length > 0
    },

    addLoadingSitePointId(sitePointId) {
      setLoadingSitePointIds((prevLoadingSitePointIds) => [
        ...prevLoadingSitePointIds,
        sitePointId,
      ])
    },

    /** Adds the asset if it hasn't already been added to Time Series, otherwise removes it. */
    addOrRemoveAsset(siteEquipmentId) {
      if (state.siteEquipmentIds.includes(siteEquipmentId)) {
        removeUserSelectedEquipmentId(siteEquipmentId)
      } else {
        addUserSelectedEquipmentId(siteEquipmentId)
      }

      setState((prevState) => ({
        ...prevState,
        siteEquipmentIds: _.xor(prevState.siteEquipmentIds, [siteEquipmentId]),
      }))
    },

    /**
     * Adds the sensor if it hasn't already been added to Time Series, otherwise removes it.
     * Also first adds the asset to Time Series if it needs to be added.
     */
    addOrRemoveSensor(assetId, sensorId) {
      if (!state.sitePointIds.includes(sensorId)) {
        addSitePoint(sensorId)
        upsertAsset(assetId, true)
      } else {
        removeSitePoints([sensorId])
      }
    },

    removeLoadingSitePointId(sitePointId) {
      setLoadingSitePointIds((prevLoadingSitePointIds) =>
        prevLoadingSitePointIds.filter(
          (prevLoadingSitePointId) => prevLoadingSitePointId !== sitePointId
        )
      )
    },
  }

  return (
    <TimeSeriesContext.Provider value={context}>
      {children}
      <Assets />
      <Points />
    </TimeSeriesContext.Provider>
  )
}
