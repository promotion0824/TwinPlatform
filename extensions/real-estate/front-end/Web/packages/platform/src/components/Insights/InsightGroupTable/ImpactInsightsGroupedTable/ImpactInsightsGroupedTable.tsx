import { Fragment, useMemo, memo } from 'react'
import _ from 'lodash'
import tw, { styled } from 'twin.macro'
import {
  useTable,
  HeaderGroup,
  EnhancedColumn,
  Row,
  Cell,
  useSortBy,
} from 'react-table'
import { TFunction } from 'react-i18next'
import {
  NotFound,
  SortIndicator,
  TableComponents,
  InsightGroups,
  getInsightStatusTranslatedName,
} from '@willow/ui'
import {
  InsightCostImpactPropNames,
  getTotalImpactScore,
  localStorage,
} from '@willow/common'
import { Language } from '@willow/ui/providers/LanguageProvider/LanguageJson/LanguageJsonService/LanguageJsonService'
import {
  Insight,
  InsightStatus,
  InsightWorkflowStatus,
} from '@willow/common/insights/insights/types'
import ImpactInsightsUngroupedTable, {
  customGetSortByToggleProps,
} from '../../InsightsTableContent/ImpactInsightsUngroupedTable/ImpactInsightsUngroupedTable'
import useWorkflowGroupedColumns from './useWorkflowGroupedColumns'

const { Table, TBody, TD, TH, TR, THead } = TableComponents

const ImpactInsightsGroupedTable = ({
  filteredInsights,
  language,
  t,
  dataSegmentPropPage,
  selectedInsightIds,
  onToggleSelectedInsightId,
  isInsightIdSelected,
  onSelectedInsightIds,
  onExpandGroup,
  onSelectAllGroupedInsights,
  onSelectGroupedInsights,
  siteId,
  selectedInsight,
  onSelectInsight,
  insightMetric,
  groupBy,
  groupByName,
  expandedGroupId,
  groupById,
  getTranslatedModelName,
  paginationEnabled,
  pageSize,
  initialPageIndex,
  onPageSizeChange,
  insightTab,
  onInsightTabChange,
  hideStatusColumn = false,
  dateColumn,
  isSavings,
  tab,
  clearSelectedInsightIds,
}: {
  filteredInsights: Insight[]
  language: Language
  t: TFunction
  dataSegmentPropPage: string
  selectedInsightIds: string[]
  onToggleSelectedInsightId: (insightId: string) => void
  isInsightIdSelected: (insightId: string) => boolean
  onSelectedInsightIds: (selectedInsightIds: string[]) => void
  onExpandGroup?: (obj: { expandedGroupId?: string }) => void
  onSelectAllGroupedInsights: (
    allGroupedInsights: Insight[],
    isAllDisplayedInsightsSelected: boolean
  ) => void
  onSelectGroupedInsights: (
    isEveryInsightChecked: boolean,
    rowData: Insight
  ) => void
  siteId?: string
  selectedInsight?: Insight
  onSelectInsight: (insight?: Insight) => void
  insightMetric: string
  groupBy: InsightGroups
  groupByName: string
  expandedGroupId?: string
  groupById: string
  getTranslatedModelName?: (primaryModelId: string) => void
  paginationEnabled?: boolean
  pageSize: number
  initialPageIndex: number
  onPageSizeChange?: (pageSize: number) => void
  insightTab: string
  onInsightTabChange: (insightTab: string) => void
  hideStatusColumn?: boolean
  dateColumn: {
    columnText: string
    accessor: string
  }
  isSavings?: boolean
  tab?: InsightStatus | InsightWorkflowStatus
  clearSelectedInsightIds?: () => void
}) => {
  // this is the memoized "data" array passed to child component to call useTable with;
  // the array contains one insight for each unique ruleId; each insight
  // has dailyAvoidableCost, totalCostToDate, dailyAvoidableEnergy, totalEnergyToDate
  // and totalOccurrences prop which adds up all sub row insight and display at parent level
  // when matched with ruleId, and each insight also has a subRowInsightIds
  // prop which contains ids of insights that are with same ruleId and status of "open"
  const groupedInsightsData: Insight[] = useMemo(
    () =>
      Object.entries(_.groupBy(filteredInsights, (i) => i?.[groupById])).map(
        ([modelId, groupInsights]) => {
          const costImpact = getTotalImpactScore({
            groupInsights,
            scoreName: InsightCostImpactPropNames.dailyAvoidableCost,
            multiplier: 365,
            language,
            isRollUpTotal: true,
            decimalPlaces: 0,
          })
          const totalCostImpact = getTotalImpactScore({
            groupInsights,
            scoreName: InsightCostImpactPropNames.totalCostToDate,
            language,
            isRollUpTotal: true,
            decimalPlaces: 0,
          })
          const energyImpact = getTotalImpactScore({
            groupInsights,
            scoreName: InsightCostImpactPropNames.dailyAvoidableEnergy,
            multiplier: 365,
            language,
            decimalPlaces: 0,
            isRollUpTotal: true,
          })
          const totalEnergyImpact = getTotalImpactScore({
            groupInsights,
            scoreName: InsightCostImpactPropNames.totalEnergyToDate,
            language,
            decimalPlaces: 0,
            isRollUpTotal: true,
          })
          const uniqueSiteNames = _.uniq(groupInsights.map((i) => i?.siteName))
          // we take status from insight.lastStatus for insight workflow project,
          // however, the legacy insight system takes status from insight.status
          // https://willow.atlassian.net/wiki/spaces/PE/pages/2482831361/Insights+to+Action+V2+Feature+Overview
          // https://willow.atlassian.net/wiki/spaces/PE/pages/2498625742/Tech+Notes+LastStatus+for+Insight+Workflow+associated+Dtos
          const uniqueStatuses = _.uniq(groupInsights.map((i) => i?.lastStatus))
          const uniqueTypes = _.uniq(groupInsights.map((i) => i?.type))
          const modelName =
            groupBy === InsightGroups.ASSET_TYPE
              ? getTranslatedModelName?.(modelId)
              : ''
          const getGroupedValue = (groupedValues: Array<string | undefined>) =>
            groupedValues.length > 1
              ? t('plainText.multiple')
              : groupedValues[0] ?? ''
          const insightStatusFromGroup = getGroupedValue(uniqueStatuses)
          const valuesOfOccurredDates = groupInsights.map((groupInsight) =>
            new Date(groupInsight[dateColumn.accessor]).valueOf()
          )
          const maxValue = Math.max(...valuesOfOccurredDates)
          const indexOfMaxValue = valuesOfOccurredDates.indexOf(maxValue)

          return {
            ...groupInsights[0],
            // setting default values on ruleName and modelName for the purpose of sorting,
            // so the value of "Ungrouped" and "No name found" will be sorted against
            // actual ruleName and modelName of an insight
            ruleName: groupInsights[0]?.ruleName ?? 'Ungrouped',
            modelName:
              modelName === '' || modelName == null
                ? 'No name found'
                : modelName,
            yearlyAvoidableCost: costImpact.totalImpactScore,
            // setting yearlyAvoidableCostValue to undefined
            // when costImpact.value is undefined helps to
            // always sort undefined values to the bottom of the table
            yearlyAvoidableCostValue:
              typeof costImpact?.value === 'number'
                ? costImpact.value * 365
                : undefined,
            totalCostToDate: totalCostImpact.totalImpactScore,
            totalCostToDateValue: totalCostImpact.value,
            yearlyAvoidableEnergy: energyImpact.totalImpactScore,
            yearlyAvoidableEnergyValue:
              typeof energyImpact?.value === 'number'
                ? energyImpact.value * 365
                : undefined,
            totalEnergyToDate: totalEnergyImpact.totalImpactScore,
            totalEnergyToDateValue: totalEnergyImpact.value,
            // take the latest occurred date of all insights in the group
            lastUpdatedOccurredDate:
              groupInsights[indexOfMaxValue][dateColumn.accessor],
            totalOccurrences: _.sum(
              groupInsights.map((i) => i.occurrenceCount)
            ),
            maxPriority: _.max(
              groupInsights.map(
                (i) =>
                  i?.impactScores?.find(
                    (impactScore) =>
                      impactScore?.name ===
                      InsightCostImpactPropNames.priorityScore
                  )?.value
              )
            ),
            totalCount: groupInsights.length,
            subRowInsightIds: groupInsights.map((i) => i.id),
            groupSiteName: getGroupedValue(uniqueSiteNames),
            groupTypes: getGroupedValue(uniqueTypes),
            // translate insight status, e.g. inProgress will be translated to "In progress" (if lang is English)
            // if status is not translatable, leave it as is
            groupStatus:
              getInsightStatusTranslatedName(t, insightStatusFromGroup) ??
              insightStatusFromGroup,
            groupEquipmentName: getGroupedValue(
              _.uniq(groupInsights.map((i) => i?.equipmentName))
            ),
          }
        }
      ),
    [filteredInsights, groupById, language]
  )

  // Getting Insight workflow columns
  const insightWorkflowColumns = useWorkflowGroupedColumns({
    language,
    t,
    selectedInsightIds,
    filteredInsights,
    groupedInsightsData,
    expandedGroupId,
    onSelectAllGroupedInsights,
    onSelectGroupedInsights,
    siteId,
    insightMetric,
    groupBy,
    groupByName,
    groupById,
    hideStatusColumn,
    dateColumn,
    isSavings,
  })

  const { getTableProps, getTableBodyProps, rows, prepareRow, headerGroups } =
    useTable(
      {
        columns: insightWorkflowColumns,
        data: groupedInsightsData,
        disableSortRemove: true,
        autoResetSortBy: false,
        initialState: {
          sortBy: localStorage.get(groupedTableSortByKey) ?? [
            {
              id: 'occurredDate',
              desc: true,
            },
          ],
        },
      },
      useSortBy
    )

  const memorizedFilteredInsights = useMemo(() => {
    const row = rows.find((r) => r?.original?.[groupById] === expandedGroupId)
    return filteredInsights.filter(
      (insight) => insight?.[groupById] === row?.original?.[groupById]
    )
  }, [expandedGroupId, filteredInsights, groupById, rows])

  return (
    <>
      {rows.length > 0 ? (
        <Table {...getTableProps()}>
          <StickyTHead>
            {headerGroups.map((headerGroup: HeaderGroup) => (
              <HeaderRow
                {...headerGroup.getHeaderGroupProps()}
                $isAllSites={siteId == null}
                $hideStatusColumn={hideStatusColumn}
              >
                {headerGroup.headers.map((column: EnhancedColumn) => (
                  <StyledTH
                    {...column.getHeaderProps(
                      customGetSortByToggleProps({
                        column,
                        localStorageKey: groupedTableSortByKey,
                      })
                    )}
                  >
                    <SortIndicator
                      isSorted={column.isSorted}
                      $transform={
                        column.isSortedDesc
                          ? 'translateY(14px)'
                          : 'translateY(-12px) rotate(-180deg)'
                      }
                    >
                      {column.render('Header')}
                    </SortIndicator>
                  </StyledTH>
                ))}
              </HeaderRow>
            ))}
          </StickyTHead>

          <TBody {...getTableBodyProps()} data-testid="groupedTableBody">
            {rows.map((row: Row) => {
              prepareRow(row)
              const isExpanded = expandedGroupId === row.original?.[groupById]

              return (
                <Fragment key={row.id}>
                  <StyledTR
                    {...row.getRowProps()}
                    $isAllSites={siteId == null}
                    $isExpanded={isExpanded}
                    $hideStatusColumn={hideStatusColumn}
                    onClick={() =>
                      onExpandGroup?.({
                        expandedGroupId: isExpanded
                          ? undefined
                          : row.original?.[groupById],
                      })
                    }
                    key={`parentRow-${row.key}`}
                    top="0px"
                  >
                    {row.cells.map((cell: Cell) => (
                      <StyledTD
                        {...cell.getCellProps()}
                        $isAllSites={siteId == null}
                        $isExpanded={isExpanded}
                      >
                        {cell.render('Cell')}
                      </StyledTD>
                    ))}
                  </StyledTR>
                  {isExpanded && (
                    // wrap subtable in <tr> and <td> as
                    // the highest level element in the subtable is a <div>
                    // which is not allowed in a <tbody> as direct child
                    <tr>
                      <td>
                        <ImpactInsightsUngroupedTable
                          isGrouped
                          filteredInsights={memorizedFilteredInsights}
                          selectedInsight={selectedInsight}
                          onSelectInsight={onSelectInsight}
                          siteId={siteId}
                          selectedInsightIds={selectedInsightIds}
                          isInsightIdSelected={isInsightIdSelected}
                          onSelectedInsightIds={onSelectedInsightIds}
                          onToggleSelectedInsightId={onToggleSelectedInsightId}
                          // this prop helps to limit number of visible
                          // AssetDetailsModal to 1 since ImpactInsightsGroupedTable
                          // will render one ImpactInsightsUngroupedTable for each
                          // ruleId, and each ImpactInsightsUngroupedTable
                          // renders an AssetDetailsModal. Note AssetDetailsModal
                          // needs to be rendered inside ImpactInsightsUngroupedTable
                          // to have access to the sorted data
                          shouldShowDetailModal={
                            selectedInsight?.[groupById] ===
                            row.original?.[groupById]
                          }
                          dataSegmentPropPage={dataSegmentPropPage}
                          insightMetric={insightMetric}
                          paginationEnabled={paginationEnabled}
                          pageSize={pageSize}
                          initialPageIndex={initialPageIndex}
                          onPageSizeChange={onPageSizeChange}
                          language={language}
                          t={t}
                          insightTab={insightTab}
                          onInsightTabChange={onInsightTabChange}
                          hideStatusColumn={hideStatusColumn}
                          dateColumn={dateColumn}
                          isSavings={isSavings}
                          tab={tab}
                          clearSelectedInsightIds={clearSelectedInsightIds}
                        />
                      </td>
                    </tr>
                  )}
                </Fragment>
              )
            })}
          </TBody>
        </Table>
      ) : (
        <NotFound>{t('plainText.noInsightsFound')}</NotFound>
      )}
    </>
  )
}

const StickyTHead = styled(THead)({
  position: 'sticky',
  top: '0px',
  zIndex: 110,
})

const gridStyles = ({ $isAllSites, $hideStatusColumn }) => {
  const gridTemplateColumns = [
    '44px', // Chevron icon and header checkbox
    '44px', // row level checkbox
    $hideStatusColumn ? '' : '136px', // Status
    'minmax(80px, 2fr)', // Rule / Asset Type Name
    'minmax(80px, 1fr)', // Asset Name displayed as link
    `minmax(40px, 300px)`, // Type
    $isAllSites ? '125px' : '', // Site name when all site is selected
    'minmax(100px, 1fr)', // Avoidable cost/energy per year
    '90px', // priority
    `minmax(80px, 152px)`, // last occurred
  ]

  const formattedGridTemplateColumns = gridTemplateColumns
    .filter((v) => v != null)
    .join(' ')
  return {
    display: 'grid',
    gridTemplateColumns: formattedGridTemplateColumns,
  }
}

const HeaderRow = styled(TR)<{
  $isAllSites: boolean
  $hideStatusColumn: boolean
}>(({ $isAllSites, $hideStatusColumn, theme }) => ({
  ...gridStyles({ $isAllSites, $hideStatusColumn }),
  filter: 'drop-shadow(0px 3px 6px rgba(0, 0, 0, 0.160784))',
  height: theme.spacing.s48,
}))

const StyledTH = styled(TH)(({ theme }) => ({
  cursor: 'pointer',
  display: 'flex',
  flexDirection: 'column',
  justifyContent: 'center',
  overflow: 'hidden',
  color: theme.color.neutral.fg.default,
  font: 'normal 500 12px/20px Poppins',

  ...theme.font.heading.xs,

  '&:first-child': {
    padding: '0 6px',
  },

  '& > div': {
    whiteSpace: 'nowrap',
  },
  '&:hover': {
    color: theme.color.neutral.fg.highlight,
  },
  '&&& span': {
    lineHeight: theme.font.heading.xs.lineHeight,

    '& > span': {
      display: 'block',
    },
  },
}))

const StyledTR = styled(TR)<{
  $isAllSites: boolean
  $isExpanded: boolean
  $hideStatusColumn: boolean
}>(({ $isAllSites, $isExpanded, $hideStatusColumn, theme }) => ({
  height: theme.spacing.s48,
  marginTop: theme.spacing.s24,
  ...gridStyles({ $isAllSites, $hideStatusColumn }),

  borderTop: `1px solid ${theme.color.neutral.border.default}`,
  backgroundColor: $isExpanded
    ? theme.color.neutral.bg.accent.activated
    : theme.color.neutral.bg.panel.default,
  color: $isExpanded
    ? theme.color.neutral.fg.highlight
    : theme.color.neutral.fg.default,
  top: theme.spacing.s48,
  position: 'sticky',
  zIndex: 100,

  '&:hover': {
    cursor: 'pointer',
    '& td': {
      color: theme.color.neutral.fg.highlight,
    },
  },
}))

const StyledTD = styled(TD)<{
  $isExpanded: boolean
}>(({ $isExpanded, theme }) => ({
  alignItems: 'center',
  display: 'flex',

  color: $isExpanded
    ? theme.color.neutral.fg.highlight
    : theme.color.neutral.fg.default,

  '&:first-child': {
    justifyContent: 'center',
  },

  // Changing the background color for checkbox when row is expanded
  '&:nth-child(2)': {
    'button > div > div': {
      backgroundColor: $isExpanded
        ? theme.color.intent.secondary.fg.default
        : 'none',
    },
  },
}))

const MemorizedTable = memo(ImpactInsightsGroupedTable)
export default MemorizedTable

const groupedTableSortByKey = 'groupedTableSortBy'
