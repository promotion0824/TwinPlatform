/* eslint-disable complexity */
import tw, { styled } from 'twin.macro'
import { css } from 'styled-components'
import {
  useTable,
  HeaderGroup,
  EnhancedColumn,
  Row,
  Cell,
  useSortBy,
  usePagination,
  ColumnInstance,
} from 'react-table'
import {
  SortIndicator,
  TableComponents,
  Icon,
  Select,
  Option,
  NotFound,
} from '@willow/ui'
import { localStorage } from '@willow/common'
import { IconButton } from '@willowinc/ui'
import { Language } from '@willow/ui/providers/LanguageProvider/LanguageJson/LanguageJsonService/LanguageJsonService'
import { TFunction } from 'react-i18next'
import {
  Insight,
  InsightStatus,
  InsightWorkflowStatus,
} from '@willow/common/insights/insights/types'
import { useEffect, memo, useState } from 'react'
import AssetDetailsModal, {
  Item,
} from '../../../AssetDetailsModal/AssetDetailsModal'
import useWorkflowUngroupedColumns from './useWorkflowUngroupedColumns'
import { InsightActions } from '../../ui/ActionsViewControl'

const { Table, TBody, TD, TH, TR, THead } = TableComponents

const ImpactInsightsUngroupedTable = ({
  selectedInsight,
  selectedInsightIds,
  onToggleSelectedInsightId,
  isInsightIdSelected,
  onSelectedInsightIds,
  onSelectInsight,
  filteredInsights,
  siteId,
  isGrouped = false,
  shouldShowDetailModal = true,
  dataSegmentPropPage,
  insightMetric,
  paginationEnabled,
  pageSize: initialPageSize,
  initialPageIndex = 0,
  onPageSizeChange,
  language,
  t,
  insightTab,
  onInsightTabChange,
  dateColumn,
  hideStatusColumn = false,
  isSavings,
  tab,
  onInsightIdChange,
  clearSelectedInsightIds,
}: {
  filteredInsights: Insight[]
  selectedInsightIds: string[]
  onToggleSelectedInsightId: (insightId: string) => void
  isInsightIdSelected: (insightId: string) => boolean
  onSelectedInsightIds: (selectedInsightIds: string[]) => void
  onSelectInsight: (insight?: Insight) => void
  selectedInsight?: Insight
  siteId?: string
  isGrouped?: boolean
  shouldShowDetailModal?: boolean
  dataSegmentPropPage?: string
  insightMetric: string
  paginationEnabled?: boolean
  pageSize?: number
  initialPageIndex?: number
  onPageSizeChange?: (pageSize: number) => void
  language: Language
  t: TFunction
  insightTab: string
  onInsightTabChange: (insightTab: string) => void
  dateColumn: {
    columnText: string
    accessor: string
  }
  hideStatusColumn?: boolean
  isSavings?: boolean
  tab?: InsightStatus | InsightWorkflowStatus
  onInsightIdChange?: (insightId?: string) => void
  clearSelectedInsightIds?: () => void
}) => {
  if (filteredInsights.length === 0) {
    return <NotFound>{t('plainText.noInsightsFound')}</NotFound>
  }
  const [selectedAction, setSelectedAction] = useState<string | undefined>(
    undefined
  )
  // local state to control which modal is open, could be
  // insight, ticket, or confirmation (delete or resolve an insight) modal
  const [selectedItem, setSelectedItem] = useState<Item | undefined>(
    selectedInsight ? { ...selectedInsight, modalType: 'insight' } : undefined
  )

  const handleSelectAction = (action: string, rowContent: Insight) => {
    setSelectedAction(action)
    handleInsightChange(rowContent)
    setSelectedItem(
      action === InsightActions.newTicket
        ? {
            insightId: rowContent.id,
            insightName: rowContent.sequenceNumber,
            insightStatus: rowContent.status,
            siteId: rowContent.siteId,
            modalType: 'newTicket',
          }
        : action === InsightActions.delete
        ? {
            modalType: 'deleteInsightsConfirmation',
          }
        : action === InsightActions.resolve
        ? {
            modalType: 'resolveInsightConfirmation',
          }
        : action === InsightActions.report
        ? {
            modalType: 'report',
          }
        : {
            ...rowContent,
            modalType: 'insight',
          }
    )
  }

  // Getting Insight workflow columns
  const insightWorkflowColumns = useWorkflowUngroupedColumns({
    language,
    t,
    selectedInsightIds,
    filteredInsights,
    siteId,
    insightMetric,
    isGrouped,
    isInsightIdSelected,
    onSelectedInsightIds,
    onToggleSelectedInsightId,
    hideStatusColumn,
    dateColumn,
    isSavings,
    tab,
    onSelectAction: handleSelectAction,
  })

  const {
    getTableProps,
    getTableBodyProps,
    headerGroups,
    prepareRow,
    rows,
    page,
    canPreviousPage,
    canNextPage,
    pageCount,
    gotoPage,
    nextPage,
    previousPage,
    setPageSize,
    state: { pageIndex, pageSize },
  } = useTable(
    {
      columns: insightWorkflowColumns,
      data: filteredInsights,
      disableSortRemove: true,
      autoResetSortBy: false,
      initialState: {
        ...(paginationEnabled
          ? { pageIndex: initialPageIndex, pageSize: initialPageSize }
          : {}),
        sortBy: localStorage.get(innerTableSortByKey) ?? [
          {
            id: 'occurredDate',
            desc: true,
          },
        ],
      },
      autoResetPage: false,
    },
    useSortBy,
    usePagination
  )

  // go to the page that contains the selected insight
  // in case it is not in the current page
  useEffect(() => {
    if (paginationEnabled) {
      const expectedPageIndex = Math.floor(
        rows.findIndex(
          (row: { original: { id: string } }) =>
            row.original.id === selectedInsight?.id
        ) / pageSize
      )
      if (expectedPageIndex !== pageIndex) {
        gotoPage(expectedPageIndex)
      }
      // pageCount starts at 0 and pageIndex starts at 1
      if (pageCount < pageIndex + 1) {
        gotoPage(pageCount - 1)
      }
    }
  }, [
    gotoPage,
    pageCount,
    pageIndex,
    pageSize,
    paginationEnabled,
    rows,
    selectedInsight?.id,
  ])

  const handleRowClick = (event: React.MouseEvent, rowContent: Insight) => {
    event.stopPropagation()
    onSelectInsight(rowContent)
    setSelectedItem({ ...rowContent, modalType: 'insight' })
    setSelectedAction(undefined)
  }

  const handleInsightChange = (rowContent?: Insight) => {
    // onInsightIdChange sets insightId query param in url when user clicks on
    // an action icon belongs to an insight row; relevant to Insight Card View
    // since it doesn't display insight modal so we need to set insightId query param
    // for other modals to work properly
    if (onInsightIdChange) {
      onInsightIdChange(rowContent?.id)
    } else {
      // onSelectInsight sets selectedInsight in InsightsContext when user clicks on
      // an action icon belongs to an insight row
      onSelectInsight(rowContent)
    }
  }

  /**
   * resets states and close the current opened modal;
   * however, if user is on resolve confirmation modal and open a ticket modal,
   * closing ticket modal will open the resolve confirmation modal because marking
   * ticket status as "closed" is prerequisite to resolve an insight.
   * reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/80180
   */
  const handleModalClose = () => {
    if (
      selectedAction === InsightActions.resolve &&
      selectedItem?.modalType === 'ticket'
    ) {
      setSelectedItem({
        modalType: 'resolveInsightConfirmation',
      })
    } else {
      setSelectedItem(undefined)
      onSelectInsight(undefined)
      setSelectedAction(undefined)
      onInsightIdChange?.(undefined)
    }
  }

  const renderedRows = paginationEnabled ? page : rows

  return (
    <>
      <TableContainer $isGrouped={isGrouped}>
        <Table {...getTableProps()}>
          <StickyTHead $isGrouped={isGrouped}>
            {headerGroups.map((headerGroup: HeaderGroup) => (
              <HeaderRow
                {...headerGroup.getHeaderGroupProps()}
                $isAllSites={siteId == null}
                $isGrouped={isGrouped}
                $hideStatusColumn={hideStatusColumn}
              >
                {headerGroup.headers.map((column: EnhancedColumn) => (
                  <StyledTH
                    $isHidden={column.hideSortingIcons}
                    $isGrouped={isGrouped}
                    // Setting title to undefined to remove the default tooltip provided by react-table
                    {...column.getHeaderProps(
                      customGetSortByToggleProps({
                        column,
                        localStorageKey: innerTableSortByKey,
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

          <TBody {...getTableBodyProps()}>
            {renderedRows.map((row: Row) => {
              prepareRow(row)
              return (
                <StyledTR
                  {...row.getRowProps()}
                  $isGrouped={isGrouped}
                  $isAllSites={siteId == null}
                  $hideStatusColumn={hideStatusColumn}
                  onClick={(event) => handleRowClick(event, row.original)}
                  selected={selectedInsight?.id === row.original.id}
                  data-testid={row.original?.sequenceNumber ?? row.original.id}
                  ref={(node: HTMLTableRowElement) => {
                    if (
                      shouldShowDetailModal &&
                      selectedInsight?.id === row.original.id &&
                      node != null
                    ) {
                      // get the bounding client rect of the element
                      const rect = node.getBoundingClientRect()
                      // please refer to the following file and note that
                      // the whole insight table is rendered under <Content /> in <Tabs />
                      // and <Content /> has role of "tabpanel" and serve as the container of
                      // content rendered inside Tabs
                      // packages\platform\src\components\Insights\InsightsContent.tsx
                      const parent = node.closest('[role="tabpanel"]')
                      const parentRect = parent?.getBoundingClientRect()

                      if (
                        parent &&
                        parentRect &&
                        rect.top >= 0 &&
                        rect.top <= parentRect.top
                      ) {
                        // we add 48px at the end because insight table's header
                        // row is fixed in position, and it has height of 48px
                        parent.scrollTop -= parentRect.top - rect.top + 48
                      }
                      // check if the element is already in view
                      else if (
                        rect.top >= 0 &&
                        rect.bottom <= window.innerHeight
                      ) {
                        // the element is already in view, do nothing
                      } else {
                        node.scrollIntoView({
                          behavior: 'smooth',
                          block: 'end',
                        })
                      }
                    }
                  }}
                >
                  {row.cells.map((cell: Cell) => (
                    <StyledTD
                      {...cell.getCellProps()}
                      $isGrouped={isGrouped}
                      $isAllSites={siteId == null}
                      /**
                       *  Status & Priority column should not be Semi-Bolded in `New Status` Rows in Insights Table....
                       *  Link: https://dev.azure.com/willowdev/Unified/_workitems/edit/87605
                       */
                      $isSemiBold={
                        !['lastStatus', 'priority'].includes(cell.column.id) &&
                        row.original.lastStatus === 'new'
                      }
                    >
                      {/**
                       * Condition for occurence count so that it takes the width specified
                       * in the table grid for that column and hide the count in UI
                       */}
                      {cell.column.id === 'occurrenceCount'
                        ? ''
                        : cell.render('Cell')}
                    </StyledTD>
                  ))}
                </StyledTR>
              )
            })}
          </TBody>
          {/*
            to minimize the chance of content jumping
            https://css-tricks.com/books/greatest-css-tricks/pin-scrolling-to-bottom/
          */}
          <tfoot
            id="anchor"
            css={css`
              height: 1px;
              overflowanchor: auto;
            `}
          />
        </Table>
        {paginationEnabled && pageCount > 1 && (
          <div tw="h-[32px] flex mt-[8px] mr-[100px] justify-end">
            <Select
              tw="h-[32px] ml-2"
              value={t('interpolation.numberPerPage', { number: pageSize })}
            >
              {[10, 20, 30].map((number) => (
                <Option
                  key={number}
                  value={number}
                  onClick={() => {
                    setPageSize(number)
                    onPageSizeChange?.(number)
                  }}
                >
                  {t('interpolation.numberPerPage', { number })}
                </Option>
              ))}
            </Select>
            {[
              {
                icon: 'chevronBack',
                onClick: () => gotoPage(0),
                disabled: !canPreviousPage,
                testId: 'toFirstPage',
              },
              {
                icon: 'chevronRight',
                style: { transform: 'rotate(180deg)' },
                onClick: previousPage,
                disabled: !canPreviousPage,
                testId: 'toPreviousPage',
              },
              {
                icon: 'chevronRight',
                onClick: nextPage,
                disabled: !canNextPage,
                testId: 'toNextPage',
              },
              {
                icon: 'chevronFwd',
                onClick: () => gotoPage(pageCount - 1),
                disabled: !canNextPage,
                testId: 'toLastPage',
              },
            ].map(({ icon, onClick, disabled, style, testId }) => (
              <StyledIconButton
                key={`${icon}-${testId}`}
                kind="secondary"
                background="transparent"
                onClick={onClick}
                disabled={disabled}
                style={style}
                data-testid={testId}
              >
                <Icon icon={icon} />
              </StyledIconButton>
            ))}
            <StyledDiv tw="leading-8">
              {t('interpolation.pageNumberofRecords', {
                number: pageIndex ? pageSize * pageIndex + 1 : 1,
                total: !canNextPage
                  ? filteredInsights.length
                  : pageSize * (pageIndex + 1),
                totalRecords: filteredInsights.length,
              })}
            </StyledDiv>
          </div>
        )}
      </TableContainer>
      {selectedInsight != null && shouldShowDetailModal && (
        <AssetDetailsModal
          siteId={selectedInsight.siteId}
          item={{
            ...selectedInsight,
            modalType: 'insight',
          }}
          onClose={handleModalClose}
          navigationButtonProps={{
            items: renderedRows.map((row) => row.original),
            selectedItem: selectedInsight,
            setSelectedItem: onSelectInsight as (selected) => void,
          }}
          dataSegmentPropPage={dataSegmentPropPage ?? 'Insights Details'}
          insightTab={insightTab}
          onInsightTabChange={onInsightTabChange}
          canDeleteInsight={siteId != null}
          controlledCurrentItem={selectedItem}
          onControlledCurrentItemChange={setSelectedItem}
          onActionChange={setSelectedAction}
          selectedInsightIds={
            selectedInsight ? [selectedInsight.id] : selectedInsightIds
          }
          onClearSelectedInsightIds={clearSelectedInsightIds}
        />
      )}
    </>
  )
}

const TableContainer = styled.div<{ $isGrouped: boolean }>(
  ({ $isGrouped }) => ({
    position: 'relative',
    paddingBottom: $isGrouped ? '12px' : '0px',
  })
)

const StickyTHead = styled(THead)<{ $isGrouped: boolean }>(
  ({ $isGrouped }) => ({
    position: 'sticky',
    top: $isGrouped ? '95px' : '0px',
    zIndex: '1',
  })
)

const gridStyles = ({
  $isAllSites,
  $isGrouped,
  $hideStatusColumn,
}: {
  $isAllSites: boolean
  $isGrouped: boolean
  $hideStatusColumn?: boolean
}) => {
  const gridTemplateColumns: string[] = [
    $isGrouped ? '88px' : '64px',
    $hideStatusColumn ? '' : '136px', // status
    `minmax(80px, 2fr)`, // insight summary/description
    `minmax(80px, 1fr)`, // Asset Name displayed as link
    `minmax(40px, 105px)`, // Type
    `minmax(80px, 195px)`, // Activity
    // TODO: hide History column for now, need designer tell us what to do
    // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/84994
    // `minmax(40px, 105px)`,
    $isAllSites ? '125px' : '', // Site name when all site is selected
    'minmax(100px, 1fr)', // avoidable cost/energy per year
    '90px', // priority
    '120px', // occurred date
    '32px', // action icon for insights
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
  $isGrouped: boolean
  $hideStatusColumn: boolean
}>(({ $isAllSites, $isGrouped, $hideStatusColumn }) => ({
  ...gridStyles({
    $isAllSites,
    $isGrouped,
    $hideStatusColumn,
  }),
  filter: 'drop-shadow(0px 3px 6px rgba(0, 0, 0, 0.160784))',
}))

const StyledTH = styled(TH)<{
  $isGrouped: boolean
  $isHidden: boolean
}>(({ $isGrouped, $isHidden, theme }) => ({
  cursor: 'pointer',
  display: 'flex',
  flexDirection: 'column',
  justifyContent: 'center',
  overflow: 'hidden',
  font: 'normal 500 12px/20px Poppins',
  ...theme.font.heading.xs,
  color: theme.color.neutral.fg.default,
  background: $isGrouped
    ? theme.color.neutral.bg.accent.default
    : theme.color.neutral.bg.panel.default,

  ':first-child': {
    alignItems: $isGrouped ? 'end' : 'left',
    padding: `0 ${theme.spacing.s12}`,
  },

  '& > div': {
    whiteSpace: 'nowrap',
    display: $isHidden ? 'none' : '',
  },
  '&:hover': {
    color: theme.color.neutral.fg.highlight,
  },
}))

const StyledTR = styled(TR)<{
  selected: boolean
  $isAllSites: boolean
  $isGrouped: boolean
  $hideStatusColumn: boolean
}>(({ selected, $isAllSites, $isGrouped, $hideStatusColumn, theme }) => ({
  height: '48px',
  ...gridStyles({
    $isAllSites,
    $isGrouped,
    $hideStatusColumn,
  }),
  backgroundColor: selected
    ? theme.color.neutral.bg.accent.activated
    : theme.color.neutral.bg.panel.default,
  color: theme.color.neutral.fg.default,

  '&:hover': {
    cursor: 'pointer',
    backgroundColor: theme.color.neutral.bg.accent.hovered,

    '&&& td': {
      color: theme.color.neutral.fg.highlight,
    },
  },
}))

const StyledTD = styled(TD)<{
  $isGrouped: boolean
  $isSemiBold: boolean
}>(({ theme, $isGrouped, $isSemiBold }) => ({
  alignItems: 'center',
  display: 'flex',

  ':first-child': {
    padding: '0 12px',
    justifyContent: $isGrouped ? 'end' : 'left',
  },

  '&&& *': {
    fontWeight: $isSemiBold
      ? theme.font.body.md.semibold.fontWeight
      : 'inherit',
  },

  '&:nth-child(2)': {
    '& span': {
      maxWidth: '95%',
      textOverflow: 'ellipsis',
      whiteSpace: 'nowrap',
    },
  },
}))

const MemorizedTable = memo(ImpactInsightsUngroupedTable)
export default MemorizedTable

const StyledDiv = styled.div`
  line-height: 32px;
  font: 400 12px/32px Poppins;
`

const StyledIconButton = styled(IconButton)<{
  disabled: boolean
}>(({ disabled, theme }) => ({
  height: theme.spacing.s32,
  color: disabled ? theme.color.state.disabled.fg : '',
}))

const innerTableSortByKey = 'innerTableSortBy'

/**
 * save sortBy option in localStorage when user click on header row cell
 * so table's sortBy will be persistent
 */
export const customGetSortByToggleProps = ({
  column,
  localStorageKey,
}: {
  column: ColumnInstance
  localStorageKey: string
}) => {
  const sortByToggleProps = column.getSortByToggleProps({ title: undefined })

  return {
    ...sortByToggleProps,
    onClick: (e: MouseEvent) => {
      // Call the original onClick handler to maintain React Table functionality
      // function will not be available if the column is hidden
      if (sortByToggleProps?.onClick) {
        sortByToggleProps.onClick(e)

        localStorage.set(localStorageKey, [
          { id: column.id, desc: !column.isSortedDesc },
        ])
      }
    },
  }
}
