/* eslint-disable complexity */
import {
  FullSizeContainer,
  FullSizeLoader,
  InsightCostImpactPropNames,
  InsightMetric,
  formatDateTime,
  getImpactScore,
  titleCase,
} from '@willow/common'
import {
  ActivityCount,
  InsightTypeBadge,
  PriorityName,
} from '@willow/common/insights/component'
import { Insight } from '@willow/common/insights/insights/types'
import { getModelOfInterest } from '@willow/common/twins/view/modelsOfInterest'
import {
  ALL_SITES,
  TwinChip,
  dateComparator,
  useFeatureFlag,
  useScopeSelector,
} from '@willow/ui'
import {
  DataGrid,
  GRID_CHECKBOX_SELECTION_COL_DEF,
  GridColDef,
  Icon,
  IconName,
  Indicator,
} from '@willowinc/ui'
import _ from 'lodash'
import { useMemo, useState } from 'react'
import { styled } from 'twin.macro'
import SiteChip from '../../../../views/Portfolio/twins/page/ui/SiteChip'
import { TwinLink } from '../../../../views/Portfolio/twins/shared/index'
import AssetDetailsModal, {
  ModalType,
} from '../../../AssetDetailsModal/AssetDetailsModal'
import InsightWorkflowStatusPill from '../../../InsightStatusPill/InsightWorkflowStatusPill'
import ActionsViewControl, { InsightActions } from '../../ui/ActionsViewControl'
import NotFound from '../../ui/NotFound'
import { useInsightsContext } from '../InsightsContext'
import CustomPagination from './CustomPagination'

export default function AllInsightsDataGrid() {
  const { isScopeUsedAsBuilding, location } = useScopeSelector()
  const [isActionsViewOpen, setIsActionsViewOpen] = useState(false)
  const featureFlags = useFeatureFlag()
  const [insightIdWithOpenControl, setInsightIdWithOpenAction] = useState<
    string | undefined
  >(undefined)

  const [selectedAction, setSelectedAction] = useState<ModalType>('insight')
  const {
    isInsightTypeNode,
    pageSize = 10,
    page = 1,
    totalInsights = 0,
    t,
    language,
    impactView,
    isLoading,
    insights = [],
    siteId,
    selectedInsight,
    selectedInsightIds = [],
    isUngrouped,
    isWalmartAlert,
    sites,
    onSelectInsightIds,
    onSortModelChange,
    onSelectInsight,
    onResetInsight,
    onQueryParamsChange,
    handleInsightClick,
    ontologyQuery: { data: ontology },
    modelsOfInterestQuery: { data: { items: modelsOfInterest } = {} },
  } = useInsightsContext()

  const handleSelectAction = (action: string, rowContent: Insight) => {
    const actionMap = {
      [InsightActions.newTicket]: 'newTicket',
      [InsightActions.delete]: 'deleteInsightsConfirmation',
      [InsightActions.resolve]: 'resolveInsightConfirmation',
      [InsightActions.report]: 'report',
    }
    setSelectedAction(_.get(actionMap, action))
    onSelectInsight(rowContent)
    onQueryParamsChange?.({ insightId: rowContent.id })
  }

  const handleActionView = (event, row) => {
    event?.stopPropagation()
    setIsActionsViewOpen(!isActionsViewOpen)
    setInsightIdWithOpenAction(!isActionsViewOpen ? row.id : undefined)
  }

  // If current scope is used as building, it means there can possibly be 1 location,
  // otherwise, there can be multiple locations
  const isSingleLocation = isScopeUsedAsBuilding(location)

  const columns: GridColDef[] = useMemo(
    () => [
      {
        field: 'lastStatus',
        headerName: t('labels.status'),
        renderCell: ({ value }) => (
          <InsightWorkflowStatusPill size="md" lastStatus={value} />
        ),
        minWidth: 140,
        flex: 1,
        disableSortBy: true,
      },
      // Displaying the insight name column only for "ungrouped" insights and "walmart alert" insights
      ...(isUngrouped || isWalmartAlert
        ? [
            {
              field: 'name',
              headerName: t('headers.insight'),
              valueGetter: ({ row, value }) => row.original?.ruleName || value,
              minWidth: 150,
              flex: 1,
              disableSortBy: true,
            },
          ]
        : []),

      // Hiding the rule name and category column for Insight Type Node Page
      // and type casting it to any to avoid type error
      ...(!isInsightTypeNode
        ? [
            {
              field: 'name',
              headerName: titleCase({
                text: t('plainText.skill'),
                language,
              }),
              // business requirement to display ruleName if it's defined and not an empty string,
              // display insight name (also called summary) otherwise
              // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/78451
              valueGetter: ({ row, value }) => row?.ruleName || value,
              minWidth: isSingleLocation ? 400 : 300,
              flex: 1,
              disableSortBy: true,
            },
            {
              field: 'type',
              headerName: t('labels.category'),
              renderCell: ({ value }) => (
                <InsightTypeBadge type={value} badgeSize="md" />
              ),
              type: 'singleSelect',
              minWidth: 150,
              flex: 1,
              disableSortBy: true,
            },
          ]
        : ([] as any)),
      {
        field: 'twinId',
        headerName: t('plainText.twin'),
        renderCell: ({ row }) =>
          row.twinId &&
          row.primaryModelId &&
          ontology &&
          modelsOfInterest && (
            <TwinLink
              tw="max-w-full"
              twin={{
                id: row.twinId,
                siteId: row.siteId,
              }}
              onClick={(e) => e.stopPropagation()}
            >
              <TwinChip
                variant="instance"
                modelOfInterest={getModelOfInterest(
                  row.primaryModelId,
                  ontology,
                  modelsOfInterest
                )}
                text={row.equipmentName}
                highlightOnHover
                tw="w-full h-full"
              />
            </TwinLink>
          ),
        minWidth: 192,
        flex: 1,
        disableSortBy: true,
      },
      // If the scope is just 1 building, this column is redundant
      ...(!isSingleLocation
        ? [
            {
              field: 'siteId',
              headerName: titleCase({
                text: t('plainText.location'),
                language,
              }),
              renderCell: ({ value }) => {
                const siteName = sites?.find((s) => s.id === value)?.name
                return <SiteChip siteName={siteName} />
              },
              minWidth: 160,
              flex: 1,
              disableSortBy: true,
            },
          ]
        : []),
      {
        field: 'activityCount',
        headerName: _.capitalize(t('plainText.activity')),
        sortable: false,
        renderCell: ({ row }) =>
          // Showing report icons for all tabs and other icons in active tabs only
          // Reference - https://dev.azure.com/willowdev/Unified/_workitems/edit/84674
          // https://dev.azure.com/willowdev/Unified/_workitems/edit/87044
          activityIcons.map(
            ({
              key,
              icon,
              tooltipText,
              itemHistory,
              filled,
              isIcon = false,
            }) => (
              <ActivityCount
                key={`${key}-${row.id}`}
                activityCount={row[key]}
                icon={icon}
                tooltipText={
                  itemHistory
                    ? t(tooltipText, { itemHistory: t(itemHistory) })
                    : t(tooltipText)
                }
                filled={filled}
                isVisible
                isIcon={isIcon}
              />
            )
          ),
        minWidth: isSingleLocation ? 120 : 100,
        flex: 1,
        disableSortBy: true,
      },
      {
        field: 'impactScores',
        headerName: titleCase({
          text: t('interpolation.avoidableExpensePerYear', {
            expense: t(`plainText.${impactView}`),
          }),
          language,
        }),
        sortable: false,
        align: 'right',
        renderCell: ({ row }) => (
          <span tw="pr-[20px]">
            {getImpactScore({
              impactScores: row.impactScores,
              scoreName:
                impactView === InsightMetric.cost
                  ? InsightCostImpactPropNames.dailyAvoidableCost
                  : InsightCostImpactPropNames.dailyAvoidableEnergy,
              multiplier: 365,
              language,
              decimalPlaces: 0,
            })}
          </span>
        ),
        minWidth: 180,
        flex: 1,
        disableSortBy: true,
      },
      {
        field: 'priority',
        headerName: t('labels.priority'),
        renderCell: ({ value }) => <PriorityName insightPriority={value} />,
        minWidth: 100,
        flex: 0.5,
        disableSortBy: true,
      },
      {
        field: 'occurredDate',
        headerName: titleCase({
          text: t('plainText.lastOccurrence'),
          language,
        }),
        valueGetter: ({ row, value }) =>
          formatDateTime({
            value,
            language,
            timeZone: row.timeZone,
          }),
        flex: 0.5,
        minWidth: 152,
        disableSortBy: true,
        sortComparator: dateComparator,
      },
      {
        field: 'id',
        headerName: '',
        renderCell: ({ row }) => {
          const isReadyToResolve =
            featureFlags.hasFeatureToggle('readyToResolve') &&
            row.lastStatus === InsightActions.readyToResolve

          return (
            // line-height: 0px; helps to position actions view control icon in the center
            // vertically and horizontally
            <FullSizeContainer css="line-height: 0px;">
              <ActionsViewControl
                position="bottom-end"
                withinPortal
                selectedInsight={row}
                lastStatus={row.lastStatus}
                floorId={row.floorId}
                siteId={row.siteId}
                assetId={row.asset?.id}
                onCreateTicketClick={() =>
                  handleSelectAction(InsightActions.newTicket, row)
                }
                onDeleteClick={() =>
                  handleSelectAction(InsightActions.delete, row)
                }
                onResolveClick={() =>
                  handleSelectAction(InsightActions.resolve, row)
                }
                onReportClick={() =>
                  handleSelectAction(InsightActions.report, row)
                }
                canDeleteInsight={siteId !== ALL_SITES}
                opened={
                  isActionsViewOpen && insightIdWithOpenControl === row.id
                }
                onToggleActionsView={(isOpen) => {
                  setIsActionsViewOpen(isOpen)
                  setInsightIdWithOpenAction(isOpen ? row.id : undefined)
                }}
              >
                <Indicator
                  intent="primary"
                  size={6}
                  hasBorder={isReadyToResolve}
                  color={isReadyToResolve ? 'violet' : 'transparent'}
                >
                  <Icon
                    tw="cursor-pointer w-[100%]"
                    data-testid={`actionViewControl-${row.id}`}
                    icon="more_vert"
                  />
                </Indicator>
              </ActionsViewControl>
            </FullSizeContainer>
          )
        },
        disableSortBy: true,
        width: 44,
      },
    ],
    [
      t,
      isUngrouped,
      isInsightTypeNode,
      language,
      ontology,
      modelsOfInterest,
      sites,
      impactView,
      featureFlags,
      siteId,
      isActionsViewOpen,
      insightIdWithOpenControl,
      handleSelectAction,
    ]
  )

  /**
   * resets states and close the current opened modal;
   * however, if user is on resolve confirmation modal and open a ticket modal,
   * closing ticket modal will open the resolve confirmation modal because marking
   * ticket status as "closed" is prerequisite to resolve an insight.
   * reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/80180
   */
  const handleModalClose = () => {
    if (selectedAction === 'ticket') {
      setSelectedAction('resolveInsightConfirmation')
    } else {
      onResetInsight?.()
      setSelectedAction('insight')
    }
    // Removing insightId query param when user closes the modal
    onQueryParamsChange?.({ insightId: undefined })
  }

  // render an external spinner to avoid DataGrid native spinner not positioned correctly
  if (isLoading) {
    return <FullSizeLoader />
  }

  return (
    <>
      <StyledDataGrid
        data-segment="All Insights Selected"
        data-testid="all-insights-results"
        rows={insights}
        columns={columns}
        rowSelectionModel={selectedInsightIds}
        onRowSelectionModelChange={onSelectInsightIds}
        onRowClick={({ row }) => handleInsightClick(row)}
        checkboxSelection
        slots={{
          pagination: CustomPagination,
          noRowsOverlay: () => (
            <NotFound
              message={titleCase({
                language,
                text: t('plainText.noInsightsFound'),
              })}
            />
          ),
        }}
        initialState={{
          pinnedColumns: {
            right: ['id'],
            left: [GRID_CHECKBOX_SELECTION_COL_DEF.field],
          },
          sorting: {
            sortModel: [{ field: 'occurredDate', sort: 'desc' }],
          },
          pagination: {
            paginationModel: {
              pageSize: Number(pageSize),
              page: Number(page),
            },
          },
        }}
        filterMode="server"
        pagination
        paginationMode="server"
        sortingMode="server"
        onSortModelChange={onSortModelChange}
        rowCount={totalInsights}
        disableColumnResize={false} // Disabling default behavior of column resizing
        // to avoid issue like:
        // https://github.com/mui/mui-x/issues/2714
        isRowSelectable={(data) => data.row != null && data.row.id != null}
        onCellClick={({ field, row }, event) => {
          if (field === 'id') {
            handleActionView(event, row)
          }
        }}
      />
      {selectedInsight != null && (
        <AssetDetailsModal
          siteId={selectedInsight.siteId}
          item={
            selectedAction === 'newTicket'
              ? {
                  ...selectedInsight,
                  insightId: selectedInsight.id,
                  insightName: selectedInsight.sequenceNumber,
                  insightStatus: selectedInsight.status,
                  siteId: selectedInsight.siteId,
                  modalType: selectedAction,
                }
              : {
                  ...selectedInsight,
                  modalType: selectedAction,
                }
          }
          onClose={handleModalClose}
          navigationButtonProps={{
            items: insights,
            selectedItem: selectedInsight,
            setSelectedItem: onSelectInsight,
          }}
          dataSegmentPropPage="Insights Details"
          canDeleteInsight={siteId !== ALL_SITES}
          onActionChange={setSelectedAction as (action?: string) => void}
          selectedInsightIds={[selectedInsight.id]}
          onClearSelectedInsightIds={onResetInsight}
        />
      )}
    </>
  )
}

const StyledDataGrid = styled(DataGrid)(({ theme }) => ({
  '&&&': {
    border: '0px',
  },
  // Overriding the existing styling to show the resize icon in the table header
  '.MuiDataGrid-columnSeparator': {
    display: 'block',
    color: theme.color.neutral.fg.default,

    '&:hover': {
      color: theme.color.neutral.fg.highlight,
    },

    '> svg': {
      height: '56px',
      display: 'block',
    },
  },

  // Changing the position of pagination to left
  '.MuiDataGrid-footerContainer': {
    justifyContent: 'flex-start',
    paddingLeft: theme.spacing.s8,
  },
  // Last pinned column is for "Action View Control" icon which is not relevant for the header row
  '& .MuiDataGrid-pinnedColumnHeaders:last-child': {
    display: 'none',
  },
}))

const activityIcons: Array<{
  icon: IconName
  tooltipText: string
  itemHistory?: string
  key: string
  filled: boolean
  isIcon?: boolean
}> = [
  {
    icon: 'assignment',
    tooltipText: 'plainText.relatedTicket',
    key: 'ticketCount',
    filled: true,
  },
  {
    icon: 'feedback',
    tooltipText: 'plainText.reported',
    key: 'reported',
    filled: true,
    isIcon: true,
  },
  {
    icon: 'check_circle',
    tooltipText: 'interpolation.previouslyItemHistory',
    itemHistory: 'headers.resolved',
    key: 'previouslyResolved',
    filled: false,
  },
  {
    icon: 'do_not_disturb_on',
    tooltipText: 'interpolation.previouslyItemHistory',
    itemHistory: 'headers.ignored',
    key: 'previouslyIgnored',
    filled: false,
  },
]
