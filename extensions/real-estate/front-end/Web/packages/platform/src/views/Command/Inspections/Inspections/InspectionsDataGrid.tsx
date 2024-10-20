import {
  FloorPill,
  MoreButtonDropdown,
  MoreButtonDropdownOption,
  NotFound,
  ProgressTotal,
  useDateTime,
  useScopeSelector,
} from '@willow/ui'
import {
  Badge,
  DataGrid,
  GridColDef,
  GridRowId,
  Icon,
  IconName,
  useGridApiRef,
} from '@willowinc/ui'
import _ from 'lodash'
import { useCallback, useMemo, useRef } from 'react'
import { useTranslation } from 'react-i18next'
import { useHistory } from 'react-router'
import { css } from 'styled-components'
import {
  Check,
  CheckRecordStatus,
  Inspection,
} from '../../../../services/Inspections/InspectionsServices'
import getWorkableStatusPillColor from '../getWorkableStatusPillColor'
import handleCheckRowClick from '../handleCheckRowClick'
import CheckEntry from './CheckEntry'

const InspectionsDataGrid = ({
  isCompletedTab,
  inspections,
  onSelect,
  onArchive,
  showSiteColumn = false,
}: {
  isCompletedTab: boolean
  inspections: Inspection[]
  onSelect: (inspection: Inspection) => void
  onArchive: (inspection: Inspection) => void
  showSiteColumn?: boolean
}) => {
  // We save the expansion state of the group row in a ref to persist it when the grid is re-rendered
  const expansionLookup = useRef<Record<GridRowId, boolean>>({})
  const { isScopeSelectorEnabled, scopeId } = useScopeSelector()
  const history = useHistory()
  const apiRef = useGridApiRef()
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const dateTime = useDateTime()

  // We flatten the inspections array to include checks as separate rows
  // and add a hierarchy property to each row to enable grouping
  const rows = useMemo(() => {
    const inspectionsWithChecks: Array<
      (Inspection | Check) & {
        hierarchy: string[]
        nextCheckRecordDueTimeText?: string
      }
    > = []

    for (const inspection of _.uniqBy(inspections, 'id')) {
      const { checks } = inspection
      if (checks && checks.length > 0) {
        for (const check of checks) {
          // We overwrite "assignedWorkgroupName" so that we can benefit from
          // DataGrid's default text-overflow behavior and tooltip handling
          const statusToTimeProps: {
            [key in CheckRecordStatus]?: { value?: string; format: string }
          } = {
            overdue: {
              value: check.statistics.lastCheckSubmittedDate,
              format: 'by',
            },
            due: {
              value: check.statistics.nextCheckRecordDueTime,
              format: 'in',
            },
            completed: {
              value: check.statistics.lastCheckSubmittedDate,
              format: 'at',
            },
          }
          const { value, format } =
            statusToTimeProps[check.statistics.workableCheckStatus] || {}

          inspectionsWithChecks.push({
            ...check,
            siteId: inspection.siteId,
            hierarchy: [inspection.id, check.id],
            assignedWorkgroupName: `${t('plainText.enteredBy')}: ${
              check.statistics.lastCheckSubmittedUserName ?? '-'
            }`,
            nextCheckRecordDueTimeText: value
              ? dateTime(value).format(format, undefined, language)
              : null,
          })
        }
      }
      inspectionsWithChecks.push({
        ...inspection,
        hierarchy: [inspection.id],
        nextCheckRecordDueTimeText: dateTime(
          inspection.nextCheckRecordDueTime
        ).format('by', undefined, language),
      })
    }
    return inspectionsWithChecks
  }, [dateTime, inspections, language, t])

  const handleRowClick = useCallback(
    (props) => {
      const {
        row: { inspectionId, id: checkId, siteId },
      } = props
      handleCheckRowClick({
        id: props.id,
        apiRef,
        expansionLookup,
        isScopeSelectorEnabled,
        history,
        scopeId,
        siteId,
        inspectionId,
        checkId,
        showSiteColumn,
      })
    },
    [
      apiRef,
      expansionLookup,
      history,
      isScopeSelectorEnabled,
      scopeId,
      showSiteColumn,
    ]
  )

  const showAdminColumn = inspections.some(
    (inspection) => inspection.isSiteAdmin
  )

  const columns = useMemo<GridColDef[]>(
    () => [
      ...(showSiteColumn
        ? [
            {
              field: 'siteName',
              headerName: t('labels.site'),
              minWidth: 185,
              flex: 1,
            },
          ]
        : []),
      {
        field: 'floorCode',
        headerName: t('labels.floor'),
        width: 70,
        renderCell: ({
          row: { floorCode, inspectionId },
        }: {
          row: { floorCode: string; inspectionId?: string }
        }) => !inspectionId && <FloorPill>{floorCode}</FloorPill>,
      },
      {
        field: 'zoneName',
        headerName: t('plainText.zone'),
        minWidth: 100,
        flex: 1,
        valueGetter: ({ row }) => (!row?.inspectionId ? row?.zoneName : ''),
        cellClassName: 'data-grid-cell data-grid-cell-zoneName',
      },
      {
        field: 'assetName',
        headerName: t('plainText.asset'),
        minWidth: 100,
        flex: 1,
        valueGetter: ({ row }) => (!row?.inspectionId ? row?.assetName : ''),
        cellClassName: 'data-grid-cell data-grid-cell-assetName',
      },
      {
        field: 'name',
        headerName: t('plainText.inspection'),
        minWidth: 200,
        flex: 1,
      },
      {
        field: 'status',
        headerName: t('labels.status'),
        width: 120,
        renderCell: <T,>({ row }: { row: InspectionOrCheck<T> }) => {
          const isCheck = 'inspectionId' in row
          const color = getWorkableStatusPillColor(
            isCheck ? row.statistics.workableCheckStatus : row.status
          )
          const key = _.camelCase(
            isCheck ? row.statistics.workableCheckStatus : row.status
          )

          return (
            <Badge variant="outline" size="md" color={color}>
              {t('interpolation.plainText', { key })}
            </Badge>
          )
        },
      },
      {
        field: 'completedCheckCount',
        headerName: t('plainText.progress'),
        width: 180,
        renderCell: <T,>({ row }: { row: InspectionOrCheck<T> }) =>
          'inspectionId' in row ? (
            <CheckEntry
              check={row}
              entry={formatCheck(row)}
              type={row.type}
              typeValue={row.typeValue}
            />
          ) : (
            <ProgressTotal
              value={row.completedCheckCount}
              total={row.workableCheckCount}
            />
          ),
      },
      {
        field: 'nextCheckRecordDueTime',
        headerName: t('plainText.due'),
        width: 160,
        flex: 1,
        // We use combination of valueGetter, valueFormatter and sortComparator to
        // sort by the actual date value while displaying a formatted date
        // e.g. display: "by Apr 29, 2023, 02:00", sort by: "2023-04-29T02:00:00.000Z"
        // and keep DataGrid's default text-overflow behavior and tooltip handling
        valueGetter: ({ row }) => ({
          displayValue: row.nextCheckRecordDueTimeText,
          sortValue: row.nextCheckRecordDueTime,
        }),
        valueFormatter: (data) => data?.value?.displayValue || '',
        sortComparator: (v1, v2) =>
          new Date(v1.sortValue).getTime() - new Date(v2.sortValue).getTime(),
      },
      {
        field: 'assignedWorkgroupName',
        headerName: isCompletedTab
          ? t('plainText.enteredBy')
          : t('plainText.responsibleBy'),
        flex: 1,
      },
      ...(showAdminColumn
        ? [
            {
              field: 'admin',
              headerName: '',
              width: 40,
              sortable: false,
              renderCell: <T,>({ row }: { row: InspectionOrCheck<T> }) =>
                !('inspectionId' in row) && row.isSiteAdmin ? (
                  <MoreButtonDropdown
                    targetButtonProps={{
                      background: 'transparent',
                    }}
                  >
                    {[
                      [
                        'arrow_forward',
                        onSelect,
                        'plainText.inspectionSettings',
                      ],
                      ['delete', onArchive, 'headers.archiveInspection'],
                    ].map(
                      ([icon, onClick, text]: [
                        IconName,
                        (row: Inspection) => void,
                        string
                      ]) => (
                        <MoreButtonDropdownOption
                          key={icon}
                          onClick={(e) => {
                            e.preventDefault()
                            e.stopPropagation()
                            onClick(row)
                          }}
                          prefix={<Icon icon={icon} />}
                        >
                          {t(text)}
                        </MoreButtonDropdownOption>
                      )
                    )}
                  </MoreButtonDropdown>
                ) : null,
            },
          ]
        : []),
    ],
    [showSiteColumn, t, isCompletedTab, showAdminColumn, onSelect, onArchive]
  )

  return (
    <DataGrid
      apiRef={apiRef}
      columns={columns}
      rows={rows}
      treeData
      getTreeDataPath={(row) => row.hierarchy}
      groupingColDef={{
        headerName: '',
        hideDescendantCount: true,
        width: 50,
        resizable: false,
      }}
      isGroupExpandedByDefault={(node) =>
        // We use either Inspection.id or Check.id as the grouping key
        // so groupingKey is guaranteed to be a string
        !!expansionLookup.current[node?.groupingKey as string]
      }
      onRowClick={handleRowClick}
      slots={{
        noRowsOverlay: () => (
          <NotFound>{t('plainText.noInspectionsFound')}</NotFound>
        ),
      }}
      css={css(
        ({ theme }) =>
          `
      border: none;
      & .data-grid-cell {
        align-items: center;
        border-radius: ${theme.radius.r2};
        display: inline-flex;
        font-size: ${theme.font.body.xs.regular.fontSize};
        white-space: nowrap;
        & .MuiDataGrid-cellContent {
          padding: 0 ${theme.spacing.s8};
        }
      }
      & .data-grid-cell-zoneName {
        & .MuiDataGrid-cellContent {
          padding: 0 ${theme.spacing.s8};
          background-color: ${theme.color.neutral.border.default}
        }
      }
      & .data-grid-cell-assetName { 
        color: ${theme.color.intent.secondary.fg.default};
        text-transform: uppercase;
        & .MuiDataGrid-cellContent {
          padding: 0 ${theme.spacing.s8};
          border: 1px solid ${theme.color.neutral.border.default};
        }
      }
    `
      )}
    />
  )
}

const formatCheck = (check: Check) => {
  const { lastCheckSubmittedEntry } = check.statistics

  if (!lastCheckSubmittedEntry) {
    return null
  }

  if (
    check.type === 'List' ||
    Number.isNaN(parseFloat(lastCheckSubmittedEntry))
  ) {
    return lastCheckSubmittedEntry
  }

  return parseFloat(lastCheckSubmittedEntry).toFixed(check.decimalPlaces ?? 0)
}

export default InspectionsDataGrid

// Locally defined type to determine if the object is an Inspection or a Check
// based on the presence of the 'inspectionId' property.
type InspectionOrCheck<T> = T extends { inspectionId: string }
  ? Check
  : Inspection
