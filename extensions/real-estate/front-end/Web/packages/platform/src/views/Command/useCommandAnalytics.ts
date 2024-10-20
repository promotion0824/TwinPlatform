import { useMemo } from 'react'
import { useAnalytics } from '@willow/ui'
import { useSites } from '../../providers'

const pageEvents = {
  insights: {
    open: 'Insights open',
    inProgress: 'Insights In progress',
    acknowledged: 'Insights Acknowledged',
    closed: 'Insights Closed',
  },
  tickets: {
    standard: {
      landing: 'Standard Tickets',
      open: 'Tickets Open',
      resolved: 'Tickets Resolved',
      closed: 'Tickets Closed',
    },
    scheduled: {
      landing: 'Scheduled Tickets',
      open: 'Scheduled Tickets Open',
      resolved: 'Scheduled Tickets Resolved',
      closed: 'Scheduled Tickets Closed',
    },
    schedules: {
      landing: 'Schedules',
      active: 'Active Schedules',
      archived: 'Archived Schedules',
    },
  },
  inspections: {
    insights: 'Inspections Insights',
    history: 'Inspection History',
    due: 'Inspections Landing',
    completed: 'Inspections Completed',
    usage: 'Inspections Usage',
    zones: 'Inspections zones',
  },
}

/**
 * One stop analytics hook for Command pages (currently only Insights, Tickets
 * and Inspections page).
 * @param siteId The id of site from site selector, used to include site property in Page event.
 */
export default function useCommandAnalytics(siteId?: string) {
  const analytics = useAnalytics()
  const sites = useSites()
  const site = siteId ? sites.find((s) => s.id === siteId) : undefined

  const analyticsProps = useMemo(
    () => ({
      site: site?.name ?? 'All sites',
    }),
    [site?.name]
  )

  return useMemo(
    () => ({
      pageInsights: (tab: keyof typeof pageEvents.insights) =>
        analytics.page(pageEvents.insights[tab], analyticsProps),
      pageTickets: (
        ticketType: keyof typeof pageEvents.tickets,
        tab = 'landing'
      ) => analytics.page(pageEvents.tickets[ticketType][tab], analyticsProps),
      pageInspections: (tab: string) =>
        analytics.page(pageEvents.inspections[tab], analyticsProps),
      trackTicketsSaveSchedule: (schedule) =>
        analytics.track('Schedule Tickets', {
          summary: schedule.summary,
          assets: schedule.assets.map((asset) => asset.name),
        }),
      trackTicketsArchiveSchedules: (schedule) =>
        analytics.track('Archive Schedules', {
          name: schedule.description,
          assets: schedule.assets,
          recurrence: schedule.recurrence,
          priority: schedule.priority,
        }),
      trackInspectionUsageDropdown: (period) =>
        analytics.track('Inspection Usage dropdown', { period }),
      trackInspectionsSaveZone: () => analytics.track('Inspections Save Zone'),
    }),
    [analytics, analyticsProps]
  )
}
