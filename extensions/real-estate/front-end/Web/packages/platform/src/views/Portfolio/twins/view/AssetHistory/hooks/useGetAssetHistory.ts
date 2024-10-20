import { useMemo } from 'react'
import { useQuery, UseQueryOptions } from 'react-query'
import { useTicketStatuses } from '@willow/common'
import {
  isTicketStatusEquates,
  Status as TicketStatus,
} from '@willow/common/ticketStatus'
import { Insight } from '@willow/common/insights/insights/types'
import {
  getInspections,
  InspectionsResponse,
  Inspection,
} from '../../../../../../services/Inspections/InspectionsServices'
import {
  FilterOperator,
  InsightsResponse,
  fetchAssetInsights,
} from '../../../../../../services/Insight/InsightsService'
import {
  getAssetTicketsHistory,
  TicketsResponse,
  TicketSimpleDto,
} from '../../../../../../services/Tickets/TicketsService'

type AssetHistoryResponse = {
  insights: InsightsResponse
  standardTickets: TicketsResponse
  scheduledTickets: TicketsResponse
  inspections: InspectionsResponse
}
export type UseGetAssetHistoryParams = {
  siteId: string
  assetId: string
  twinId: string
  isTicketingDisabled?: boolean
  isInsightsDisabled?: boolean
  isInspectionEnabled?: boolean
  isScheduledTicketsEnabled?: boolean
  timeZone?: string
  options?: UseQueryOptions<
    AssetHistoryResponse,
    unknown,
    AssetHistoryResponse,
    string[]
  >
}

export type AssetHistoryType =
  | 'insight'
  | 'inspection'
  | 'standardTicket'
  | 'scheduledTicket'

/**
 * Attributes that exist on all asset history items
 */
type AssetHistoryItemBase = {
  ID: string
  date?: string
}

type AssetHistoryTicket = {
  assetHistoryType: 'standardTicket' | 'scheduledTicket'
  status?: TicketStatus
} & AssetHistoryItemBase &
  TicketSimpleDto

type AssetHistoryInsight = {
  assetHistoryType: 'insight'
  timeZone?: string
} & AssetHistoryItemBase &
  Insight

type AssetHistoryInspection = {
  assetHistoryType: 'inspection'
} & AssetHistoryItemBase &
  Inspection & {
    status: Inspection['checkRecordSummaryStatus']
  }

/**
 * An item in an asset history list - may be a ticket, insight, or inspection.
 */
export type AssetHistory =
  | AssetHistoryTicket
  | AssetHistoryInsight
  | AssetHistoryInspection

/**
 * Type predicate to check if the asset history item is a ticket
 */
export function isAssetHistoryTicket(
  item: AssetHistory
): item is AssetHistoryTicket {
  return (
    item.assetHistoryType === 'standardTicket' ||
    item.assetHistoryType === 'scheduledTicket'
  )
}

/**
 * Type predicate to check if the asset history item is an insight
 */
export function isAssetHistoryInsight(
  item: AssetHistory
): item is AssetHistoryInsight {
  return item.assetHistoryType === 'insight'
}

/**
 *  This hook will fetch all required data for Asset History:
 *  insights, standard tickets, scheduled tickets, and inspections.
 *  and also, manipulate data to be better suited for the Asset history table.
 */
export default function useGetAssetHistory({
  siteId,
  assetId,
  twinId,
  isTicketingDisabled,
  isInsightsDisabled,
  isInspectionEnabled,
  isScheduledTicketsEnabled,
  timeZone,
  options,
}: UseGetAssetHistoryParams) {
  const assetHistoryQuery = useQuery(
    ['asset history', siteId, assetId, twinId],
    () =>
      getAssetHistory({
        siteId,
        assetId,
        twinId,
        isTicketingDisabled,
        isInsightsDisabled,
        isInspectionEnabled,
        isScheduledTicketsEnabled,
      }),
    options
  )
  const ticketStatuses = useTicketStatuses()

  return useMemo(() => {
    const { insights, standardTickets, scheduledTickets, inspections } =
      assetHistoryQuery.data || {}

    const assetHistory: AssetHistory[] =
      assetHistoryQuery.data == null
        ? []
        : [
            ...(insights || []).map(
              (insight): AssetHistory => ({
                ...insight,
                assetHistoryType: 'insight',
                date: insight.updatedDate,
                ID: insight.sequenceNumber ?? '',
                timeZone,
              })
            ),
            ...(standardTickets || []).map((standardTicket): AssetHistory => {
              const ticketStatus = ticketStatuses.getByStatusCode(
                standardTicket.statusCode
              )
              return {
                ...standardTicket,
                assetHistoryType: 'standardTicket',
                status: ticketStatus?.status,
                date:
                  ticketStatus &&
                  isTicketStatusEquates(ticketStatus, TicketStatus.closed)
                    ? standardTicket.closedDate
                    : standardTicket.updatedDate,
                ID: standardTicket.sequenceNumber ?? '',
              }
            }),
            ...(scheduledTickets || []).map((scheduledTicket): AssetHistory => {
              const ticketStatus = ticketStatuses.getByStatusCode(
                scheduledTicket.statusCode
              )
              return {
                ...scheduledTicket,
                assetHistoryType: 'scheduledTicket',
                status: ticketStatus?.status,
                date:
                  ticketStatus &&
                  isTicketStatusEquates(ticketStatus, TicketStatus.closed)
                    ? scheduledTicket.closedDate
                    : scheduledTicket.updatedDate,
                ID: scheduledTicket.sequenceNumber ?? '',
              }
            }),
            ...(inspections || [])
              .filter((inspection) => inspection.assetId === assetId)
              .map(
                (inspection): AssetHistory => ({
                  ...inspection,
                  assetHistoryType: 'inspection',
                  date: inspection.startDate,
                  ID: inspection.name ?? '',
                  status: inspection.checkRecordSummaryStatus,
                })
              ),
          ]

    return {
      ...assetHistoryQuery,
      status: assetHistoryQuery.status,

      assetHistory,
    }
  }, [assetHistoryQuery, assetId, ticketStatuses])
}

function getAssetHistory({
  siteId,
  assetId,
  twinId,
  isTicketingDisabled,
  isInsightsDisabled,
  isInspectionEnabled,
  isScheduledTicketsEnabled,
}: {
  siteId: string
  assetId: string
  twinId: string
  isTicketingDisabled?: boolean
  isInsightsDisabled?: boolean
  isInspectionEnabled?: boolean
  isScheduledTicketsEnabled?: boolean
}) {
  return Promise.all([
    isInsightsDisabled
      ? Promise.resolve([])
      : fetchAssetInsights({
          params: {
            filterSpecifications: [
              {
                field: 'siteId',
                operator: FilterOperator.equalsLiteral,
                value: siteId,
              },
              {
                field: 'twinId',
                operator: FilterOperator.equalsLiteral,
                value: twinId,
              },
            ],
          },
        }),
    isTicketingDisabled
      ? Promise.resolve([])
      : getAssetTicketsHistory({ siteId, assetId, tab: 'all' }), // when scheduled is undefined, it will only fetch standard tickets records

    !isTicketingDisabled && isScheduledTicketsEnabled
      ? getAssetTicketsHistory({ siteId, assetId, tab: 'all', scheduled: true }) // when scheduled is true, it will only fetch scheduled tickets records
      : Promise.resolve([]),
    isInspectionEnabled ? getInspections(siteId) : Promise.resolve([]),
  ]).then(([insights, standardTickets, scheduledTickets, inspections]) => ({
    insights,
    standardTickets,
    scheduledTickets,
    inspections,
  }))
}
