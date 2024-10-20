/* eslint-disable complexity */
import {
  DebouncedSearchInput,
  priorities as priorityOptions,
  titleCase,
} from '@willow/common'
import { ActionIcon } from '@willow/common/insights/component'
import {
  Insight,
  InsightActionIcon,
} from '@willow/common/insights/insights/types'
import { ALL_SITES, getContainmentHelper, useSnackbar } from '@willow/ui'
import {
  Badge,
  Button,
  Checkbox,
  CheckboxGroup,
  Group,
  Icon,
  IconButton,
  Indicator,
  Loader,
  Panel,
  PanelContent,
  PanelGroup,
  Popover,
  Radio,
  RadioGroup,
  Select,
} from '@willowinc/ui'
import _ from 'lodash'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { UseMutationResult } from 'react-query'
import styled, { css, useTheme } from 'styled-components'
import 'twin.macro'
import useUpdateInsightsStatuses from '../../../hooks/Insight/useUpdateInsightsStatuses'
import { statusMap } from '../../../services/Insight/InsightsService'
import AssetDetailsModal from '../../AssetDetailsModal/AssetDetailsModal'
import { useInsightsContext } from '../CardViewInsights/InsightsContext'
import AllInsightsDataGrid from '../CardViewInsights/TableContent/AllInsightsDataGrid'
import { InsightActions } from '../ui/ActionsViewControl'
import InsightsFilters from './InsightsFilters'

const insightTypeNodeTableContainer = 'insightTypeNodeTableContainer'
const { containerName, getContainerQuery } = getContainmentHelper(
  insightTypeNodeTableContainer
)

/**
 * This section is used to display the table view of the insights belonging to specific rule.
 * User can also perform bulk actions on the insights.
 */
const TableContainer = () => {
  const theme = useTheme()
  const snackbar = useSnackbar()
  const {
    siteId,
    insights = [],
    t,
    selectedInsightIds = [],
    canWillowUserDeleteInsight,
    onSelectInsightIds,
    onResetInsightIds,
    isInsightTypeNode,
    totalCount = 0,
  } = useInsightsContext()

  const [selectedAction, setSelectedAction] = useState<string | undefined>(
    undefined
  )

  // exclude insights that are not actionable
  // e.g. when user selects insights on "open" tab, and then switches to "resolved" tab,
  // insights with status "Open" will not be displayed and hence cannot be actioned
  const selectedInsightsCanBeActioned = selectedInsightIds
    .map((id) => insights.find((insight) => insight.id === id))
    .filter((insight): insight is Insight => insight != null)

  // exclude insights with status of "inProgress" as they cannot be ignored
  // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/84169
  const insightIdsCanBeIgnored = selectedInsightsCanBeActioned
    .filter((i) => i.lastStatus !== 'inProgress')
    .map((i) => i.id)

  // only insights with status of "open" can be "setToNew"
  // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/80914
  const insightIdsCanBeSetToNew = selectedInsightsCanBeActioned
    .filter((i) => i.lastStatus === 'open')
    .map((i) => i.id)

  const isSingleSiteSelected = siteId !== ALL_SITES
  const isActionEnabled = isSingleSiteSelected && selectedInsightIds.length > 0
  const isDeleteEnabled = canWillowUserDeleteInsight && isActionEnabled
  const getTooltip = (text: string, nextSiteId?: string) =>
    nextSiteId === ALL_SITES
      ? t('plainText.bulkActionsAreDisabled')
      : _.startCase(text)
  const isMultiInsightsIgnoreEnabled =
    isActionEnabled && insightIdsCanBeIgnored.length > 0
  const isMultiInsightsSetToNewEnabled =
    isActionEnabled && insightIdsCanBeSetToNew.length > 0

  const ignoreInsightsMutation = useUpdateInsightsStatuses(
    {
      siteId,
      insightIds: insightIdsCanBeIgnored,
      newStatus: 'ignored',
    },
    {
      enabled: isSingleSiteSelected,
    }
  )

  const setInsightsStatusesToNewMutation = useUpdateInsightsStatuses(
    {
      siteId,
      insightIds: insightIdsCanBeSetToNew,
      newStatus: 'new',
    },
    {
      enabled: isSingleSiteSelected,
    }
  )

  const handleSetToNewClick = () => {
    const snackbarOptions = {
      isToast: true,
      closeButtonLabel: t('plainText.dismiss'),
    }

    if (setInsightsStatusesToNewMutation.status === 'loading') {
      return
    }

    setInsightsStatusesToNewMutation.mutate(undefined, {
      onError: () => {
        snackbar.show(t('plainText.errorOccurred'), {
          ...snackbarOptions,
          isError: true,
        })
      },
      onSuccess: () => {
        onSelectInsightIds([])
        const insigtCountCannotBeSetToNew =
          selectedInsightsCanBeActioned.length - insightIdsCanBeSetToNew.length

        snackbar.show(
          _.upperFirst(
            t('interpolation.insightsActioned', {
              count: insightIdsCanBeSetToNew.length,
              action: t('plainText.setToNew'),
            })
          ),
          snackbarOptions
        )

        if (insigtCountCannotBeSetToNew > 0) {
          const newInsights = selectedInsightsCanBeActioned.filter(
            (i) => i.lastStatus === 'new'
          )

          const inProgressInsights = selectedInsightsCanBeActioned.filter(
            (i) => i.lastStatus === 'inProgress'
          )

          for (const collection of [newInsights, inProgressInsights]) {
            if (collection.length > 0) {
              snackbar.show(
                t('interpolation.insightCountCannotBeSetToNew', {
                  count: collection.length,
                  lastStatus:
                    collection[0].lastStatus === 'inProgress'
                      ? _.startCase(t('plainText.inProgress'))
                      : t('beamer.new'),
                }),
                { ...snackbarOptions, isError: true, height: '84px' }
              )
            }
          }
        }
      },
    })
  }

  const handleIgnoreInsightsClick = () => {
    const snackbarOptions = {
      isToast: true,
      closeButtonLabel: t('plainText.dismiss'),
    }

    if (ignoreInsightsMutation.status === 'loading') {
      return
    }
    ignoreInsightsMutation.mutate(undefined, {
      onError: () => {
        snackbar.show(t('plainText.errorOccurred'), {
          ...snackbarOptions,
          isError: true,
        })
      },
      onSuccess: () => {
        onSelectInsightIds([])
        const insightCountCannotBeIgnored =
          selectedInsightsCanBeActioned.length - insightIdsCanBeIgnored.length
        snackbar.show(
          _.capitalize(
            t('interpolation.insightsActioned', {
              count: insightIdsCanBeIgnored.length,
              action: t('headers.ignored'),
            })
          ),
          snackbarOptions
        )
        if (insightCountCannotBeIgnored > 0) {
          snackbar.show(
            _.capitalize(
              t('interpolation.insightsCannotBeIgnored', {
                count: insightCountCannotBeIgnored,
              })
            ),
            { ...snackbarOptions, isError: true, height: '84px' }
          )
        }
      },
    })
  }

  const actionIcons: InsightActionIcon[] = [
    {
      icon: 'undo',
      tooltipText: getTooltip(t('plainText.setToNew'), siteId),
      enabled: isMultiInsightsSetToNewEnabled,
      onClick: isMultiInsightsSetToNewEnabled ? handleSetToNewClick : undefined,
      fontSize: '26px',
      marginBottom: '2px',
    },
    {
      icon: 'do_not_disturb_on',
      tooltipText: getTooltip(t('plainText.ignore'), siteId),
      enabled: isMultiInsightsIgnoreEnabled,
      onClick: isMultiInsightsIgnoreEnabled
        ? handleIgnoreInsightsClick
        : undefined,
      filled: false,
      fontSize: theme.font.heading.xl2.fontSize,
    },
    {
      icon: 'feedback',
      tooltipText: getTooltip(t('plainText.report'), siteId),
      enabled: isActionEnabled,
      onClick: isActionEnabled
        ? () => setSelectedAction(InsightActions.report)
        : undefined,
      filled: true,
      fontSize: theme.font.heading.xl2.fontSize,
    },
    {
      icon: 'delete',
      tooltipText: getTooltip(t('plainText.delete'), siteId),
      isRed: true,
      onClick: isDeleteEnabled
        ? () => setSelectedAction(InsightActions.delete)
        : undefined,
      enabled: !!isDeleteEnabled,
      filled: false,
      fontSize: theme.font.display.lg.medium.fontSize,
    },
  ]

  return (
    <ContentLayout
      css={`
        container-type: size;
        container-name: ${containerName};
      `}
      header={totalCount === 1 ? t('headers.insight') : t('headers.insights')}
      headerCount={totalCount}
      bulkActions={actionIcons}
      ignoreInsightsMutation={ignoreInsightsMutation}
      setInsightsStatusesToNewMutation={setInsightsStatusesToNewMutation}
    >
      {/* Displaying table filter only for Insight Type Node Page */}
      <PanelGroup
        direction="vertical"
        css={css`
          & > *:not(:first-child) {
            margin-top: 0px;
          }
        `}
      >
        <Panel tw="border-0" id="card-view-insights-data-grid-panel">
          {isInsightTypeNode && (
            <InsightsFilters
              css={`
                ${getContainerQuery('width < 600px')} {
                  display: none;
                }
              `}
            />
          )}
          <AllInsightsDataGrid />
        </Panel>
      </PanelGroup>
      {/*
        this modal is used for confirming deletion or report one or multiple insights
      */}
      {selectedInsightIds.length > 0 &&
        isSingleSiteSelected &&
        (selectedAction === InsightActions.delete ||
          selectedAction === InsightActions.report) && (
          <AssetDetailsModal
            siteId={siteId}
            item={{
              modalType:
                selectedAction === InsightActions.report
                  ? 'report'
                  : 'deleteInsightsConfirmation',
            }}
            onClose={() => {
              setSelectedAction(undefined)
            }}
            dataSegmentPropPage={`Insights ${
              selectedAction === InsightActions.report ? 'Report' : 'Deletion'
            } Confirmation`}
            // confirmation modal does not need navigation
            navigationButtonProps={{
              items: [],
              selectedItem: undefined,
              setSelectedItem: _.noop,
            }}
            onActionChange={setSelectedAction as (action?: string) => void}
            canDeleteInsight={siteId !== ALL_SITES}
            selectedInsightIds={selectedInsightIds}
            onClearSelectedInsightIds={onResetInsightIds}
          />
        )}
    </ContentLayout>
  )
}

export default TableContainer

/**
 * A panel with a header which contains 2 sections:
 * 1. a title text with a count badge
 * 2. a list of bulk actions and a view toggle
 */
const ContentLayout = ({
  header,
  headerCount,
  bulkActions,
  children,
  ignoreInsightsMutation,
  setInsightsStatusesToNewMutation,
  className,
}: {
  header: string
  headerCount: number
  bulkActions?: InsightActionIcon[]
  children: React.ReactNode
  ignoreInsightsMutation: UseMutationResult<Insight>
  setInsightsStatusesToNewMutation: UseMutationResult<Insight>
  className?: string
}) => {
  const { t } = useTranslation()
  const {
    language,
    selectedInsightIds = [],
    queryParams: {
      status,
      lastOccurredDate,
      priorities,
      search,
      selectedStatuses,
    } = {},
    onQueryParamsChange,
    cardSummaryFilters,
  } = useInsightsContext()

  const hasAnyFilters = [
    search,
    priorities,
    lastOccurredDate,
    selectedStatuses,
  ].some((filter) => (filter ?? []).length > 0)

  return (
    <PanelGroup className={className}>
      <Panel
        id="insightTablePanel"
        title={
          <Group>
            {titleCase({
              language,
              text: header,
            })}
            <Badge variant="bold" size="xs" color="gray" tw="ml-[6px]">
              {headerCount}
            </Badge>

            <Indicator
              disabled={!hasAnyFilters}
              css={`
                margin-left: auto;
                ${getContainerQuery('width > 600px')} {
                  display: none;
                  margin-left: auto;
                }
              `}
            >
              <Popover>
                <Popover.Target>
                  <Button kind="secondary" prefix={<Icon icon="filter_list" />}>
                    {t('headers.filters')}
                  </Button>
                </Popover.Target>
                <Popover.Dropdown
                  css={`
                    transform: translateX(-34px);
                  `}
                >
                  <Group gap="s8" p="s16" tw="flex-col">
                    <DebouncedSearchInput
                      key={(search ?? '').toString()}
                      value={search?.toString()}
                      onDebouncedSearchChange={onQueryParamsChange}
                    />
                    <Select
                      data={[
                        {
                          value: 'default',
                          label: _.capitalize(t('headers.active')),
                        },
                        {
                          value: 'inactive',
                          label: _.capitalize(t('headers.inactive')),
                        },
                      ]}
                      onChange={(nextOption: string) => {
                        onQueryParamsChange?.({
                          status: statusMap[nextOption],
                          selectedStatuses: [],
                          page: undefined,
                        })
                      }}
                      value={
                        _.isEqual(status, statusMap.inactive)
                          ? 'inactive'
                          : 'default'
                      }
                    />
                    <CheckboxGroup
                      label={t('labels.status')}
                      tw="self-start"
                      value={Array.from(selectedStatuses ?? [])}
                      onChange={(values) => {
                        onQueryParamsChange?.({
                          selectedStatuses: values,
                          page: undefined,
                        })
                      }}
                    >
                      {(cardSummaryFilters?.detailedStatus ?? []).map(
                        (item) => (
                          <Checkbox
                            label={titleCase({
                              text: t(`plainText.${_.lowerFirst(item)}`),
                              language,
                            })}
                            value={item}
                          />
                        )
                      )}
                    </CheckboxGroup>

                    <CheckboxGroup
                      label={t('labels.priority')}
                      tw="self-start"
                      value={Array.from(priorities ?? [])}
                      onChange={(values) => {
                        onQueryParamsChange?.({
                          priorities: values,
                          page: undefined,
                        })
                      }}
                    >
                      {priorityOptions.map(({ id, name }) => (
                        <Checkbox
                          key={id}
                          label={titleCase({
                            text: t(`plainText.${_.lowerFirst(name)}`),
                            language,
                          })}
                          value={id.toString()}
                        />
                      ))}
                    </CheckboxGroup>

                    <RadioGroup
                      label={t('labels.date')}
                      tw="self-start"
                      value={lastOccurredDate?.toString() ?? 'all'}
                      onChange={(value) => {
                        onQueryParamsChange?.({
                          lastOccurredDate: value === 'all' ? undefined : value,
                          page: undefined,
                        })
                      }}
                    >
                      {dateSelection.map(({ label, value }) => (
                        <Radio
                          key={value}
                          label={titleCase({
                            text: t(label),
                            language,
                          })}
                          value={value}
                        />
                      ))}
                    </RadioGroup>
                  </Group>
                </Popover.Dropdown>
              </Popover>
            </Indicator>
          </Group>
        }
        headerControls={
          <div tw="flex">
            {ignoreInsightsMutation.status === 'loading' ||
            setInsightsStatusesToNewMutation.status === 'loading' ? (
              <StyledLoader intent="secondary" />
            ) : (
              selectedInsightIds.length > 0 &&
              bulkActions?.map(
                ({
                  icon,
                  tooltipText,
                  isRed = false,
                  onClick = () => {},
                  enabled = false,
                  filled = true,
                  fontSize,
                }) => (
                  <IconButton
                    kind="secondary"
                    tw="outline-none p-[0px]"
                    key={icon}
                  >
                    <StyledActionIcon
                      icon={icon}
                      data-tooltip={tooltipText}
                      data-tooltip-position="top"
                      $isRed={isRed}
                      onClick={onClick}
                      data-testid={`insights-action-${icon}`}
                      $enabled={enabled}
                      filled={filled}
                      fontSize={fontSize}
                    />
                  </IconButton>
                )
              )
            )}
          </div>
        }
      >
        <PanelContent tw="h-full">{children}</PanelContent>
      </Panel>
    </PanelGroup>
  )
}

const StyledActionIcon = styled(ActionIcon)(({ theme }) => ({
  lineHeight: theme.font.heading.xl2.lineHeight,
  margin: `0 ${theme.spacing.s8}`,
}))

const StyledLoader = styled(Loader)(({ theme }) => ({
  marginRight: theme.spacing.s20,
  marginTop: theme.spacing.s6,
}))

const dateSelection = [
  {
    label: 'plainText.last24Hours',
    value: '1',
  },
  {
    label: 'plainText.last7Days',
    value: '7',
  },
  {
    label: 'plainText.last30Days',
    value: '30',
  },
  {
    label: 'plainText.lastYear',
    value: '365',
  },
  {
    label: 'plainText.lastTwoYears',
    value: '730',
  },
  {
    label: 'plainText.allTime',
    value: 'all',
  },
]
