import _ from 'lodash'
import tw, { styled } from 'twin.macro'
import { useMemo, useState } from 'react'
import { Row } from 'react-table'
import { Checkbox, Text } from '@willow/ui'
import { Language } from '@willow/ui/providers/LanguageProvider/LanguageJson/LanguageJsonService/LanguageJsonService'
import { TFunction } from 'react-i18next'
import {
  InsightCostImpactPropNames,
  sortImpactCost,
  getImpactScore,
  InsightMetric,
  titleCase,
  sortPriority,
  sortOccurrenceDate,
} from '@willow/common'
import {
  PriorityName,
  TextWithTooltip,
  ActivityCount,
} from '@willow/common/insights/component'
import {
  Insight,
  InsightStatus,
  InsightWorkflowStatus,
} from '@willow/common/insights/insights/types'
import AssetLink from '@willow/common/components/AssetLink'
import { Icon, IconName } from '@willowinc/ui'
import routes from '../../../../routes'
import {
  AvoidableExpPerYearHeader,
  FormattedCell,
} from '../../InsightGroupTable/ImpactInsightsGroupedTable/useWorkflowGroupedColumns'
import InsightWorkflowStatusPill from '../../../InsightStatusPill/InsightWorkflowStatusPill'
import ActionsViewControl, { InsightActions } from '../../ui/ActionsViewControl'

export default function useWorkflowUngroupedColumns({
  language,
  t,
  filteredInsights,
  selectedInsightIds,
  isGrouped = false,
  isInsightIdSelected,
  onSelectedInsightIds,
  onToggleSelectedInsightId,
  siteId,
  dateColumn,
  insightMetric = InsightMetric.cost,
  hideStatusColumn = false,
  isSavings,
  tab,
  onSelectAction,
}: {
  language: Language
  t: TFunction
  filteredInsights: Insight[]
  selectedInsightIds: string[]
  isGrouped: boolean
  onToggleSelectedInsightId: (insightId: string) => void
  isInsightIdSelected: (insightId: string) => boolean
  onSelectedInsightIds: (selectedInsightIds: string[]) => void
  siteId?: string
  dateColumn: {
    columnText: string
    accessor: string
  }
  insightMetric: string
  hideStatusColumn?: boolean
  isSavings?: boolean
  tab?: InsightStatus | InsightWorkflowStatus
  onSelectAction: (action: InsightActions, insight: Insight) => void
}) {
  const [isActionsViewOpen, setIsActionsViewOpen] = useState(false)
  const [insightIdWithOpenControl, setInsightIdWithOpenAction] = useState<
    string | undefined
  >(undefined)
  const isOpenTab = tab && tab === 'open'

  const handleActionView = (event, row) => {
    event?.stopPropagation()
    setIsActionsViewOpen(!isActionsViewOpen)
    setInsightIdWithOpenAction(!isActionsViewOpen ? row.id : undefined)
  }

  const activityIcons = useMemo(
    () =>
      [
        {
          icon: 'assignment',
          tooltipText: t('plainText.relatedTicket'),
          key: 'ticketCount',
          filled: true,
          isVisible: isOpenTab,
        },
        {
          icon: 'feedback',
          tooltipText: t('plainText.reported'),
          key: 'reported',
          filled: true,
          isIcon: true,
        },
        {
          icon: 'check_circle',
          tooltipText: t('interpolation.previouslyItemHistory', {
            itemHistory: t('headers.resolved'),
          }),
          key: 'previouslyResolved',
          filled: false,
          isVisible: isOpenTab,
        },
        {
          icon: 'do_not_disturb_on',
          tooltipText: t('interpolation.previouslyItemHistory', {
            itemHistory: t('headers.ignored'),
          }),
          key: 'previouslyIgnored',
          filled: false,
          isVisible: isOpenTab,
        },
      ] as Array<{
        icon: IconName
        tooltipText: string
        key: string
        filled: boolean
        isIcon?: boolean
        isVisible?: boolean
      }>,
    [isOpenTab, t]
  )

  const columns = useMemo(
    () => [
      {
        Header: () => {
          const filteredOpenInsightIds = filteredInsights.map((i) => i.id)
          const isAllOpenInsightsSelected = filteredOpenInsightIds.every((id) =>
            selectedInsightIds.includes(id)
          )
          return (
            !isGrouped && (
              <Checkbox
                aria-label={t('interpolation.actionOnAllInsight', {
                  action: isAllOpenInsightsSelected
                    ? t(`plainText.deselect`)
                    : t(`plainText.select`),
                })}
                value={isAllOpenInsightsSelected}
                onChange={() =>
                  onSelectedInsightIds(
                    isAllOpenInsightsSelected ? [] : filteredOpenInsightIds
                  )
                }
                onClick={(e) => e?.stopPropagation()}
              />
            )
          )
        },
        id: 'checkbox',
        accessor: 'checkbox',
        Cell: ({ row }) => {
          const isInsightSelected = isInsightIdSelected(row.original.id)
          return (
            <Checkbox
              aria-label={`${`${
                isInsightSelected
                  ? t(`plainText.deselect`)
                  : t(`plainText.select`)
              } ${row.original.name}`}`}
              value={isInsightSelected}
              onChange={() => onToggleSelectedInsightId(row.original.id)}
              onClick={(e) => e?.stopPropagation()}
            />
          )
        },
        disableSortBy: true,
      },
      ...(!hideStatusColumn
        ? [
            {
              Header: <TextWithTooltip text={t('labels.status')} />,
              accessor: 'lastStatus',
              Cell: ({ value }) => (
                <InsightWorkflowStatusPill size="md" lastStatus={value} />
              ),
            },
          ]
        : []),
      {
        Header: (
          <TextWithTooltip text={t('headers.insight')} tooltipWidth="100px" />
        ),
        accessor: 'name',
        // business requirement to display ruleName if it's defined and not an empty string,
        // display insight name (also called summary) otherwise
        // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/78451
        Cell: ({ value, row }) => (
          <TextWithTooltip
            text={row.original?.ruleName || value}
            tooltipWidth="200px"
            // need to display e.g. "VAV-PW-L01-01 Out of Range" or "VAV Zone Cold" as it is
            isTitleCase={false}
          />
        ),
      },
      {
        Header: (
          <TextWithTooltip text={t('plainText.asset')} tooltipWidth="100px" />
        ),
        accessor: 'equipmentName',
        Cell: ({ value, row }) => (
          <StyledAssetLink
            tw="pr-2"
            $isShow
            path={routes.portfolio_twins_view__siteId__twinId(
              row.original?.siteId,
              row.original?.twinId
            )}
            siteId={row.original?.siteId}
            twinId={row.original?.twinId}
            assetName={value}
          />
        ),
      },
      {
        Header: <TextWithTooltip text={t('labels.type')} />,
        accessor: 'type',
        Cell: ({ value }) => <Text>{_.upperFirst(value)}</Text>,
      },
      {
        Header: <TextWithTooltip text={t('plainText.activity')} />,
        id: 'activityCount',
        // Adding ticket count, report, previously ignored and previously resolved for column sorting
        accessor: (row) =>
          row.ticketCount +
          row.previouslyResolvedAndIgnoredCount +
          row.reported,
        Cell: ({ row }) => (
          // Showing report icons for all tabs and other icons in active tabs only
          // Reference - https://dev.azure.com/willowdev/Unified/_workitems/edit/84674
          // https://dev.azure.com/willowdev/Unified/_workitems/edit/87044
          <>
            {activityIcons.map(
              ({
                key,
                icon,
                tooltipText,
                filled,
                isVisible = true,
                isIcon = false,
              }) => (
                <ActivityCount
                  key={`${key}-${row.original.id}`}
                  activityCount={row.original[key]}
                  icon={icon}
                  tooltipText={tooltipText}
                  filled={filled}
                  isVisible={isVisible}
                  isIcon={isIcon}
                />
              )
            )}
          </>
        ),
      },
      // this column is only present when All Site is selected
      ...(siteId == null
        ? [
            {
              Header: <TextWithTooltip text={t('labels.site')} tw="pl-[2px]" />,
              accessor: 'siteName',
              Cell: ({ value }) => (
                <TextWithTooltip text={value} tw="pl-[2px]" />
              ),
            },
          ]
        : []),
      ...(insightMetric === InsightMetric.cost
        ? [
            {
              Header: (
                <AvoidableExpPerYearHeader
                  t={t}
                  insightMetric={insightMetric}
                  language={language}
                  isSavings={isSavings}
                />
              ),
              id: 'yearlyCost',
              accessor: (row: Row) =>
                getImpactScore({
                  impactScores: row.impactScores,
                  scoreName: InsightCostImpactPropNames.dailyAvoidableCost,
                  multiplier: 365,
                  language,
                  decimalPlaces: 0,
                }),
              sortType: sortImpactCost(
                InsightCostImpactPropNames.dailyAvoidableCost
              ),
              Cell: FormattedCell,
              sortDescFirst: true,
            },
          ]
        : [
            {
              Header: (
                <AvoidableExpPerYearHeader
                  t={t}
                  insightMetric={insightMetric}
                  language={language}
                  isSavings={isSavings}
                />
              ),
              id: 'yearlyEnergy',
              accessor: (row: Row) =>
                getImpactScore({
                  impactScores: row.impactScores,
                  scoreName: InsightCostImpactPropNames.dailyAvoidableEnergy,
                  multiplier: 365,
                  language,
                  decimalPlaces: 0,
                }),
              sortType: sortImpactCost(
                InsightCostImpactPropNames.dailyAvoidableEnergy
              ),
              Cell: FormattedCell,
              sortDescFirst: true,
            },
          ]),
      {
        Header: <TextWithTooltip text={t('labels.priority')} />,
        id: 'priority',
        accessor: (row: Row) =>
          getImpactScore({
            impactScores: row.impactScores,
            scoreName: InsightCostImpactPropNames.priorityScore,
            language,
          }),
        Cell: ({ value, row }) => (
          <PriorityName
            impactScorePriority={value}
            insightPriority={row.original.priority}
          />
        ),
        sortType: sortPriority(),
        sortDescFirst: true,
      },
      {
        Header: (
          <TextWithTooltip
            text={titleCase({
              text: t(dateColumn.columnText),
              language,
            })}
          />
        ),
        id: 'occurredDate',
        accessor: dateColumn.accessor,
        Cell: ({ value }: { value: string }) => (
          <TextWithTooltip text={value} />
        ),
        sortDescFirst: true,
        sortType: sortOccurrenceDate(),
      },
      {
        id: 'insightActionIcon',
        accessor: 'id',
        Cell: ({ row }) => (
          <ActionsViewControl
            selectedInsight={row.original}
            lastStatus={row.original.lastStatus}
            floorId={row.original.floorId}
            siteId={row.original.siteId}
            assetId={row.original.asset?.id}
            onCreateTicketClick={() => {
              onSelectAction(InsightActions.newTicket, row.original)
            }}
            onDeleteClick={() => {
              onSelectAction(InsightActions.delete, row.original)
            }}
            onResolveClick={() => {
              onSelectAction(InsightActions.resolve, row.original)
            }}
            onReportClick={() =>
              onSelectAction(InsightActions.report, row.original)
            }
            canDeleteInsight={siteId != null}
            opened={isActionsViewOpen && insightIdWithOpenControl === row.id}
            onToggleActionsView={() => {
              setIsActionsViewOpen(!isActionsViewOpen)
              setInsightIdWithOpenAction(
                !isActionsViewOpen ? row.id : undefined
              )
            }}
          >
            <Icon
              data-testid={`actionViewControl-${row.original.id}`}
              icon="more_vert"
              className="insightActionIcon"
              tw="leading-[26px]"
              onClick={(event) => handleActionView(event, row)}
            />
          </ActionsViewControl>
        ),
        disableSortBy: true,
      },
    ],
    [
      insightIdWithOpenControl,
      isActionsViewOpen,
      t,
      insightMetric,
      siteId,
      filteredInsights,
      selectedInsightIds,
      onSelectedInsightIds,
      isInsightIdSelected,
      onToggleSelectedInsightId,
      language,
    ]
  )

  return columns
}

const StyledAssetLink = styled(AssetLink)<{ $isShow?: boolean }>(
  ({ $isShow = true }) => ({
    display: $isShow ? 'block' : 'none',
  })
)
