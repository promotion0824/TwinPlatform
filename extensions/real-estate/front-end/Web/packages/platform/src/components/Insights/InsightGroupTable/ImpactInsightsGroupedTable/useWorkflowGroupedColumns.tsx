import { useMemo } from 'react'
import { Row } from 'react-table'
import _ from 'lodash'
import tw, { styled } from 'twin.macro'
import { TFunction } from 'react-i18next'
import {
  InsightCostImpactPropNames,
  InsightMetric,
  titleCase,
  sortOccurrenceDate,
} from '@willow/common'
import { Checkbox, Icon, Text, InsightGroups } from '@willow/ui'
import { Language } from '@willow/ui/providers/LanguageProvider/LanguageJson/LanguageJsonService/LanguageJsonService'
import { Insight } from '@willow/common/insights/insights/types'
import { TextWithTooltip } from '@willow/common/insights/component'

export default function useWorkflowGroupedColumns({
  language,
  t,
  filteredInsights,
  expandedGroupId,
  selectedInsightIds,
  groupedInsightsData,
  onSelectAllGroupedInsights,
  onSelectGroupedInsights,
  siteId,
  insightMetric = InsightMetric.cost,
  groupBy,
  groupByName,
  groupById,
  hideStatusColumn = false,
  dateColumn,
  isSavings,
}: {
  language: Language
  t: TFunction
  filteredInsights: Insight[]
  selectedInsightIds: string[]
  groupedInsightsData: Insight[]
  expandedGroupId?: string
  onSelectAllGroupedInsights: (
    allGroupedInsights: Insight[],
    isAllDisplayedInsightsSelected: boolean
  ) => void
  onSelectGroupedInsights: (
    isEveryInsightChecked: boolean,
    rowData: Insight
  ) => void
  siteId?: string
  insightMetric: string
  groupBy: InsightGroups
  groupByName: string
  groupById: string
  hideStatusColumn?: boolean
  dateColumn: {
    columnText: string
    accessor: string
  }
  isSavings?: boolean
}) {
  const columns = useMemo(
    () => [
      {
        Header: () => {
          // Getting all the insight IDs present in the grouped table
          const allOpenInsightIds = groupedInsightsData.reduce(
            (a: string[], b) => a.concat(b.subRowInsightIds ?? []),
            []
          )
          // Here fetching only those insight IDs which are selected and displayed in the table
          const visibleSelectedInsightIds = _.intersection(
            selectedInsightIds,
            allOpenInsightIds
          )
          const isAllDisplayedInsightsSelected =
            visibleSelectedInsightIds.length === allOpenInsightIds.length
          return (
            <Checkbox
              value={isAllDisplayedInsightsSelected}
              aria-label={t('interpolation.actionOnAllInsight', {
                action: isAllDisplayedInsightsSelected
                  ? t('plainText.deselect')
                  : t('plainText.select'),
              })}
              onClick={(e) => {
                e?.stopPropagation()
                onSelectAllGroupedInsights(
                  groupedInsightsData,
                  isAllDisplayedInsightsSelected
                )
              }}
            />
          )
        },
        id: 'headerCheckbox',
        accessor: 'externalId',
        Cell: ({ row }) => (
          // Chevron used for displaying expanded or collapsed state of grouped insights
          <IconContainer>
            <ChevronContainer>
              <ChevronIcon
                icon="chevron"
                size="medium"
                $isExpanded={expandedGroupId === row.original?.[groupById]}
              />
            </ChevronContainer>
          </IconContainer>
        ),
        disableSortBy: true,
      },
      {
        id: 'rowCheckbox',
        accessor: 'checkbox',
        Cell: ({ row }) => {
          const isEveryInsightChecked = row.original.subRowInsightIds.every(
            (insightId) => selectedInsightIds.includes(insightId)
          )

          return (
            <Checkbox
              aria-label={`${
                isEveryInsightChecked
                  ? t('plainText.deselect')
                  : t('plainText.select')
              } ${row.original?.[groupByName] ?? t('plainText.ungrouped')}`}
              value={isEveryInsightChecked}
              onClick={(e) => {
                e?.stopPropagation()
                onSelectGroupedInsights(isEveryInsightChecked, row.original)
              }}
            />
          )
        },
        disableSortBy: true,
      },
      ...(!hideStatusColumn
        ? [
            {
              Header: t('labels.status'),
              accessor: 'groupStatus',
            },
          ]
        : []),
      {
        Header: (
          <TextWithTooltip
            text={
              groupBy === InsightGroups.RULE
                ? t(`plainText.rule`)
                : titleCase({ text: t(`plainText.assetType`), language })
            }
          />
        ),
        id: groupByName,
        accessor: groupById,
        sortType: getSortTypeFunc(groupByName),
        Cell: ({ value, row }) => {
          const groupByNameOnRow = row.original?.[groupByName]
          return (
            <TextWithTooltip
              text={
                !value || groupByNameOnRow == null
                  ? titleCase({
                      text:
                        groupBy !== InsightGroups.ASSET_TYPE
                          ? t('plainText.ungrouped')
                          : t('plainText.noNameFound'),
                      language,
                    })
                  : groupByNameOnRow
              }
              tooltipWidth="200px"
              isTitleCase={false}
            />
          )
        },
      },
      {
        Header: <TextWithTooltip text={t('plainText.asset')} />,
        accessor: 'groupEquipmentName',
        Cell: ({ value }) => (
          <TextWithTooltip text={value} isTitleCase={false} />
        ),
      },
      {
        Header: <TextWithTooltip text={t('labels.type')} />,
        accessor: 'groupTypes',
        Cell: ({ value }) => <TextWithTooltip text={value} />,
      },
      // this column is only present when All Site is selected
      ...(siteId == null
        ? [
            {
              Header: t('labels.site'),
              accessor: 'groupSiteName',
              Cell: ({ value }) => <TextWithTooltip text={value} />,
            },
          ]
        : []),
      ...(insightMetric === InsightMetric.cost
        ? [
            {
              Header: (
                <AvoidableExpPerYearHeader
                  insightMetric={insightMetric}
                  t={t}
                  language={language}
                  isSavings={isSavings}
                />
              ),
              id: 'yearlyCost',
              accessor: 'yearlyAvoidableCost',
              sortType: getSortTypeFunc(
                InsightCostImpactPropNames.yearlyAvoidableCostValue
              ),
              sortDescFirst: true,
              Cell: FormattedCell,
            },
          ]
        : [
            {
              Header: (
                <AvoidableExpPerYearHeader
                  insightMetric={insightMetric}
                  t={t}
                  language={language}
                  isSavings={isSavings}
                />
              ),
              id: 'yearlyEnergy',
              accessor: 'yearlyAvoidableEnergy',
              sortType: getSortTypeFunc(
                InsightCostImpactPropNames.yearlyAvoidableEnergyValue
              ),
              sortDescFirst: true,
              Cell: FormattedCell,
            },
          ]),
      /**
       *  A hidden column to make sure 'priority' column of sub table aligns properly with main table ....
       */
      {
        id: 'placeholderForPriority',
        disableSortBy: true,
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
        accessor: 'lastUpdatedOccurredDate',
        Cell: ({ value }: { value: string }) => (
          <TextWithTooltip text={value} />
        ),
        sortDescFirst: true,
        sortType: sortOccurrenceDate(),
      },
    ],
    [
      groupBy,
      t,
      groupByName,
      groupById,
      insightMetric,
      siteId,
      groupedInsightsData,
      selectedInsightIds,
      onSelectAllGroupedInsights,
      expandedGroupId,
      filteredInsights,
      onSelectGroupedInsights,
      language,
    ]
  )

  return columns
}

const FormattedText = styled(Text)({
  whiteSpace: 'nowrap',
  overflow: 'hidden',
  textOverflow: 'ellipsis',
})

const IconContainer = styled.div({
  display: 'flex',
  flexDirection: 'column',
  position: 'relative',
  justifyContent: 'center',
})

const ChevronContainer = styled.div({
  position: 'absolute',
  width: '100%',
  display: 'flex',
  justifyContent: 'center',
})

const ChevronIcon = styled(Icon)<{ $isExpanded: boolean }>`
  transform: ${({ $isExpanded }) => ($isExpanded ? 'rotate(-180deg)' : 'none')};
`

// TODO: Removed below functions/components once the functionality of V3 insights table is completed
const getSortTypeFunc =
  (name: string) =>
  (rowA: Row, rowB: Row, _columnId: string, desc: boolean) => {
    const scoreA = rowA?.original?.[name]
    const scoreB = rowB?.original?.[name]

    // do not sort when both scores are undefined or same value
    if ((scoreA == null && scoreB == null) || scoreA === scoreB) return 0

    // place a defined value before undefined value regardless of sorting order
    if (scoreB == null && scoreA != null) return desc ? 1 : -1
    if (scoreA == null && scoreB != null) return desc ? -1 : 1

    return scoreA > scoreB ? 1 : -1
  }

export const AvoidableExpPerYearHeader = ({
  t,
  insightMetric,
  language,
  isSavings = false,
}: {
  t: TFunction
  insightMetric: string
  language: string
  isSavings?: boolean
}) => (
  <span
    data-tooltip={
      isSavings
        ? t('plainText.savingsPerYearTooltip')
        : _.capitalize(
            t('interpolation.avoidableExpensePerYearTable', {
              expense:
                insightMetric === InsightMetric.cost
                  ? t('plainText.cost')
                  : t('plainText.energyUsage'),
            })
          )
    }
    data-tooltip-position="top"
    data-tooltip-width="242px"
    data-tooltip-time={500}
    tw="h-full w-full flex items-center"
  >
    <FormattedText tw="block">
      {isSavings
        ? `${titleCase({
            text: t('plainText.savingsPerYear'),
            language,
          })} *`
        : `${titleCase({
            text: t('interpolation.avoidableExpensePerYear', {
              expense: t(`plainText.${insightMetric}`),
            }),
            language,
          })} *`}
    </FormattedText>
  </span>
)

export const ExpToDateHeader = ({
  t,
  insightMetric,
  language,
}: {
  t: TFunction
  insightMetric: string
  language: string
}) => (
  <span
    data-tooltip={_.capitalize(
      t('interpolation.avoidableExpenseToDateTable', {
        expense:
          insightMetric === InsightMetric.cost
            ? t('plainText.cost')
            : t('plainText.energy'),
      })
    )}
    data-tooltip-position="top"
    data-tooltip-width="242px"
    data-tooltip-time={500}
    tw="h-full w-full flex items-center"
  >
    <FormattedText tw="block">
      {`${titleCase({
        text: t('interpolation.expenseToDate', {
          expense: t(`plainText.${insightMetric}`),
        }),
        language,
      })} *`}
    </FormattedText>
  </span>
)

export const FormattedCell = ({ value }) => <span>{value}</span>
