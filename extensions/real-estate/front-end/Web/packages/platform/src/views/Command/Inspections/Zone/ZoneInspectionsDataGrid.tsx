import { FullSizeLoader } from '@willow/common'
import {
  FloorPill,
  MoreButtonDropdown,
  MoreButtonDropdownOption,
  NotFound,
  useDateTime,
  useScopeSelector,
} from '@willow/ui'
import {
  Badge,
  Box,
  DataGrid,
  GridColDef,
  GridRowId,
  Group,
  Icon,
  IconButton,
  IconName,
  Stack,
  useGridApiRef,
  useTheme,
} from '@willowinc/ui'
import _ from 'lodash'
import { ReactNode, useCallback, useMemo, useRef } from 'react'
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
import CheckComponent from './Check'

const InspectionsDataGrid = ({
  inspections,
  onSelect,
  onArchive,
  onSortOrderChange,
  isLoading,
}: {
  inspections: Inspection[]
  onSelect: (inspection: Inspection) => void
  onArchive: (inspection: Inspection) => void
  onSortOrderChange: ({
    siteId,
    zoneId,
    inspectionIds,
  }: {
    siteId: string
    zoneId: string
    inspectionIds: string[]
  }) => void
  isLoading: boolean
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
  const theme = useTheme()

  // We flatten the inspections array to include checks as separate rows
  // and add a hierarchy property to each row to enable grouping
  const rows = useMemo(() => {
    const inspectionsWithChecks: Array<(Inspection | Check) & SharedProps> = []

    for (const inspection of _.uniqBy(inspections, 'id')) {
      const { checks } = inspection
      if (checks && checks.length > 0) {
        for (const check of checks) {
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

          // We add a header row for each group of checks
          if (checks.indexOf(check) === 0) {
            inspectionsWithChecks.push({
              ...check,
              id: `${check.id}-header`,
              name: t('plainText.checkName'),
              isChildHeader: true,
              siteId: inspection.siteId,
              hierarchy: [inspection.id, `${check.id}-header`],
              assignedWorkgroupName: t('plainText.assignedGroup'),
            })
          }

          inspectionsWithChecks.push({
            ...check,
            isChildRow: true,
            childIndex: checks.indexOf(check),
            siteId: inspection.siteId,
            hierarchy: [inspection.id, check.id],
            assignedWorkgroupName: inspection.assignedWorkgroupName,
            nextCheckRecordDueTimeText: value
              ? dateTime(value).format(format, undefined, language)
              : null,
          })
        }
      }
      inspectionsWithChecks.push({
        ...inspection,
        assignedWorkgroupName: undefined,
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
        row: { inspectionId, id: checkId, siteId, isChildHeader },
      } = props
      if (isChildHeader) {
        return // Do nothing when clicking on the check header row
      }
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
      })
    },
    [apiRef, expansionLookup, history, isScopeSelectorEnabled, scopeId]
  )

  const columns = useMemo<GridColDef[]>(
    () => [
      {
        field: 'floorCode',
        headerName: t('labels.floor'),
        width: 70,
        sortable: false,
        renderCell: ({
          row: { floorCode, inspectionId },
        }: {
          row: { floorCode: string; inspectionId?: string }
        }) => !inspectionId && <FloorPill>{floorCode}</FloorPill>,
      },
      {
        field: 'name',
        headerName: t('plainText.inspection'),
        minWidth: 180,
        maxWidth: 250,
        flex: 1,
        sortable: false,
        renderCell: ({ row }) => {
          if (!row.isChildHeader && !row.isChildRow) {
            return (
              <Stack
                display="block"
                gap={0}
                className="MuiDataGrid-cellContent"
                role="presentation"
                title={row.name}
              >
                <span
                  css={{
                    padding: `${theme.spacing.s4} ${theme.spacing.s8}`,
                    backgroundColor: theme.color.neutral.border.default,
                    fontSize: theme.font.body.xs.regular.fontSize,
                  }}
                >
                  {row.name}
                </span>
              </Stack>
            )
          }

          return (
            <TwoLineCellContent
              first={row.isChildHeader ? t('plainText.checkName') : row.name}
              second={
                row.isChildHeader
                  ? t('plainText.checkNo')
                  : `No.${row.childIndex + 1}`
              }
              title={
                row.isChildHeader
                  ? t('plainText.checkName')
                  : `${row.name} No.${row.childIndex + 1}`
              }
            />
          )
        },
      },
      {
        field: 'completedCheckCount',
        headerName: t('plainText.checks'),
        minWidth: 100,
        sortable: false,
        renderCell: ({ row }: { row: (Inspection | Check) & SharedProps }) =>
          isCheck(row) ? null : (
            <Box
              component="span"
              c="intent.secondary.fg.hovered"
              px="s8"
              bg="neutral.border.default"
              css={{
                height: '100%',
                display: 'flex',
                flexDirection: 'column',
                justifyContent: 'center',
                alignContent: 'center',
                fontSize: theme.font.body.xs.regular.fontSize,
              }}
            >
              <Group gap="s2">
                <Icon size={16} icon="task_alt" />
                {row.workableCheckCount}
              </Group>
            </Box>
          ),
      },
      {
        field: 'assetName',
        headerName: t('plainText.asset'),
        minWidth: 100,
        flex: 1,
        maxWidth: 250,
        sortable: false,
        renderCell: ({ row }) => {
          const assetName = row?.inspectionId ? '' : row?.assetName
          return (
            <span
              className="MuiDataGrid-cellContent"
              role="presentation"
              title={assetName}
              css={{
                color: theme.color.intent.secondary.fg.default,
                textTransform: 'uppercase',
                padding: `0 ${theme.spacing.s8}`,
                border:
                  assetName &&
                  `1px solid ${theme.color.neutral.border.default}`,
                fontSize: theme.font.body.xs.regular.fontSize,
              }}
            >
              {assetName}
            </span>
          )
        },
      },
      {
        field: 'status',
        headerName: '',
        minWidth: 160,
        flex: 1,
        sortable: false,
        renderCell: ({ row }: { row: (Inspection | Check) & SharedProps }) => {
          if (row.isChildHeader) {
            return (
              <TwoLineCellContent
                first={t('labels.status')}
                second={t('plainText.nextDue')}
                title={t('labels.status')}
              />
            )
          }
          if (!isCheck(row)) {
            return undefined
          }
          const color = getWorkableStatusPillColor(
            row.statistics.workableCheckStatus
          )
          const key = _.camelCase(row.statistics.workableCheckStatus)

          return (
            <TwoLineCellContent
              first={
                <Badge w="100%" variant="outline" size="md" color={color}>
                  {t('interpolation.plainText', { key })}
                </Badge>
              }
              second={row.nextCheckRecordDueTimeText}
              title={t('interpolation.plainText', { key })}
            />
          )
        },
      },
      {
        field: 'nextCheckRecordDueTime',
        headerName: '',
        minWidth: 160,
        flex: 1,
        sortable: false,
        renderCell: ({ row }: { row: (Inspection | Check) & SharedProps }) => {
          if (!isCheck(row)) {
            return ''
          }

          if (row.isChildHeader) {
            return (
              <TwoLineCellContent
                first={t('plainText.latestEntry')}
                second={t('labels.updated')}
                title={t('plainText.latestEntry')}
              />
            )
          }

          return (
            <TwoLineCellContent
              first={<CheckComponent check={row} />}
              second={dateTime(row.statistics.lastCheckSubmittedDate).format(
                'at',
                undefined,
                language
              )}
            />
          )
        },
      },
      {
        field: 'total',
        headerName: '',
        minWidth: 160,
        flex: 1,
        sortable: false,
        renderCell: ({ row }: { row: (Inspection | Check) & SharedProps }) => {
          if (!isCheck(row)) {
            return ''
          }

          if (row.isChildHeader) {
            return (
              <TwoLineCellContent
                first={t('plainText.totalRecords')}
                title={t('plainText.totalRecords')}
              />
            )
          }

          return row.statistics.checkRecordCount
        },
      },
      {
        field: 'assignedWorkgroupName',
        headerName: '',
        minWidth: 160,
        flex: 1,
        sortable: false,
        renderCell: ({ row }: { row: (Inspection | Check) & SharedProps }) => {
          if (!isCheck(row)) {
            return ''
          }

          if (row.isChildHeader) {
            return (
              <TwoLineCellContent
                first={t('plainText.assignedGroup')}
                second={t('labels.status')}
                title={t('plainText.assignedGroup')}
              />
            )
          }

          return (
            <TwoLineCellContent
              first={row.assignedWorkgroupName}
              second={
                row.isPaused ? t('plainText.paused') : t('plainText.running')
              }
              title={row.assignedWorkgroupName}
            />
          )
        },
      },
      {
        field: 'sortOrder',
        headerName: '',
        width: 100,
        sortable: false,
        renderCell: ({ row }: { row: (Inspection | Check) & SharedProps }) => {
          if (isCheck(row)) {
            return ''
          } else {
            return (
              <>
                {['arrow_upward' as const, 'arrow_downward' as const].map(
                  (icon) => {
                    const isSortingUp = icon === 'arrow_upward'
                    return (
                      <IconButton
                        display="block"
                        className="MuiDataGrid-cellContent"
                        role="presentation"
                        title={t(
                          isSortingUp
                            ? 'plainText.increaseSortOrder'
                            : 'plainText.decreaseSortOrder'
                        )}
                        key={icon}
                        kind="secondary"
                        icon={icon}
                        background="transparent"
                        disabled={
                          isSortingUp
                            ? row.id === inspections[0].id
                            : row.id === inspections.at(-1)?.id
                        }
                        onClick={(e) => {
                          const inspectionIds = inspections.map((i) => i.id)
                          const currentIndex = inspectionIds.indexOf(row.id)

                          e.stopPropagation()
                          onSortOrderChange({
                            siteId: row.siteId,
                            zoneId: row.zoneId,
                            inspectionIds: [
                              ...inspectionIds.slice(
                                0,
                                isSortingUp ? currentIndex - 1 : currentIndex
                              ),
                              inspectionIds[
                                isSortingUp ? currentIndex : currentIndex + 1
                              ],
                              inspectionIds[
                                isSortingUp ? currentIndex - 1 : currentIndex
                              ],
                              ...inspectionIds.slice(
                                isSortingUp
                                  ? currentIndex + 1
                                  : currentIndex + 2
                              ),
                            ],
                          })
                        }}
                      />
                    )
                  }
                )}
              </>
            )
          }
        },
      },
      {
        field: 'admin',
        headerName: '',
        width: 40,
        sortable: false,
        renderCell: ({ row }: { row: (Inspection | Check) & SharedProps }) =>
          !isCheck(row) ? (
            <MoreButtonDropdown
              targetButtonProps={{
                background: 'transparent',
              }}
            >
              {[
                ['arrow_forward', onSelect, 'plainText.inspectionSettings'],
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
    ],
    [t, dateTime, language, inspections, onSortOrderChange, onSelect, onArchive]
  )

  return (
    <DataGrid
      apiRef={apiRef}
      columns={columns}
      rows={rows}
      treeData
      getTreeDataPath={(row) => row.hierarchy}
      disableRowSelectionOnClick
      loading={isLoading}
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
        loadingOverlay: () => <FullSizeLoader />,
      }}
      css={css`
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
      `}
    />
  )
}

export default InspectionsDataGrid

const isCheck = (
  row: (Inspection | Check) & SharedProps
): row is Check & SharedProps => 'inspectionId' in row

type SharedProps = {
  hierarchy: string[]
  nextCheckRecordDueTimeText?: string
  isChildHeader?: boolean
  isChildRow?: boolean
  childIndex?: number
  assignedWorkgroupName?: string
}

const TwoLineCellContent = ({
  first,
  second,
  title,
}: {
  first: ReactNode
  second?: string
  title?: string
}) => {
  const theme = useTheme()
  return (
    <Stack
      display="block"
      gap={0}
      className="MuiDataGrid-cellContent"
      role="presentation"
      title={title}
    >
      <span>{first}</span>
      <br />
      {second && (
        <span
          css={{
            ...theme.font.body.xs.regular,
            color: theme.color.intent.secondary.fg.hovered,
          }}
        >
          {second}
        </span>
      )}
    </Stack>
  )
}
