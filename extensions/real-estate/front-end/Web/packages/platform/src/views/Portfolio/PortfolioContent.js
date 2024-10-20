import {
  ALL_LOCATIONS,
  ALL_SITES,
  ScopeSelectorWrapper,
  useAnalytics,
  useDateTime,
  useFeatureFlag,
  useScopeSelector,
  useUser,
} from '@willow/ui'
import { Stack } from '@willowinc/ui'
import { SiteSelect } from 'components/SiteSelect'
import _ from 'lodash'
import { useEffect, useState } from 'react'
import { useHistory, useLocation } from 'react-router'
import tw, { styled } from 'twin.macro'
import { useSite } from '../../providers'
import routes from '../../routes'
import { LayoutHeader } from '../Layout/index'
import { PortfolioContext } from './PortfolioContext'

/*
We respond to the following routes:

  `/`, `/portfolio` - Dashboards tab
  `/portfolio/twins` - the Twins tab

Structure of the portfolio page:

- We always have "Dashboards" tab; the Dashboards tab is always displayed.
- If enabled, a Twins tab is also displayed. To display the twins tab, a portfolio must
  have the "Twins Search enabled" ConfigCat parameter enabled, and also the
  wp-isKPIDashboard-enabled woggle toggle must be enabled.
*/

const defaultQuickRange = '1M'

export default function PortfolioContent({
  sites,
  buildingScores,
  isBuildingScoresLoading,
  setSearchParams,
  children,
  searchParams,
  dateRange,
}) {
  const dateTime = useDateTime()
  const siteContext = useSite()
  const scopeSelector = useScopeSelector()
  const user = useUser()
  const history = useHistory()
  const analytics = useAnalytics()
  const featureFlags = useFeatureFlag()

  const location = useLocation()
  const [selectedSite, setSelectedSite] = useState()
  const [baseMapSites, setBaseMapSites] = useState()
  const [selectedBuilding, setSelectedBuilding] = useState({
    id: null,
    name: ALL_SITES,
  })
  const [selectedCountry, setSelectedCountry] = useState()
  const [selectedStates, setSelectedStates] = useState([])
  const [selectedTypes, setSelectedTypes] = useState(['All Building Types'])
  const [selectedStatuses, setSelectedStatuses] = useState(['All Status'])
  const [search, setSearch] = useState('')
  const [selectedLocation, setSelectedLocation] = useState([])
  const [shouldFilterByMap, setShouldFilterByMap] = useState(false)
  const [shouldResetMapBounds, setShouldResetMapBounds] = useState(false)
  const [shouldUpdateBaseMapSites, setShouldUpdateBaseMapSites] = useState(true)
  const [siteIdsOnMap, setSiteIdsOnMap] = useState([])
  const [submenuCategory, setSubmenuCategory] = useState('operational')
  const {
    category,
    selectedDashboard,
    quickOptionSelected = defaultQuickRange,
    selectedDayRange = user?.options?.selectedDayRange,
    selectedBusinessHourRange = user?.options?.selectedBusinessHourRange,
  } = searchParams

  const selectedScopeIds =
    scopeSelector.location?.twin.id === ALL_LOCATIONS
      ? []
      : [
          scopeSelector.location?.twin.siteId,
          ...scopeSelector.descendantSiteIds,
        ]
          // Currently campuses may have dummy site IDs that need to be filtered out.
          // These should be removed in the future.
          .filter((id) => id !== '00000000-0000-0000-0000-000000000000')

  // the following effect block is helpful for updating the baseMapSites when
  // pathname changes so when user clicks back or forward on browser, the sites
  // on the map will be updated accordingly
  useEffect(() => {
    setShouldUpdateBaseMapSites(true)
  }, [location.pathname, scopeSelector?.twinQuery?.data])

  const filteredSites = _(sites)
    .filter(
      (site) =>
        !scopeSelector.isScopeSelectorEnabled ||
        !selectedScopeIds.length ||
        selectedScopeIds.includes(site.id)
    )
    .filter((site) => {
      if (selectedBuilding.id === null) return true
      return selectedBuilding.id === site.id
    })
    .filter((site) => {
      if (selectedCountry === 'Worldwide') {
        return true
      }
      return (
        selectedBuilding.id === site.id ||
        selectedCountry == null ||
        site.country === selectedCountry
      )
    })
    .filter(
      (site) =>
        selectedStates.length === 0 || selectedStates.includes(site.state)
    )
    .filter(
      (site) =>
        selectedLocation.length === 0 ||
        site.state === selectedLocation[selectedLocation.length - 1] ||
        site.country === selectedLocation[selectedLocation.length - 1]
    )
    .filter((site) => {
      if (selectedTypes.includes('All Building Types')) {
        return true
      }
      return (
        selectedBuilding.id === site.id ||
        selectedTypes.length === 0 ||
        selectedTypes.includes(site.type)
      )
    })
    .filter((site) => {
      if (selectedStatuses.includes('All Status')) {
        return true
      }
      return (
        selectedBuilding.id === site.id ||
        selectedStatuses.length === 0 ||
        selectedStatuses.includes(site.status)
      )
    })
    .filter((site) => site.name.toLowerCase().includes(search.toLowerCase()))
    .filter((site) => {
      if (!shouldFilterByMap || shouldUpdateBaseMapSites) return true
      return siteIdsOnMap.includes(site.id)
    })

    .value()

  const filteredSiteIds = filteredSites.map((site) => site.id)

  if (shouldUpdateBaseMapSites) {
    setShouldFilterByMap(false)
    setShouldUpdateBaseMapSites(false)
    setBaseMapSites(filteredSites)
  }

  function selectSite(site) {
    setSelectedSite({ ...site })
  }

  function toggleBuilding(building) {
    setShouldUpdateBaseMapSites(true)

    let shouldReset

    setSelectedBuilding((preBuilding) => {
      shouldReset = building.id === null || preBuilding.id === building.id
      // Click on "All Building" or any single building will resets all filters
      setSelectedCountry(() => 'Worldwide')
      setSelectedTypes(() => ['All Building Types'])
      setSelectedLocation(() => [])
      setSelectedStatuses(() => ['All Status'])

      return shouldReset ? { id: null, name: ALL_SITES } : building
    })
    if (shouldReset) {
      user.saveOptions('siteId', null)
    } else {
      user.saveOptions('siteId', building.id)
    }
  }

  function toggleCountry(country) {
    setShouldUpdateBaseMapSites(true)
    setSelectedCountry((prevCountry) =>
      prevCountry !== country ? country : 'Worldwide'
    )
  }

  function toggleState(state) {
    setShouldUpdateBaseMapSites(true)
    setSelectedStates((prevStates) => _.xor(prevStates, [state]))
  }

  // location: array consists of locations lead to current location ["a_country", "a_state"]
  function toggleLocation(newLocation, locationLevel) {
    setShouldUpdateBaseMapSites(true)
    if (newLocation === 'Worldwide') {
      setSelectedLocation(() => [])
    } else {
      setSelectedLocation((prevLocation) =>
        prevLocation.slice(-1)[0] === newLocation
          ? [...prevLocation.slice(0, locationLevel)]
          : [...prevLocation.slice(0, locationLevel), newLocation]
      )
    }
    setSelectedBuilding({ id: null, name: ALL_SITES })
  }

  /**
   *
   * @param {Object} locationObj worldwide object, {country1: {state1: {...}, state2:{...}}}
   * @param {Array} locations the location array ["country", "state"] stored in selectedLocation
   * @param {Int} level country is at level 2, state is at level 2
   * @returns {Array} the array of location items to be displayed at specific level
   *
   * any location array can have 3 stats (#1 - #3):
   * #1: empty array (current location is at lower level where selectedLocation.length < level - 1)
   * #2: array contains all keys at that level (selectedLocation.length === level - 1 or selectedLocation.slice(-1)[0] has no children)
   * #3: array contains 1 key at that level, and its value being selectedLocation.slice(level - 1, level)
   * #4: using getLocationObj util function and selectedLocation array, we can find out children list of any location in selectedLocation
   * #5: 1st level location list will have at least 1 item because by default we will always show full 1st level location list
   */
  function getLocationList(locationObj, locations, level) {
    const getLocationObj = (arr, obj) =>
      arr.reduce((prev, cur) => prev[cur], obj)
    let locationList
    switch (locations.length) {
      case level - 1:
        locationList = Object.keys(
          getLocationObj(locations.slice(0, level - 1), locationObj)
        )
        break
      case level:
        if (
          Object.keys(getLocationObj(locations.slice(0, level), locationObj))
            .length
        ) {
          locationList = locations.slice(level - 1, level)
          break
        } else {
          locationList = Object.keys(
            getLocationObj(locations.slice(0, level - 1), locationObj)
          )
          break
        }
      default:
        if (locations.length < level) {
          locationList =
            level === 1
              ? Object.keys(
                  getLocationObj(locations.slice(0, level), locationObj)
                )
              : []
        } else {
          locationList = locations.slice(level - 1, level)
        }
    }
    return locationList
  }

  function getPadding(locationObj, locations, level) {
    const childrenLocation = locations
      .slice(0, level)
      .reduce((prev, cur) => prev[cur], locationObj)
    if (
      locations.length === level - 1 ||
      (locations.length === level && Object.keys(childrenLocation).length === 0)
    ) {
      return 'large'
    }
    return ''
  }

  function toggleType(type) {
    setShouldUpdateBaseMapSites(true)

    // type is multi-selectable while first entry is All Building Type (resets when clicked)
    if (type === 'All Building Types') {
      setSelectedTypes(() => ['All Building Types'])
    } else {
      setSelectedTypes((prevTypes) =>
        _.xor(prevTypes, [type]).filter(
          (typeName) => typeName !== 'All Building Types'
        )
      )
      /* when all items are checked off, All Building Types are selected */
      setSelectedTypes((prevTypes) =>
        prevTypes.length === 0 ? ['All Building Types'] : prevTypes
      )
    }
    setSelectedBuilding({ id: null, name: ALL_SITES })
  }

  function toggleStatus(status) {
    setShouldUpdateBaseMapSites(true)

    // status is multi-selectable while first entry is Status (resets when clicked)
    if (status === 'All Status') {
      setSelectedStatuses(() => ['All Status'])
    } else {
      setSelectedStatuses((prevStatuses) =>
        _.xor(prevStatuses, [status]).filter(
          (statusName) => statusName !== 'All Status'
        )
      )
      /* when all items are checked off, All Status is auto selected */
      setSelectedStatuses((prevStatuses) =>
        prevStatuses.length === 0 ? ['All Status'] : prevStatuses
      )
    }
    setSelectedBuilding({ id: null, name: ALL_SITES })
  }
  function isDefaultFilter() {
    return (
      selectedBuilding.id === null &&
      selectedTypes[0] === 'All Building Types' &&
      selectedStatuses[0] === 'All Status' &&
      selectedLocation.length === 0
    )
  }

  function handleDashboardReportClick(nextDashboardReport) {
    setSearchParams({
      selectedDashboard: nextDashboardReport.name,
    })
  }
  function handleReportSelection(newReport, newCategory) {
    setSearchParams({
      category: newCategory,
      selectedDashboard: newReport.name,
    })
  }
  function handleDateRangeChange(newDateRange, newQuickOption) {
    analytics?.track('Date Range Filter Clicked', {
      customer_name: user.customer.name,
      date_range_filter: quickOptionSelected,
      ...(selectedBuilding.id == null
        ? {}
        : {
            site: selectedBuilding,
          }),
    })
    // Set date to localTimezone so the datepicker date and url param date are same.
    setSearchParams({
      startDate: dateTime(newDateRange[0]).format('dateTimeLocal'),
      endDate: dateTime(newDateRange[1]).format('dateTimeLocal'),
      quickOptionSelected: newQuickOption,
    })
  }
  function handleQuickOptionChange(newQuickOption) {
    setSearchParams({ quickOptionSelected: newQuickOption })
  }

  function handleDayRangeChange(newSelectedDayRange) {
    setSearchParams({
      startDate: dateRange[0],
      endDate: dateRange[1],
      selectedDayRange: newSelectedDayRange,
    })
  }

  function handleBusinessHourChange(newBusinessHour) {
    setSearchParams({
      startDate: dateRange[0],
      endDate: dateRange[1],
      selectedBusinessHourRange: newBusinessHour,
    })
  }

  function handleResetClick() {
    user.saveOptions('selectedDayRange', undefined)
    user.saveOptions('selectedBusinessHourRange', undefined)
    setSearchParams({
      startDate: undefined,
      endDate: undefined,
      selectedDayRange: undefined,
      selectedBusinessHourRange: undefined,
      quickOptionSelected: undefined,
    })
  }

  function resetFilters() {
    toggleLocation('Worldwide')
    toggleType('All Building Types')
    toggleStatus('All Status')
    updateSearch('')
  }

  function updateSearch(newSearch) {
    setShouldUpdateBaseMapSites(true)
    setSearch(newSearch)
  }

  function handleResetMapClick() {
    document.querySelector('.mapboxgl-popup')?.remove()
    setShouldResetMapBounds(true)
    selectSite(undefined)
  }

  const context = {
    baseMapSites,
    selectedBuilding,
    selectedSite,
    selectedCountry,
    selectedStates,
    selectedLocation,
    selectedTypes,
    selectedStatuses,
    search,
    setShouldFilterByMap,
    setShouldResetMapBounds,
    shouldFilterByMap,
    shouldResetMapBounds,
    submenuCategory,
    sites,
    filteredSites,
    filteredSiteIds,
    category,
    selectedDashboard,

    buildingScores,
    isBuildingScoresLoading,

    dateRange,
    setSearchParams,
    setSiteIdsOnMap,
    quickOptionSelected,
    selectedDayRange,
    selectedBusinessHourRange,
    selectSite,
    toggleBuilding,
    toggleCountry,
    toggleState,
    toggleLocation,
    getLocationList,
    getPadding,
    toggleType,
    toggleStatus,
    setSearch,
    updateSearch,
    isDefaultFilter,
    setSubmenuCategory,
    handleDashboardReportClick,
    handleReportSelection,
    handleDateRangeChange,
    handleQuickOptionChange,
    handleDayRangeChange,
    handleBusinessHourChange,
    handleResetClick,
    resetFilters,
    handleResetMapClick,
  }

  const {
    location: { pathname },
    push,
  } = history

  const handleSiteChange = (site) => {
    context.toggleBuilding(site)
    analytics.track('Site Select', { site, customer: user?.customer ?? {} })

    if (pathname.startsWith(routes.portfolio_reports)) {
      push(
        site?.id != null
          ? routes.portfolio_reports__siteId(site.id)
          : routes.portfolio_reports
      )
    } else if (pathname === routes.home) {
      const nextSearchParams = new URLSearchParams(location.search)
      // remove this search param to avoid case where when navigating to site routes,
      // selectedDashboard is available to Portfolio but not to Site
      nextSearchParams.delete('selectedDashboard')
      // remove this search param to avoid edge case where the category of dashboard
      // exist on Portfolio level (site.id == null) while it does not exist on
      // Site level
      nextSearchParams.delete('category')
      push({
        pathname:
          site?.id != null ? routes.sites__siteId(site.id) : routes.home,
        search: nextSearchParams.toString(),
      })
    } else if (
      pathname === routes.dashboards ||
      pathname === routes.dashboards_sites__siteId(siteContext.id)
    ) {
      push(
        site.id === null
          ? routes.dashboards
          : routes.dashboards_sites__siteId(site.id)
      )
    }
  }

  const handleLocationChange = (loc) => {
    const { twin } = loc

    // if the scope user picked is not a building, it means user will
    // still be viewing portfolio dashboards, so we keep the search params
    const isNextScopeUsedAsBuilding = scopeSelector.isScopeUsedAsBuilding(loc)
    const nextStringSearchParams = new URLSearchParams(
      location.search
    ).toString()

    const isReportsRoute = pathname.startsWith(routes.reports)
    const isDashboardRoute = pathname.startsWith(routes.dashboards)

    setShouldUpdateBaseMapSites(true)
    if (twin.id === ALL_LOCATIONS) {
      push({
        pathname: isReportsRoute
          ? routes.reports
          : isDashboardRoute
          ? routes.dashboards
          : routes.home,
        search:
          isDashboardRoute && !isNextScopeUsedAsBuilding
            ? nextStringSearchParams
            : '',
      })
    } else {
      push({
        pathname: isReportsRoute
          ? routes.reports_scope__scopeId(twin.id)
          : isDashboardRoute
          ? routes.dashboards_scope__scopeId(twin.id)
          : routes.home_scope__scopeId(twin.id),
        search:
          isDashboardRoute && !isNextScopeUsedAsBuilding
            ? nextStringSearchParams
            : '',
      })
    }
  }

  return (
    <PortfolioContext.Provider value={context}>
      <Stack>
        <LayoutHeader>
          {!history.location.pathname.startsWith(routes.portfolio_twins) &&
            /*
                SiteSelect is not relevant on the legacy Portfolio Page (legacy landing page)
              */
            history.location.pathname !== routes.portfolio && (
              <FlexContainer>
                {featureFlags.hasFeatureToggle('scopeSelector') ? (
                  <ScopeSelectorWrapper
                    onLocationChange={handleLocationChange}
                  />
                ) : (
                  <SiteSelect
                    sites={sites}
                    value={
                      history.location.pathname === routes.home
                        ? selectedBuilding
                        : location.pathname === routes.dashboards ||
                          location.pathname === routes.portfolio_reports
                        ? { id: null }
                        : siteContext
                    }
                    onChange={handleSiteChange}
                  />
                )}
              </FlexContainer>
            )}
        </LayoutHeader>
        {children}
      </Stack>
    </PortfolioContext.Provider>
  )
}

const FlexContainer = tw.div`h-full flex flex-col justify-center`

export const NoFilterText = styled.div(({ theme }) => ({
  fontWeight: '600',
  lineHeight: '20px',
  color: theme.color.neutral.fg.subtle,
  padding: '32px 16px',
}))
