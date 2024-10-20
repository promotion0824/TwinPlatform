import { useCallback, useMemo } from 'react'
import _ from 'lodash'
import { useAnalytics } from '@willow/ui'

/**
 * Analytics functions for the twin explorer.
 */
export default function useTwinAnalytics() {
  const analytics = useAnalytics()

  // Remove null/undefined values from the payloads; filtering by "is set" in
  // Mixpanel still returns fields with null values.
  const track = useCallback(
    (eventName, props) => {
      analytics.track(eventName, {
        ..._.pickBy(props, (val) => val != null),
      })
    },
    [analytics]
  )

  return useMemo(
    () => ({
      trackLandingPage: () => {
        track('Twin Explorer Landing Page')
      },
      trackTwinSearch: ({ term, ...others }) => {
        // In all these methods we normalise term from empty string to undefined
        // so it will be removed and then can be filtered out in Mixpanel by
        // filtering by "is set".
        track('Twin Search', {
          term: term || undefined,
          ...others,
        })
      },
      trackTwinSearchResults: ({ term, ...others }) => {
        track('Twin Search Results', {
          term: term || undefined,
          ...others,
        })
      },
      trackTwinSearchResultsAdditionalPage: ({ term, ...others }) => {
        track('Twin Search Results, additional page', {
          term: term || undefined,
          ...others,
        })
      },
      trackNoSearchResults: ({ term, ...others }) => {
        track('No Twins Found', { term: term || undefined, ...others })
      },
      trackRegisterInterest: ({ term, ...others }) => {
        track('Interest for More Twins', {
          term: term || undefined,
          ...others,
        })
      },
      trackDisplayChange: ({ term, ...others }) => {
        track('Search Results Display Change', {
          term: term || undefined,
          ...others,
        })
      },
      trackTimeSeriesViewed: (twin) =>
        track('Time Series Viewed', {
          twin,
          context: 'Twin View',
        }),
      trackThreeDimensionViewed: (twin) =>
        track('Three Dimension Viewed', {
          twin,
        }),
      trackRelationsMapViewed: (twin) =>
        track('Relations Map Viewed', { twin }),
      trackSummaryViewed: ({ twin }) => {
        track('Twin Summary Viewed', { twin })
      },
      trackRelatedTwinsViewed: ({ twin, count }) => {
        track('Related Twins Viewed', { twin, count })
      },
      trackTwinExportAll: () => {
        track('Twin Export', { all: true })
      },
      trackTwinExport: ({ count }) => {
        track('Twin Export', { count })
      },
      trackRelatedTwinsRelationshipTypeChanged: ({ type }) => {
        track('Related Twins Relationship Type Filter', { type })
      },
      trackRelatedTwinsNameFilterTyped: () => {
        track('Related Twins Name Filter')
      },
      trackTwinFilesViewed: ({ twin }) => {
        track('Twin Files Viewed', { twin })
      },
      trackFileDownloaded: ({ twin }) => {
        track('File Downloaded', { twin })
      },
      trackSensorsViewed: ({ twin, count }) => {
        track('Sensors Viewed', { twin, count })
      },
      trackGrowRelationsMap: () => {
        track('Grow Relations Map')
      },
      trackShrinkRelationsMap: () => {
        track('Shrink Relations Map')
      },
      trackRelatedTwinAction: ({ option, twin }) => {
        track('Related Twin Action', { option, twin })
      },
      trackTwinViewedViaRelationsMap: () => {
        track('Twin Viewed via Relations Map')
      },
    }),
    [track]
  )
}
