import {
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState,
  useEffect,
} from 'react'
import { TFunction, useTranslation } from 'react-i18next'
import {
  useAnalytics,
  useDateTime,
  useScopeSelector,
  useUser,
} from '@willow/ui'
import { getDateTimeRange } from '@willow/ui/components/DatePicker/DatePicker/QuickRangeOptions'
import { useHistory } from 'react-router'
import { Site } from '@willow/common/site/site/types'
import useGetAssetHistory, {
  AssetHistory,
  AssetHistoryType,
  isAssetHistoryInsight,
  isAssetHistoryTicket,
} from '../hooks/useGetAssetHistory'
import routes from '../../../../../../routes'
import { useSites } from '../../../../../../providers'
import { User } from '../../../../KPIDashboards/HeaderControls/HeaderControls'

export type FilterType = 'all' | AssetHistoryType

export interface AssetHistoryContextParams {
  filterType: FilterType
  user: User
  sites: Site[]
  t: TFunction
  dateTime: ReturnType<typeof useDateTime>
  setFilterType: (filterType: FilterType) => void
  filterDateRange: [string, string]
  handleDateRangeChange: (nextDateRange: [string, string]) => void
  dateRangePickerOptions: {
    [key: string]: { label: string; handleDateRangePick: () => void }
  }
  typeOptions: Record<FilterType, string>
  assetHistory: AssetHistory[]
  assetHistoryQueryStatus: string
  selectedItem: AssetHistory | undefined
  setSelectedItem: (item: AssetHistory | undefined) => void
}

const AssetHistoryContext = createContext<
  AssetHistoryContextParams | undefined
>(undefined)

export function useAssetHistory() {
  const context = useContext(AssetHistoryContext)
  if (context == null) {
    throw new Error('useAssetHistory needs a AssetHistoryProvider')
  }
  return context
}

export default function AssetHistoryProvider({
  siteId,
  assetId,
  twinId,
  filterType,
  children,
  setFilterType,
  setInsightId,
}: {
  siteId: string
  assetId: string
  twinId: string
  filterType: FilterType
  children: JSX.Element
  setFilterType: (filterType: FilterType) => void
  setInsightId: (insightId?: string) => void
}) {
  const { isScopeSelectorEnabled, location } = useScopeSelector()
  const user = useUser()
  const sites = useSites()
  const { t } = useTranslation()
  const dateTime = useDateTime()
  const history = useHistory()

  function getDefaultDateRange(): [string, string] {
    const defaultQuickRange = '1Y'
    return getDateTimeRange(dateTime.now(), defaultQuickRange)
  }

  const [filterDateRange, setFilterDateRange] = useState<[string, string]>(() =>
    getDefaultDateRange()
  )
  function handleDateRangeChange(nextDateRange) {
    setFilterDateRange(
      nextDateRange.length === 0 ? getDefaultDateRange() : nextDateRange
    )
  }
  const analytics = useAnalytics()
  const typeOptions = {
    all: t('plainText.allTypes'),
    insight: t('headers.insight'),
    inspection: t('plainText.inspection'),
    scheduledTicket: t('plainText.scheduledTicket'),
    standardTicket: t('plainText.standardTicket'),
  }

  function handleDateRangePick(fn) {
    const now = dateTime.now()
    setFilterDateRange([fn(now).format(), now.format()])
  }

  const dateRangePickerOptions = {
    lastHour: {
      label: t('plainText.lastHour'),
      handleDateRangePick: () => handleDateRangePick((now) => now.addHours(-1)),
    },
    last4Hours: {
      label: t('plainText.last4Hours'),
      handleDateRangePick: () => handleDateRangePick((now) => now.addHours(-4)),
    },
    last24Hours: {
      label: t('plainText.last24Hours'),
      handleDateRangePick: () =>
        handleDateRangePick((now) => now.addHours(-24)),
    },
    last48Hours: {
      label: t('plainText.last48Hours'),
      handleDateRangePick: () =>
        handleDateRangePick((now) => now.addHours(-48)),
    },
    last7Days: {
      label: t('plainText.last7Days'),
      handleDateRangePick: () => handleDateRangePick((now) => now.addDays(-7)),
    },
    last30Days: {
      label: t('plainText.last30Days'),
      handleDateRangePick: () => handleDateRangePick((now) => now.addDays(-30)),
    },
    last90Days: {
      label: t('plainText.last90Days'),
      handleDateRangePick: () => handleDateRangePick((now) => now.addDays(-90)),
    },
    last12Months: {
      label: t('plainText.last12Months'),
      handleDateRangePick: () =>
        handleDateRangePick((now) => now.addMonths(-12)),
    },
  }

  const site = sites.find((s) => s.id === siteId)
  const { assetHistory, status } = useGetAssetHistory({
    siteId,
    assetId,
    twinId,
    options: {},
    isInsightsDisabled: !site || site.features.isInsightsDisabled,
    isTicketingDisabled: !site || site.features.isTicketingDisabled,
    isInspectionEnabled: site && site.features.isInspectionEnabled,
    isScheduledTicketsEnabled:
      site &&
      site.features.isInspectionEnabled &&
      site.features.isScheduledTicketsEnabled,
    timeZone: site?.timeZone,
  })

  const filteredAssetHistory = useMemo(() => {
    const startDate = new Date(filterDateRange[0]).valueOf()
    const endDate = new Date(filterDateRange[1]).valueOf()

    return (
      assetHistory
        // Filter records based on filterType
        .filter(
          ({ assetHistoryType }) =>
            filterType === 'all' || filterType === assetHistoryType
        )
        // Filter records based on date range
        .filter((item) => {
          // The date we use to filter by depends on the kind of item.
          // Note, we have various versions of this logic in several places, eg.
          // `packages/platform/src/views/Command/Dashboard/FloorViewer/Floor/SidePanel/Content/AssetHistory/HistoryTable.js`
          // and we really should consolidate these.
          const dateStr =
            item.date != null
              ? item.date
              : isAssetHistoryTicket(item)
              ? item.closedDate ?? item.updatedDate
              : isAssetHistoryInsight(item)
              ? item.updatedDate
              : null
          if (dateStr != null) {
            const dateValue = new Date(dateStr).valueOf()
            return startDate <= dateValue && dateValue <= endDate
          } else {
            return false
          }
        })
        .sort(
          (a, b) =>
            // sort from latest to oldest
            new Date(b.date as string).valueOf() -
            new Date(a.date as string).valueOf()
        )
    )
  }, [assetHistory, filterType, filterDateRange])

  // This is used for when user selects an item in the asset history table
  // and the modal appears with the item's detail.
  const [selectedItem, _setSelectedItem] = useState<AssetHistory>()

  const setSelectedItem = useCallback(
    (item: AssetHistory | undefined) => {
      if (item != undefined) {
        const types = {
          standardTicket: 'Ticket',
          scheduledTicket: 'Ticket',
          inspection: 'Inspection',
          insight: 'Insight',
        }
        const type = types[item.assetHistoryType] ?? item.assetHistoryType
        analytics.track(`Asset History ${type} Viewed`, {
          itemId: item.ID,
          siteId,
          assetId,
        })
      }

      // If the item is an insight, we want to navigate to the insight node page instead of showing the modal
      if (item?.assetHistoryType === 'insight') {
        history.push(
          isScopeSelectorEnabled && location?.twin?.id
            ? routes.insights_scope__scopeId_insight__insightId(
                location.twin.id,
                item.id
              )
            : routes.sites__siteId_insights__insightId(siteId, item.id)
        )
      } else {
        _setSelectedItem(item)
        setInsightId(undefined)
      }
    },
    [
      analytics,
      assetId,
      history,
      isScopeSelectorEnabled,
      location?.twin?.id,
      setInsightId,
      siteId,
    ]
  )

  useEffect(() => {
    analytics.track('Asset History Viewed', { siteId, assetId })
  }, [analytics, siteId, assetId])

  return (
    <AssetHistoryContext.Provider
      value={{
        filterType,
        setFilterType,
        filterDateRange,
        handleDateRangeChange,
        dateRangePickerOptions,
        typeOptions,
        assetHistory: filteredAssetHistory,
        assetHistoryQueryStatus: status,
        selectedItem,
        setSelectedItem,
        user,
        sites,
        t,
        dateTime,
      }}
    >
      {children}
    </AssetHistoryContext.Provider>
  )
}
