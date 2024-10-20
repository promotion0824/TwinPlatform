import { priorities, useTicketStatuses } from '@willow/common'
import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import { caseInsensitiveEquals, useScopeSelector, useUser } from '@willow/ui'
import _ from 'lodash'
import { useSites } from 'providers'
import { useEffect, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useGetTickets } from '../../hooks'
import { TicketsContext } from './TicketsContext'

const INITIAL_DUE_BYS = ['overdue', 'today', 'next7Days', 'next30Days']

export default function TicketsProvider({
  tab,
  onTabChange,
  siteId,
  assetId = undefined,
  showSite,
  showHeader,
  showSourceId = false,
  isScheduled,
  children,
  dataSegmentPropsPage,
  selectedTicketId,
}) {
  // When user lands on Tickets page, we apply filters they have selected previously,
  // but if user lands on this page from 3D viewer, we cannot guarantee filtered tickets
  // contain the one user intended to see, so we do not apply filters in this case.
  const [{ noPresetFilter }] = useMultipleSearchParams(['noPresetFilter'])
  const { isScopeSelectorEnabled, location } = useScopeSelector()
  const scopeId = location?.twin?.id
  const { t } = useTranslation()
  const sites = useSites()
  const user = useUser()
  const nextSiteId = sites.some((site) => site.id === siteId) ? siteId : null
  const { getByStatusCode } = useTicketStatuses()

  const [filters, setFilters] = useState({
    search: '',
    sites,
    priorities,
    sources: [],
    siteId: nextSiteId,
    selectedPriorities: [],
    selectedSources: [],
    categories: [],
    selectedCategories: [],
    assignees: [],
    selectedAssignees: [],
    statuses: [], // TicketStatus[]
    selectedStatuses: [], // number[]
    dueBy: [], // | DueBy[]
    selectedDueBy: null, // | DueBy from './ticketsProviderTypes.ts'
  })

  /**
   * store insightId and siteId of the ticket when user clicks on "Willow Insight" link,
   * as they will be used to display Insight Drawer (InsightModal.js).
   * "Willow Insight" link will be displayed at the "Source" column of tickets table when ticket has insight.
   * reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/77097
   */
  const [selectedLink, setSelectedLink] = useState({
    insightId: null,
    siteId: null,
  })

  // Save site's ticket filters settings
  useEffect(() => {
    user.saveOptions('ticketFilterSettings', {
      ...user.options?.ticketFilterSettings,
      [nextSiteId]: {
        selectedPriorities: filters.selectedPriorities,
        selectedSources: filters.selectedSources,
        selectedCategories: filters.selectedCategories,
        selectedAssignees: filters.selectedAssignees,
        selectedStatuses: filters.selectedStatuses,
        selectedDueBy: filters.selectedDueBy,
      },
    })
  }, [
    filters.selectedPriorities,
    filters.selectedSources,
    filters.selectedCategories,
    filters.selectedAssignees,
    filters.selectedStatuses,
    filters.selectedDueBy,
  ])

  // Set saved site's ticket filters settings
  useEffect(() => {
    if (noPresetFilter) return
    setFilters((prevFilters) => ({
      ...prevFilters,
      selectedPriorities:
        user.options?.ticketFilterSettings?.[nextSiteId]?.selectedPriorities ||
        [],
      selectedSources:
        user.options?.ticketFilterSettings?.[nextSiteId]?.selectedSources || [],
      selectedCategories:
        user.options?.ticketFilterSettings?.[nextSiteId]?.selectedCategories ||
        [],
      selectedAssignees:
        user.options?.ticketFilterSettings?.[nextSiteId]?.selectedAssignees ||
        [],
      selectedStatuses:
        user.options?.ticketFilterSettings?.[nextSiteId]?.selectedStatuses ||
        [],
      selectedDueBy:
        user.options?.ticketFilterSettings?.[nextSiteId]?.selectedDueBy || null,
    }))
  }, [nextSiteId, noPresetFilter])

  const ticketQueryParams = {
    siteId,
    tab,
    scheduled: isScheduled,
    assetId,
    isClosed: assetId ? false : undefined,
  }

  const scopedTicketQueryParams = {
    ...ticketQueryParams,
    ...(scopeId && {
      tab,
      scopeId,
    }),
  }

  const {
    isLoading,
    isError,
    data: ticketsList,
  } = useGetTickets(
    isScopeSelectorEnabled ? scopedTicketQueryParams : ticketQueryParams,
    {
      // Rename sourceName of "Platform" to "Willow".
      // when the ticket has an associated insight and sourceName is "Platform", we want to display "Willow Insight" instead.
      // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/77097
      select: (response) =>
        response.map((ticket) => {
          const insightExists =
            ticket.insightId != null && ticket.insightId !== ''
          return {
            ...ticket,
            sourceName: caseInsensitiveEquals(
              ticket?.sourceName ?? '',
              'platform'
            )
              ? _.startCase(
                  insightExists
                    ? `${t('plainText.willow')} ${t('headers.insight')}`
                    : t('plainText.willow')
                )
              : ticket.sourceName,
          }
        }),
    }
  )

  // Get tickets filters options for a given ticket list.
  const filterOptions = useMemo(() => {
    const tickets = ticketsList || []

    const nextSources = _(tickets)
      .map((ticket) => ticket.sourceName)
      .uniq()
      .orderBy((source) => source.toLowerCase())
      .value()

    const nextCategories = _(tickets)
      .map((ticket) => ticket.category)
      .uniq()
      .orderBy((category) => category.toLowerCase())
      .value()

    const nextAssignees = _(tickets)
      .map((ticket) => ticket.assignedTo)
      .filter((assignedTo) => assignedTo != null)
      .uniq()
      .orderBy((assignedTo) => assignedTo.toLowerCase())
      .value()

    const nextStatuses = _(tickets)
      .uniqBy('statusCode')
      .sortBy('statusCode')
      .map(({ statusCode }) => getByStatusCode(statusCode))
      .value()

    const nextDueBy = tab === 'Open' ? INITIAL_DUE_BYS : []

    return {
      sourceOptions: nextSources,
      categoriesOptions: nextCategories,
      assigneesOptions: nextAssignees,
      statusOptions: nextStatuses,
      dueByOptions: nextDueBy,
    }
  }, [getByStatusCode, ticketsList, tab])

  // Whenever tickets list changes, update filters options.
  useEffect(() => {
    const {
      sourceOptions,
      categoriesOptions,
      assigneesOptions,
      statusOptions,
      dueByOptions,
    } = filterOptions

    // When tickets query is in load state, sourceOptions and categoriesOptions will be empty,
    // so we do not want to update filters options.
    if (
      sourceOptions.length > 0 ||
      categoriesOptions.length > 0 ||
      statusOptions.length > 0 ||
      // Case when tickets response is empty, update filters options.
      ticketsList?.length === 0
    ) {
      setFilters((prevFilters) => ({
        ...prevFilters,
        sources: sourceOptions,
        categories: categoriesOptions,
        assignees: assigneesOptions,
        statuses: statusOptions,
        dueBy: dueByOptions,
      }))
    }
  }, [filterOptions, ticketsList?.length])

  const selectedTicket = ticketsList?.find(
    (ticket) => ticket.id === selectedTicketId
  )

  const context = {
    selectedTicket,
    siteId: filters.siteId,
    showSite,
    showHeader,
    showSourceId,
    isScheduled,
    assetId,
    dataSegmentPropsPage,
    selectedLink,
    setSelectedLink,

    isLoading,
    isError,
    ticketsList,

    tab,
    filters,

    onTabChange,
    setFilters,

    clearFilters() {
      setFilters((prevFilters) => ({
        ...prevFilters,
        search: '',
        siteId: showSite ? null : prevFilters.siteId,
        selectedPriorities: [],
        selectedSources: [],
        selectedCategories: [],
        selectedAssignees: [],
        selectedStatuses: [],
        selectedDueBy: null,
      }))
    },

    hasFiltersChanged() {
      return !_.isEqual(
        {
          search: filters.search,
          siteId: filters.siteId,
          selectedPriorities: filters.selectedPriorities,
          // Exclude selectedSources and selectedCategories that are not in tickets response.
          // - We stored all selected filters from all the tabs in tickets page, so applied filters will persist
          //   when switching tabs.
          selectedSources: filters.selectedSources.filter((source) =>
            filters.sources.includes(source)
          ),
          selectedCategories: filters.selectedCategories.filter((category) =>
            filters.categories.includes(category)
          ),
          selectedAssignees: filters.selectedAssignees.filter((assignee) =>
            filters.assignees.includes(assignee)
          ),
          selectedStatuses: _.intersection(
            filters.selectedStatuses,
            filters.statuses.map((statues) => statues.statusCode)
          ),
          selectedDueBy:
            // filters.dueBy will be [] when it's not 'Open' tab, thus
            // previous selectedDueBy will not be considered valid
            filters.dueBy.length > 0 ? filters.selectedDueBy : null,
        },
        {
          search: '',
          siteId: showSite ? null : filters.siteId,
          selectedPriorities: [],
          selectedSources: [],
          selectedCategories: [],
          selectedAssignees: [],
          selectedStatuses: [],
          selectedDueBy: null,
        }
      )
    },
  }

  return (
    <TicketsContext.Provider value={context}>
      {children}
    </TicketsContext.Provider>
  )
}
