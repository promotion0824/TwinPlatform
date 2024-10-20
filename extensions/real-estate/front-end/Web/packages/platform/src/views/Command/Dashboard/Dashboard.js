import { useQuery, useInfiniteQuery } from 'react-query'
import _ from 'lodash'
import { useEffect, useState, useMemo, useRef } from 'react'
import { useHistory, useParams, Route } from 'react-router'

import { useAnalytics, useFeatureFlag, api, useDateTime } from '@willow/ui'
import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import { qs, priorities as insightPriorities } from '@willow/common'
import { Tab as TicketTab } from '@willow/common/ticketStatus'
import { useSite } from 'providers'
import { useGetTickets, useGetSiteStatistics } from 'hooks'
import useGetInsightFilters from '../../../hooks/Insight/useGetInsightFilters'
import BuildingDashboard from './Dashboard/BuildingDashboard'
import FloorViewer from './FloorViewer/FloorViewer'
import { DashboardContext } from './DashboardContext'
import routes from '../../../routes'
import {
  FilterOperator,
  statusMap,
} from '../../../services/Insight/InsightsService'
import BuildingHome from '../BuildingHome/BuildingHome'

export default function DashboardComponent({ site: siteProp }) {
  const history = useHistory()
  const params = useParams()
  const siteFromContext = useSite()
  const analytics = useAnalytics()
  const featureFlags = useFeatureFlag()
  const [hoverFloorId, setHoverFloorId] = useState()
  const [isHoverFloorWholeSite, setIsHoverFloorWholeSite] = useState(false)
  const [selectedCategories, setSelectedCategories] = useState([])
  const [selectedPriorities, setSelectedPriorities] = useState([])

  const site = siteProp || siteFromContext
  const isReadOnly = site.userRole !== 'admin' || qs.get('admin') !== 'true'

  const isInitialMount = useRef(true)
  const [{ days: daysFromQueryParams, search }, setSearchParams] =
    useMultipleSearchParams(['days', 'search'])
  const days = isInitialMount.current
    ? daysFromQueryParams ?? '7'
    : daysFromQueryParams

  const handleDateChange = (nextDays) => {
    isInitialMount.current = false
    setSearchParams({
      days: nextDays,
    })
  }

  useEffect(() => {
    if (isReadOnly && qs.get('admin') === 'true') {
      history.replace(history.location.pathname)
    }
  }, [])

  // Do not fetch list of insights/tickets and stats on floor page
  const isBuildingDashboard = !params.floors

  const dateTime = useDateTime()

  const occurredDate = days
    ? dateTime.now().addDays(-days).format('dateLocal')
    : undefined

  const insightFiltersQuery = useGetInsightFilters(
    {
      siteIds: [site.id],
      statusList: statusMap.default,
    },
    {
      enabled: !!site,
    }
  )

  const insightsQueryFilterSpec = useMemo(
    () => [
      {
        field: 'siteId',
        operator: FilterOperator.equalsLiteral,
        value: site.id,
      },
      {
        field: 'status',
        operator: FilterOperator.containedIn,
        value: statusMap.default,
      },
      {
        field: 'priority',
        operator: FilterOperator.containedIn,
        value:
          selectedPriorities.length === 0
            ? insightPriorities.map((p) => p.id.toString())
            : selectedPriorities,
      },
      ...(search
        ? [
            {
              field: 'name',
              operator: FilterOperator.like,
              value: search,
            },
          ]
        : []),
      ...(occurredDate
        ? [
            {
              field: 'LastOccurredDate',
              operator: FilterOperator.greaterThanOrEqual,
              value: occurredDate,
            },
          ]
        : []),
      ...(selectedCategories?.length > 0
        ? [
            {
              field: 'type',
              operator: FilterOperator.containedIn,
              value: selectedCategories,
            },
          ]
        : []),
    ],
    [occurredDate, search, selectedCategories, selectedPriorities, site.id]
  )
  const insightsQuery = useInfiniteQuery(
    [
      'infinite-insights-for-one-site',
      site.id,
      occurredDate,
      search,
      selectedCategories,
      selectedPriorities,
    ],
    async ({ pageParam = 1 }) => {
      const { data } = await api.post('/insights', {
        pageSize,
        page: pageParam,
        filterSpecifications: insightsQueryFilterSpec,
      })
      return data
    },
    {
      enabled: isBuildingDashboard,
      getNextPageParam: ({ after, total }) => {
        if (after === 0) {
          return undefined
        }
        return (total - after) / pageSize + 1
      },
    }
  )

  const insights = useMemo(
    () => insightsQuery.data?.pages?.flatMap((page) => page.items) ?? [],
    [insightsQuery.data]
  )

  const ticketsQuery = useGetTickets(
    { siteId: site.id, tab: TicketTab.open },
    { enabled: isBuildingDashboard }
  )
  const { data: insightsStat = [] } = useGetSiteStatistics(
    site.id,
    'insights',
    undefined,
    { enabled: isBuildingDashboard }
  )

  const { data: ticketsStat = [] } = useGetSiteStatistics(
    site.id,
    'tickets',
    undefined,
    { enabled: isBuildingDashboard }
  )

  const floorsQuery = useQuery(['floors', site.id], async () => {
    const { data } = await api.get(`/sites/${site.id}/floors`, {
      hasBaseModule: !site.features.isNonTenancyFloorsEnabled
        ? isReadOnly
        : false,
    })
    return data
  })

  const floorIdToMaxPriority = getMap(insights, 'floorId')
  const floorCodeToMaxPriority = getMap(insights, 'floorCode')

  const floorsWithPriority = useMemo(() => {
    const data = (floorsQuery.data ?? []).map((floor) => {
      const calculatedMaxPriority =
        floorIdToMaxPriority?.[floor.id] ?? floorCodeToMaxPriority?.[floor.code]

      return {
        ...floor,
        insightsMaxPriority: calculatedMaxPriority,
        insightsHighestPriority: calculatedMaxPriority,
      }
    })

    return data
  }, [floorCodeToMaxPriority, floorIdToMaxPriority, floorsQuery.data])

  const context = {
    isReadOnly,
    hoverFloorId,
    isHoverFloorWholeSite,

    setHoverFloor(floor) {
      setHoverFloorId(floor?.id)
      setIsHoverFloorWholeSite(
        floor?.name === 'BLDG' || floor?.name === 'SOFI CAMPUS OVERALL'
      )
    },

    setHoverFloorId,
    insightsQuery,
    insightTypeFilters: insightFiltersQuery.data?.filters?.insightTypes ?? [],
    searchInput: search,
    setSearchInput: setSearchParams,
    selectedCategories,
    setSelectedCategories,
    selectedPriorities,
    setSelectedPriorities,
    insights,
    insightsStat,
    ticketsQuery,
    ticketsStat,
    floors: floorsWithPriority,
    insightsQueryFilterSpec,
    site,
  }

  return (
    <DashboardContext.Provider value={context}>
      <>
        <Route
          path={[routes.sites__siteId(), routes.home_scope__scopeId()]}
          exact
        >
          <BuildingHome
            floors={floorsWithPriority}
            site={site}
            days={days ?? (isInitialMount.current ? '7' : undefined)}
            onDateChange={handleDateChange}
          />
        </Route>
        <Route
          path={[
            routes.dashboards_sites__siteId(),
            routes.dashboards_scope__scopeId(),
          ]}
          exact
        >
          <BuildingDashboard
            site={site}
            analytics={analytics}
            featureFlags={featureFlags}
          />
        </Route>
        <Route path={routes.sites__siteId_floors__floorId()}>
          <FloorViewer floors={floorsWithPriority} />
        </Route>
      </>
    </DashboardContext.Provider>
  )
}

/**
 * please refer to: packages\platform\src\views\Command\Dashboard\Dashboard.js
 * and packages\ui\src\components\Viewer3D\utils\index.ts and note that
 * 1: red is highest priority, 2: orange is priority ranked #2, 3: yellow is ranked #3
 * any priority that is less than 1 or greater than 3 will be treated as invalid and displayed as gray which is default color
 * */
const priorities = [1, 2, 3]

/**
 * transfer array of Insight to a map where key is either floorId or floorCode
 * and value is max priority of that floor
 */
const getMap = (insights, property) =>
  Object.fromEntries(
    Object.entries(_.groupBy(insights ?? [], (i) => i?.[property]))
      .map(mapFn)
      .filter(filterFn)
  )

const mapFn = ([key, insights]) => {
  const prioritiesFromInsights = insights.map((i) => i.priority)
  const customizedMaxPriority = priorities.find((priority) =>
    prioritiesFromInsights.includes(priority)
  )

  // where key is either floorId or floorCode, and value is max priority
  return [key, customizedMaxPriority]
}

// only priority 1, 2, 3 are valid, so filter out any other priority
const filterFn = ([_key, value]) => priorities.includes(value)

/**
 * This is an arbitrary number to temporarily deal with insight
 * endpoint timing out. At the time of writing, the endpoint timed out
 * when querying 9000 insights on DDK. The number was picked according
 * to manual testing.
 *
 * Note that his component will be completely removed
 * and replaced with new Home Page in the near future.
 *
 * TODO: remove this constant and the component itself once
 * https://dev.azure.com/willowdev/Unified/_workitems/edit/132321 is complete
 *
 * reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/133682
 */
const pageSize = 400
