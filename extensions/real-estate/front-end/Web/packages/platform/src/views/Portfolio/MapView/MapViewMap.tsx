/* eslint-disable complexity */
import _ from 'lodash'
import { css } from 'twin.macro'
import { Badge } from '@willowinc/ui'
import { useEffect, useMemo, useState, Fragment, useRef } from 'react'
import { useModelsOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import { useQuery, useInfiniteQuery } from 'react-query'
import { Map, Clusters, api, useIntersectionObserverRef } from '@willow/ui'
import getBounds from '../Map/Map/getBounds'
import GroupMarker from '../Map/Map/GroupMarker'
import MapViewMarker from './MapViewMarker'
import {
  isMapViewTwin,
  MapViewItem,
  MapViewPlaneStatus,
  MapViewSite,
  MapViewTwinType,
  passengerBoardingBridgesModelId,
  dockedRuleId,
  dockedButNotConnectedRuleId,
} from './types'
import {
  FilterOperator,
  fetchInsights,
  statusMap,
} from '../../../services/Insight/InsightsService'
import useOntologyInPlatform from '../../../hooks/useOntologyInPlatform'

/**
 * This is used only in POC for DFW, please refer to the following story for more details:
 * https://dev.azure.com/willowdev/Unified/_workitems/edit/90924
 *
 * the general idea of this POC is that:
 * - get all sites and their long/lat and put them on the map
 *   with marker (with icon of "Site")
 * - get all "Passenger Boarding Bridges" twins and their long/lat
 *   and put them on the map with a marker (with icon of "plane")
 * - fetch insights for all sites, group them by siteId and pass
 *   the array of insights to each site marker
 * - fetch insights for all "Passenger Boarding Bridges" twins,
 *   group them by twinId and pass the array of insights to each twin marker
 */
export default function MapViewMap({
  sites,
  twinTypes,
  legends,
  focalDateTime,
  selectedMarker,
  onMarkerClick,
}: {
  sites: MapViewSite[]
  twinTypes: MapViewTwinType[]
  legends: MapViewPlaneStatus[]
  focalDateTime: number
  selectedMarker?: MapViewItem
  onMarkerClick: (item?: MapViewItem) => void
}) {
  const containerRef = useRef()
  const { data: ontology, isLoading: isOntologyLoading } =
    useOntologyInPlatform()
  const {
    data: { items: modelsOfInterest } = {},
    isLoading: isModelsOfInterestLoading,
  } = useModelsOfInterest()
  const isPreparingTwinChips = isOntologyLoading || isModelsOfInterestLoading

  const mapViewInsightsQuery = useQuery(
    ['mapViewInsights'],
    async () => {
      const { data } = await api.get('/insights/mapview')
      return data
    },
    {
      select: (data) => {
        const insights = (data ?? [])
          .filter((i) => i?.twinId)
          .map((i) => ({
            ...i,
            occurredDate: new Date(i.occurredDate).valueOf(),
            // convert started/ended date of occurrences to number
            ...(i.occurrences
              ? {
                  occurrences: i.occurrences.map((o) => ({
                    ...o,
                    started: new Date(o.started).valueOf(),
                    ended: new Date(o.ended).valueOf(),
                  })),
                }
              : {}),
          }))
          .filter((i) =>
            // only show insights that are valid and faulted
            // and are active at the focal date timegit
            (i?.occurrences ?? []).some(
              (o) =>
                o.isValid &&
                o.isFaulted &&
                o.started <= focalDateTime.valueOf() &&
                o.ended >= focalDateTime.valueOf()
            )
          )

        return _.groupBy(insights, 'twinId')
      },
    }
  )

  const sitesInsightsQuery = useQuery(
    ['sitesInsights'],
    async () => {
      const data = await fetchInsights({
        specifications: {
          // only fetch active insights
          filterSpecifications: [
            {
              field: 'Status',
              operator: FilterOperator.containedIn,
              value: statusMap.default,
            },
          ],
        },
      })
      return data
    },
    {
      select: (data) =>
        _.groupBy(
          data.map((insight) => ({
            ...insight,
            occurredDate: new Date(insight.occurredDate).valueOf(),
          })),
          'siteId'
        ),
    }
  )

  // for this DFW POC, leadership arbitrarily
  // picked passenger boarding bridges model
  // to be dipicted as "plane" icon on the map
  const passengerBoardingBridgesQuery = useInfiniteQuery(
    ['passenger-boarding-bridges'],
    async ({ pageParam }) => {
      let response
      if (pageParam) {
        response = await api.get(pageParam)
      } else {
        response = await api.get('/twins/search', {
          params: {
            modelId: passengerBoardingBridgesModelId,
          },
        })
      }
      return response.data
    },
    {
      getNextPageParam: (lastPage) => lastPage.nextPage,
    }
  )
  const { status } = passengerBoardingBridgesQuery

  const twins = useMemo(
    () =>
      passengerBoardingBridgesQuery.data?.pages
        .flatMap((page) => page.twins)
        // map each twin to a new object with location and insights
        ?.map((twin) => {
          const { longitude, latitude } =
            JSON.parse(twin.rawTwin)?.customProperties?.coordinates ?? {}

          return {
            ...twin,
            location: longitude && latitude ? [longitude, latitude] : undefined,
            insights: mapViewInsightsQuery.data?.[twin.id],
          }
        })
        // if legends does not contains MapViewPlaneStatus.Docked, then filter out twins that has docked insights
        .filter(
          (twin) =>
            legends.includes(MapViewPlaneStatus.Docked) ||
            !twin?.insights?.some((i) => i.ruleId === dockedRuleId)
        )
        // if legends does not contains MapViewPlaneStatus.Undocked, then filter out twins that has no insights
        .filter(
          (twin) =>
            legends.includes(MapViewPlaneStatus.Undocked) ||
            twin?.insights?.length
        )
        .filter((twin) => twin?.location),
    [
      legends,
      mapViewInsightsQuery.data,
      passengerBoardingBridgesQuery.data?.pages,
    ]
  )

  // for this POC, we want to fetch all pbb twins
  const fetchAllRef = useIntersectionObserverRef(
    {
      onView: () => passengerBoardingBridgesQuery.fetchNextPage(),
    },
    [passengerBoardingBridgesQuery.data]
  )

  const [bounds, setBounds] = useState(() => getBounds(sites))

  const markerItems = useMemo(() => {
    const showMapViewSites = twinTypes.includes(MapViewTwinType.Buildings)
    const showMapViewInsights = twinTypes.includes(
      MapViewTwinType.PassengerBoardingBridges
    )
    const mapViewSites = showMapViewSites ? sites : []

    return [
      ...(sitesInsightsQuery.status !== 'success'
        ? mapViewSites
        : mapViewSites.map((site) => ({
            ...site,
            insights: sitesInsightsQuery.data[site.id]
              ? _.sortBy(sitesInsightsQuery.data[site.id], 'occurredDate')
              : undefined,
          }))),
      ...(status !== 'success' || !showMapViewInsights ? [] : twins ?? []),
    ]
  }, [
    sites,
    sitesInsightsQuery.data,
    sitesInsightsQuery.status,
    status,
    twinTypes,
    twins,
  ])

  // when marks change, we update bounds to
  // fit all markers on the map
  useEffect(() => {
    setBounds(getBounds(markerItems))
  }, [markerItems])

  // user selects something on map, it zooms in
  useEffect(() => {
    if (selectedMarker?.location) {
      setBounds([selectedMarker.location])
    }
  }, [selectedMarker?.location])

  const clustersFeatures = markerItems.map((item) => ({
    type: 'Feature',
    properties: item,
    geometry: {
      type: 'Point',
      coordinates: item.location,
    },
  }))

  const clusters = useMemo(
    () => (
      <Clusters features={clustersFeatures} onResize={_.noop}>
        {(features) =>
          features.map((feature) => {
            // arbitrary logic for this POC to only show faulted insights
            // where faulted insights are insights that are docked but not connected
            const faultedInsights = (
              feature.properties?.insights ?? []
            )?.filter((i) => i.ruleId === dockedButNotConnectedRuleId)

            const hasNoInsights =
              (feature.properties?.insights ?? []).length === 0
            const isMapViewTwinFeature = isMapViewTwin(feature.properties)
            const hasNotConnectedInsights = feature.properties?.insights?.some(
              (i) => i.ruleId === dockedButNotConnectedRuleId
            )

            /**
             * if a twin feature:
             * - if has no insights, then it's undocked
             * - if has insights, then it's docked
             * - if has insights and has faulted insights, then it's faulted
             *
             * if not a twin feature:
             * - if has no insights, then it's undocked
             * - if has insights, then it's faulted
             */
            const markerColorKey = isMapViewTwinFeature
              ? hasNoInsights
                ? MapViewPlaneStatus.Undocked
                : hasNotConnectedInsights
                ? MapViewPlaneStatus.Faulted
                : MapViewPlaneStatus.Docked
              : hasNoInsights
              ? MapViewPlaneStatus.Undocked
              : MapViewPlaneStatus.Faulted

            return (
              <Fragment
                key={
                  feature.properties.cluster_id
                    ? `cluster_id_${feature.properties.cluster_id}`
                    : feature.properties.id
                }
              >
                {feature.properties.cluster ? (
                  <GroupMarker feature={feature} />
                ) : (
                  <MapViewMarker
                    feature={feature}
                    icon={isMapViewTwinFeature ? 'plane' : 'site'}
                    onClick={onMarkerClick}
                    isSelected={selectedMarker?.id === feature.properties.id}
                    colorKey={markerColorKey}
                    count={
                      isMapViewTwinFeature
                        ? faultedInsights.length
                        : feature.properties?.insights?.length ?? 0
                    }
                    isLoading={
                      isPreparingTwinChips || isMapViewTwinFeature
                        ? mapViewInsightsQuery.status === 'loading' ||
                          status === 'loading'
                        : sitesInsightsQuery.status === 'loading'
                    }
                    ontology={ontology}
                    modelsOfInterest={modelsOfInterest}
                    headerChip={
                      markerColorKey === MapViewPlaneStatus.Docked && (
                        <Badge variant="dot" size="md" color="green">
                          Docked
                        </Badge>
                      )
                    }
                  />
                )}
              </Fragment>
            )
          })
        }
      </Clusters>
    ),
    [
      clustersFeatures,
      isPreparingTwinChips,
      mapViewInsightsQuery.status,
      modelsOfInterest,
      onMarkerClick,
      ontology,
      selectedMarker?.id,
      sitesInsightsQuery.status,
      status,
    ]
  )

  return (
    <div
      ref={fetchAllRef}
      tw="h-full"
      className="map-view-map"
      css={css(({ theme }) => ({
        '& .mapboxgl-popup-close-button': {
          ...theme.font.display.md.light,
          color: theme.color.neutral.fg.muted,
          '&:focus': {
            outline: 'none',
          },
          transform: 'translate(-10px, 10px)',
        },
        '&&& .mapboxgl-popup': {
          maxWidth: '450px',
          minWidth: '450px',
        },
      }))}
    >
      <Map bounds={bounds} containerRef={containerRef} key={selectedMarker?.id}>
        {clusters}
      </Map>
    </div>
  )
}
