import { UseQueryResult } from 'react-query'
import { styled, css } from 'twin.macro'
import { useCallback, useEffect, useRef } from 'react'
import { useTranslation } from 'react-i18next'
import { titleCase } from '@willow/common'
import { TwinChip, TooltipWhenTruncated } from '@willow/ui'
import {
  PanelGroup,
  Panel,
  Icon,
  PanelContent,
  Loader,
  Select,
  DataGrid,
  GridColDef,
  Badge,
  Button,
  useGridApiContext,
  GridRenderCellParams,
  ButtonProps,
  IconButton,
  useGridApiRef,
  GridRowId,
  Group,
  Stack,
  IconName,
} from '@willowinc/ui'
import {
  Insight,
  DiagnosticOccurrence,
  PointTwinDto,
} from '@willow/common/insights/insights/types'
import { getModelInfo } from '@willow/common/twins/utils'
import FullSizeLoader from '@willow/common/components/FullSizeLoader'
import _ from 'lodash'
import { ErrorMessage } from './shared/index'
import styles from './LeftPanel.css'
import { useSelectedPoints } from '../../MiniTimeSeries/index'
import { TwinLink } from '../../../views/Portfolio/twins/shared/index'
import NotFound from '../ui/NotFound'

const Diagnostics = ({
  insightDiagnosticsQuery,
  insight,
  diagnosticPeriods,
  onDiagnosticPeriodChange,
  selectedPeriod,
  modelInfo,
  status,
  onRowClick,
  rows,
  pointsSelected,
  pointsNotSelected,
  isAllPointsSelected = false,
}: {
  insightDiagnosticsQuery: UseQueryResult<DiagnosticOccurrence[]>
  insight: Insight
  diagnosticPeriods: Array<{
    start: string
    end: string
    value: string
    label: string
  }>
  onDiagnosticPeriodChange: (period: string | null) => void
  selectedPeriod?: string
  modelInfo?: ReturnType<typeof getModelInfo>
  status: string
  onRowClick: (params) => void
  rows: Array<DiagnosticOccurrence & PointTwinDto>
  pointsSelected: string[]
  pointsNotSelected: string[]
  isAllPointsSelected?: boolean
}) => {
  const selectedPointsContext = useSelectedPoints()
  const translation = useTranslation()

  // DataGrid will lose its expansion state when context or props change happens,
  // we therefore preserve the expansion state in a ref
  const expansionLookup = useRef<Record<GridRowId, boolean>>({})
  const apiRef = useGridApiRef()
  useEffect(() => {
    apiRef?.current?.subscribeEvent?.('rowExpansionChange', (node) => {
      expansionLookup.current[node.id] = node.childrenExpanded || true
    })
  }, [apiRef, apiRef?.current?.subscribeEvent])
  const isGroupExpandedByDefault = useCallback(
    (node) => !!expansionLookup.current[node.id],
    [expansionLookup]
  )

  const {
    t,
    i18n: { language },
  } = translation
  const columns: GridColDef[] = [
    {
      field: 'ruleName',
      headerName: titleCase({ text: t('plainText.skill'), language }),
      flex: 3,
      minWidth: 220,
      renderCell: ({ row: { ruleName, twinName, occurrenceLiveData } }) => {
        const label = ruleName || occurrenceLiveData?.pointName || twinName

        return (
          <TooltipWhenTruncated label={label}>
            <span
              css={`
                width: 100%;
                white-space: nowrap;
                overflow: hidden;
                text-overflow: ellipsis;
              `}
            >
              {label}
            </span>
          </TooltipWhenTruncated>
        )
      },
    },
    {
      field: 'twinName',
      headerName: titleCase({
        text: t('labels.twin'),
        language,
      }),
      flex: 2,
      minWidth: 160,
      renderCell: ({ row: { twinId, twinName, siteId } }) =>
        insight?.twinId == null ? null : status === 'loading' ? (
          <Loader />
        ) : status === 'error' ? (
          <Icon
            icon="warning"
            css={css(({ theme }) => ({
              color: theme.color.intent.negative.fg.activated,
            }))}
          />
        ) : twinId && siteId && modelInfo ? (
          <TwinLink
            twin={{
              id: twinId,
              siteId,
            }}
            onClick={(e) => e.stopPropagation()}
          >
            <TwinChip
              modelOfInterest={modelInfo?.modelOfInterest}
              text={twinName}
              highlightOnHover
            />
          </TwinLink>
        ) : (
          twinName
        ),
    },
    {
      field: 'monitor',
      flex: 1,
      minWidth: 140,
      renderHeader: () => (
        <MonitorButton
          css={css(({ theme }) => ({
            color: isAllPointsSelected
              ? theme.color.neutral.fg.default
              : theme.color.neutral.fg.muted,
          }))}
          filled={isAllPointsSelected}
          kind={isAllPointsSelected ? 'primary' : 'secondary'}
          text={titleCase({
            text: t('plainText.monitorAll'),
            language,
          })}
          onClick={() => {
            // update period query param if there is no diagnostic points
            // and we click monitor all to add them all
            if (pointsSelected.length === 0) {
              onDiagnosticPeriodChange(selectedPeriod || null)
            }

            if (isAllPointsSelected) {
              pointsSelected.forEach((point) =>
                selectedPointsContext.onSelectPoint(point, false)
              )
              onDiagnosticPeriodChange(null)
            } else {
              pointsNotSelected.forEach((point) =>
                selectedPointsContext.onSelectPoint(point, true)
              )
            }
          }}
        />
      ),
      sortable: false,
      renderCell: ({ row: { id, siteId } }) => {
        const sitePointId = `${siteId}_${id}`
        const isSelected = selectedPointsContext.pointIds.includes(sitePointId)

        return (
          <MonitorButton
            kind="secondary"
            css={css(({ theme }) => ({
              color: isSelected
                ? theme.color.neutral.fg.default
                : theme.color.neutral.fg.muted,
            }))}
            text={titleCase({
              text: t('plainText.monitor'),
              language,
            })}
            filled={isSelected}
            iconColor={selectedPointsContext.pointColorMap[sitePointId]}
            onClick={() => {
              // if we are about to remove the last selected point, we should
              // call onDiagnosticPeriodChange(null) to reset the diagnostic period
              if (isSelected && selectedPointsContext.pointIds.length === 1) {
                onDiagnosticPeriodChange(null)
              } else {
                onDiagnosticPeriodChange(selectedPeriod || null)
              }
              selectedPointsContext.onSelectPoint(sitePointId, !isSelected)
            }}
          />
        )
      },
    },
  ]

  return (
    <StyledPanelGroup
      direction="vertical"
      className={styles.insightSummaryPanelContainer}
    >
      <Panel
        tw="border-0"
        css={css(({ theme }) => ({
          border: 'none',
          paddingTop: rows.length ? theme.spacing.s8 : 0,
        }))}
        id="insight-node-diagnostics-panel"
        title={
          rows.length > 0 && (
            <Stack>
              <Group>
                <div
                  css={css(({ theme }) => ({
                    ...theme.font.heading.md,
                  }))}
                >
                  {titleCase({
                    text: t('plainText.diagnosticOccurrences'),
                    language,
                  })}
                </div>
                <Icon
                  icon="info"
                  filled
                  data-tooltip={t('plainText.diagnosticTooltip')}
                  data-tooltip-position="top"
                  data-tooltip-width="250px"
                />
              </Group>
              <Select
                css={css(({ theme }) => ({
                  '& *': {
                    ...theme.font.body.sm.regular,
                    color: theme.color.neutral.fg.default,
                  },
                }))}
                tw="max-w-[350px]"
                data={diagnosticPeriods.map(({ value, label }) => ({
                  label,
                  value,
                }))}
                defaultValue={
                  diagnosticPeriods.length > 0 ? diagnosticPeriods[0].value : ''
                }
                value={selectedPeriod}
                onChange={onDiagnosticPeriodChange}
                disabled={
                  insightDiagnosticsQuery.status === 'success' &&
                  rows.length === 0
                }
              />
            </Stack>
          )
        }
      >
        <PanelContent
          css={css`
            height: ${insightDiagnosticsQuery.status !== 'success' ||
            rows.length === 0
              ? '100%'
              : 'auto'};
            overflow-x: hidden;
            padding: ${({ theme }) =>
              rows.length
                ? `${theme.spacing.s8} ${theme.spacing.s16} ${theme.spacing.s16} ${theme.spacing.s16}`
                : 0};
          `}
        >
          {insightDiagnosticsQuery.status === 'loading' ? (
            <FullSizeLoader />
          ) : insightDiagnosticsQuery.status === 'error' ? (
            <ErrorMessage />
          ) : rows.length ? (
            <DataGrid
              apiRef={apiRef}
              columns={columns}
              rows={rows}
              treeData
              getTreeDataPath={(row) => row.hierarchy}
              autosizeOnMount
              initialState={{
                pinnedColumns: {
                  right: ['monitor'],
                },
              }}
              groupingColDef={{
                headerName: t('plainText.checks'),
                hideDescendantCount: true,
                minWidth: 160,
                flex: 1,
                renderCell: (params) => (
                  <CustomGridTreeDataGroupingCell {...params} />
                ),
              }}
              // hide "Total Row" footer and remove bottom border per design
              // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/92377
              css={css`
                border-bottom: none;
                &&& .MuiDataGrid-footerContainer {
                  display: none;
                }
                & .MuiDataGrid-pinnedColumnHeaders {
                  padding: 0 !important;
                }
              `}
              disableRowSelectionOnClick
              onRowClick={onRowClick}
              isGroupExpandedByDefault={isGroupExpandedByDefault}
            />
          ) : (
            <NotFound
              size={20}
              message={t('plainText.diagnosticNotAvailable')}
            />
          )}
        </PanelContent>
      </Panel>
    </StyledPanelGroup>
  )
}

export default Diagnostics

// remove border on and <PanelHeader />
const StyledPanelGroup = styled(PanelGroup)`
  container-type: inline-size;
  container-name: insightSummaryPanelContainer;
  overflow-y: auto !important;
  & > div > div > div {
    border-bottom: none !important;
  }
`
function CustomGridTreeDataGroupingCell(props: GridRenderCellParams) {
  const {
    id,
    field,
    rowNode,
    row: { check },
  } = props
  const apiRef = useGridApiContext()
  const { t } = useTranslation()
  const notGroupNode = rowNode.type !== 'group'

  const isHierarchial =
    apiRef?.current?.state.rows.totalRowCount !==
    apiRef.current?.state.rows.totalTopLevelRowCount

  const handleClick: ButtonProps['onClick'] = (event) => {
    if (notGroupNode) {
      return
    }

    apiRef.current.setRowChildrenExpansion(id, !rowNode.childrenExpanded)
    apiRef.current.setCellFocus(id, field)
    event.stopPropagation()
  }

  return (
    <div
      css={css(({ theme }) => ({
        display: 'flex',
        gap: isHierarchial ? theme.spacing.s24 : 0,
        marginLeft: isHierarchial ? `${rowNode.depth * 16}px` : 0,
        Width: isHierarchial ? '180px' : '100px',
      }))}
    >
      <IconButton
        kind="secondary"
        background="transparent"
        onClick={handleClick}
        icon={
          (!notGroupNode && rowNode.childrenExpanded
            ? 'expand_more'
            : 'chevron_right') as IconName
        }
        css={css`
          display: ${isHierarchial ? 'block' : 'none'};
          visibility: ${notGroupNode ? 'hidden' : 'visible'};
        `}
      />
      <Badge
        size="md"
        prefix={<Icon icon={check ? 'check' : 'close'} />}
        color={check ? 'green' : 'red'}
        variant="subtle"
      >
        {_.capitalize(check ? t('plainText.pass') : t('plainText.fail'))}
      </Badge>
    </div>
  )
}

const DiagnosticIcon = ({
  filled,
  iconColor,
}: {
  filled: boolean
  iconColor?: string
}) => (
  <Icon
    icon="filter_none"
    filled={filled}
    css={css`
      transform: perspective(500px) rotateX(45deg) rotate(-45deg);
      color: ${iconColor};
    `}
  />
)

const MonitorButton = ({
  className = undefined,
  kind = 'secondary',
  iconColor,
  filled = false,
  text,
  onClick = _.noop,
}: {
  className?: string
  kind?: ButtonProps['kind']
  iconColor?: string
  filled?: boolean
  text: string
  onClick?: () => void
}) => (
  <Button
    className={className}
    prefix={<DiagnosticIcon iconColor={iconColor} filled={filled} />}
    kind={kind}
    onClick={(e) => {
      e.stopPropagation()
      onClick?.()
    }}
  >
    {text}
  </Button>
)
