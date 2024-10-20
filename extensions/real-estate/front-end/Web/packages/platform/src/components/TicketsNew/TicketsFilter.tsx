import {
  getPriorityTranslatedName,
  getTicketStatusTranslatedName,
  useLanguage,
} from '@willow/ui'
import {
  Button,
  Checkbox,
  CheckboxGroup,
  Panel,
  PanelContent,
  SearchInput,
  Select,
  Stack,
} from '@willowinc/ui'
import _, { find } from 'lodash'
import { useMemo } from 'react'
import { TFunction, useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'

import { capitalizeFirstChar, titleCase } from '@willow/common'
import { TicketStatus } from '@willow/common/ticketStatus'
import AssigneesSelect from './AssigneesSelect'
import { useTickets } from './TicketsContext'
import { DueBy } from './ticketsProviderTypes'

const StyledPanelContent = styled(PanelContent)({
  height: '100%',
})

const getOptionByDueBy = (
  dueBy: DueBy,
  t: TFunction<'translation', undefined>
):
  | {
      value: DueBy
      label: string
    }
  | undefined => {
  const dueByOptions = [
    {
      value: DueBy.Overdue,
      label: t('plainText.overdue'),
    },
    {
      value: DueBy.Today,
      label: t('plainText.today'),
    },
    {
      value: DueBy.Next7Days,
      label: capitalizeFirstChar(t('interpolation.nextNumDays', { num: 7 })),
    },
    {
      value: DueBy.Next30Days,
      label: capitalizeFirstChar(t('interpolation.nextNumDays', { num: 30 })),
    },
  ]

  return find(dueByOptions, {
    value: dueBy,
  })
}

export default function TicketsFilters({ isWithinPortal }) {
  const { language } = useLanguage()
  const tickets = useTickets()
  const { t } = useTranslation()

  const { filters, setFilters } = tickets
  // filters.assignees is all possible assignees based on tickets in current table,
  // filters.selectedAssignees is the assignees filters that are currently selected.
  // if an assignee filter is applied but it's not in filters.assignees,
  // it means that assignee is not relevant to current tickets in table, so we exclude it.
  const availableAssigneesFilter = useMemo(
    () =>
      filters.selectedAssignees.filter((assignee) =>
        filters.assignees.includes(assignee)
      ),
    [filters]
  )

  return (
    <Panel
      id="tickets-filters-panel"
      collapsible={!isWithinPortal}
      {...(isWithinPortal ? undefined : { defaultSize: 320 })}
      // show Panel footer only if it's not within portal and at least one filter is applied
      {...(!isWithinPortal
        ? {
            title: t('headers.filters'),
            footer: (
              <Button
                background="transparent"
                disabled={!tickets.hasFiltersChanged()}
                kind="secondary"
                onClick={tickets.clearFilters}
              >
                {titleCase({ text: t('labels.resetFilters'), language })}
              </Button>
            ),
          }
        : undefined)}
      css={`
        border: ${isWithinPortal ? 'none' : ''};
      `}
    >
      <StyledPanelContent>
        <Stack p="s16" gap="s16">
          {!isWithinPortal && <TicketsSearchInputFilter />}
          <AssigneesSelect
            assignees={filters.assignees}
            selectedAssignees={availableAssigneesFilter}
            onToggle={(assignee) => {
              setFilters((prevFilters) => ({
                ...prevFilters,
                selectedAssignees: assignee
                  ? _.xor(prevFilters.selectedAssignees, [assignee])
                  : [],
              }))
            }}
          />
          {tickets.showSite && (
            <Select
              clearable
              label={t('labels.site')}
              placeholder={t('labels.selectSite')}
              css={{
                input: { textTransform: 'capitalize' },
              }}
              value={tickets.filters.siteId}
              onChange={(siteId) => {
                tickets.setFilters((prevFilters) => ({
                  ...prevFilters,
                  siteId,
                }))
              }}
              data={tickets.filters.sites.map((site) => ({
                value: site.id,
                label: site.name,
              }))}
            />
          )}

          {tickets.filters.dueBy.length > 0 && (
            <Select
              clearable
              label={t('labels.dueDate')}
              placeholder={titleCase({
                text: t('labels.selectDueDate'),
                language,
              })}
              data={tickets.filters.dueBy.map((dueBy) =>
                getOptionByDueBy(dueBy, t)
              )}
              value={tickets.filters.selectedDueBy}
              onChange={(value) => {
                tickets.setFilters((prevFilters) => ({
                  ...prevFilters,
                  selectedDueBy: value,
                }))
              }}
            />
          )}
          {tickets.filters.statuses.length > 0 && (
            <CheckboxGroup
              label={t('labels.status')}
              onChange={(values) =>
                tickets.setFilters((prevFilters) => ({
                  ...prevFilters,
                  selectedStatuses: values.map((value) => Number(value)),
                }))
              }
              // CheckboxGroup value type only supports string[] at the moment
              value={tickets.filters.selectedStatuses.map(
                (statusCode: TicketStatus['statusCode']) =>
                  statusCode.toString()
              )}
            >
              {tickets.filters.statuses.map((status: TicketStatus) => (
                <Checkbox
                  key={status.status}
                  value={status.statusCode.toString()}
                  label={titleCase({
                    text:
                      getTicketStatusTranslatedName(t, status?.status ?? '') ??
                      status?.status,
                    language,
                  })}
                />
              ))}
            </CheckboxGroup>
          )}
          <CheckboxGroup
            label={t('labels.priority')}
            // value type is string[]
            value={tickets.filters.selectedPriorities.map((priority) =>
              priority.toString()
            )}
            onChange={(values) =>
              tickets.setFilters((prevFilters) => ({
                ...prevFilters,
                selectedPriorities: values.map((value) => Number(value)),
              }))
            }
          >
            {tickets.filters.priorities.map((priority) => (
              <Checkbox
                key={priority.id}
                value={priority.id.toString()}
                label={getPriorityTranslatedName(t, priority.id)}
              />
            ))}
          </CheckboxGroup>
          {tickets.filters.sources.length > 0 && (
            <CheckboxGroup
              label={t('labels.source')}
              value={tickets.filters.selectedSources}
              onChange={(values) =>
                tickets.setFilters((prevFilters) => ({
                  ...prevFilters,
                  selectedSources: values,
                }))
              }
            >
              {tickets.filters.sources.map((source) => (
                <Checkbox key={source} value={source} label={source} />
              ))}
            </CheckboxGroup>
          )}
          {tickets.filters.categories.length > 0 && (
            <CheckboxGroup
              label={t('labels.category')}
              value={tickets.filters.selectedCategories}
              onChange={(values) =>
                tickets.setFilters((prevFilters) => ({
                  ...prevFilters,
                  selectedCategories: values,
                }))
              }
            >
              {tickets.filters.categories.map((category) => (
                <Checkbox
                  key={category}
                  value={category}
                  label={t(`ticketCategory.${_.camelCase(category)}`, {
                    defaultValue: category,
                  })}
                />
              ))}
            </CheckboxGroup>
          )}
        </Stack>
      </StyledPanelContent>
    </Panel>
  )
}

/**
 * Search input filter for tickets data grid to be used inside TicketsContext.
 */
export const TicketsSearchInputFilter = () => {
  const tickets = useTickets()
  const { t } = useTranslation()

  return (
    <SearchInput
      onChange={(e) => {
        tickets.setFilters((prevFilters) => ({
          ...prevFilters,
          search: e.target.value,
        }))
      }}
      value={tickets.filters.search}
      placeholder={t('labels.search')}
    />
  )
}
