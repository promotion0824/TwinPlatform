import _ from 'lodash'
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useHistory } from 'react-router-dom'
import { styled } from 'twin.macro'
import { useTable, useRowSelect, useSortBy } from 'react-table'
import { titleCase } from '@willow/common'
import {
  api,
  Progress,
  IconNew,
  useSnackbar,
  TwinChip,
  useFeatureFlag,
  SortIndicator,
  Popover,
  useUser,
} from '@willow/ui'
import TwinModelChip from '@willow/common/twins/view/TwinModelChip'
import {
  getModelOfInterest,
  levelModelId,
} from '@willow/common/twins/view/modelsOfInterest'

import { getFloorName, RelatedTwins } from './ResultsList'
import { useSearchResults as InjectedSearchResults } from '../../state/SearchResults'
import IndeterminateCheckbox from './IndeterminateCheckbox'
import { TwinLink } from '../../../../shared'
import SiteChip from '../../../../page/ui/SiteChip'

const Table = styled.table({
  borderCollapse: 'separate',
  borderSpacing: 0,
})

const THead = styled.thead({
  textAlign: 'left',
  fontSize: '11px',
})
const TBody = styled.tbody({})
const TR = styled.tr({
  height: '47px',
  verticalAlign: 'center',
  '& td, th': {
    padding: '0 1rem',
  },
})
const TH = styled.th(({ theme }) => ({
  position: 'sticky',
  top: 0,
  backgroundColor: theme.color.neutral.bg.panel.default,
  borderBottom: '1px solid #383838',
  fontWeight: '500',
  zIndex: 1,
}))
const TD = styled.td({
  borderBottom: '1px solid #383838',
})

const Link = styled(TwinLink)({
  color: '#D9D9D9',
  fontWeight: '500',
})

const RelatedTwinsContainer = styled.div(({ theme }) => ({
  margin: `${theme.spacing.s6} 0`,
  gap: theme.spacing.s6,
  display: 'flex',
  flexWrap: 'wrap',
}))

const floorColumnId = 'floor'

/**
 * Returns the first related twin that the sensor is a capability of,
 * meaning that it is able to be used with Time Series.
 */
export function getRelatedTwinFromSensorForTimeSeries(
  sensor,
  ontology,
  modelsOfInterest
) {
  return _(sensor.outRelationships)
    .filter((relationship) => relationship.name === 'isCapabilityOf')
    .groupBy((r) => r.modelId)
    .orderBy((twins) => twins.length, 'desc')
    .orderBy((twins) => {
      const modelOfInterest = getModelOfInterest(
        twins[0].modelId,
        ontology,
        modelsOfInterest
      )
      return modelsOfInterest.indexOf(modelOfInterest)
    })
    .map((v) => v[0])
    .value()[0]
}

/**
 * Returns IDs ready to be used with TimeSeries.
 * The assetId is the first related twin that the sensor is a capability of,
 * prepended with the siteId.
 * The sensorId also has the siteId combined with the sensor's trendId.
 * If there are no related twins, undefined is returned.
 */
export async function formatSensorForTimeSeries({
  modelsOfInterest,
  ontology,
  sensor,
}) {
  const relatedTwin = getRelatedTwinFromSensorForTimeSeries(
    sensor,
    ontology,
    modelsOfInterest
  )

  if (!relatedTwin) return undefined

  const { siteId } = sensor

  const twinResponse = await api.get(
    `/v2/sites/${siteId}/twins/${relatedTwin.targetId}`
  )

  const twinId = twinResponse.data.twin.uniqueID
  // TODO: It's still being determined whether trendId should be a top level property or not.
  const trendId = JSON.parse(sensor.rawTwin).customProperties.trendID
  const assetId = `${siteId}_${twinId}`
  const sensorId = `${siteId}_${trendId}`
  return { assetId, sensorId }
}

const ResultsTable = ({
  endOfPageRef,
  useSearchResults = InjectedSearchResults,
}) => {
  const {
    t,
    queryKey,
    sites,
    ontology,
    modelsOfInterest,
    hasNextPage,
    isLoadingNextPage,
    twins,
    exportSelected,
    exportAll,
    searchType,
  } = useSearchResults()

  const history = useHistory()
  const user = useUser()

  const levelModel = modelsOfInterest.find(
    (modelOfInterest) => modelOfInterest.modelId === levelModelId
  )

  const openInTimeSeries = async (selectedSensors) => {
    if (selectedSensors.length > 50)
      throw new Error(t('plainText.openInTimeSeriesDisabled'))

    const timeSeriesIds = (
      await Promise.all(
        selectedSensors.map((sensor) =>
          formatSensorForTimeSeries({
            modelsOfInterest,
            ontology,
            sensor,
            usingCognitiveSearch: true,
          })
        )
      )
    ).filter((v) => v)

    const previousAssets = user.options.timeSeriesImport?.assets || []
    const previousSensors = user.options.timeSeriesImport?.sensors || []

    const assets = _.uniq([
      ...timeSeriesIds.map((v) => v.assetId),
      ...previousAssets,
    ])

    const sensors = _.uniq([
      ...timeSeriesIds.map((v) => v.sensorId),
      ...previousSensors,
    ])

    if (!assets.length) throw new Error(t('plainText.noValidSensorsError'))

    user.saveOptions('timeSeriesImport', {
      assets,
      sensors,
    })

    history.push('/time-series')
  }

  const {
    i18n: { language },
  } = useTranslation()

  // We need to cache if the user has selected all because this information is lost
  // when new data is loaded. We restore this in the later useEffect
  const [cachedHasSelectedAll, setCachedHasSelectedAll] = useState(false)
  const cachedQueryKeyRef = useRef(queryKey)
  const featureFlags = useFeatureFlag()

  const columns = useMemo(
    () => [
      {
        Header: t('labels.name'),
        accessor: 'name',
        Cell: ({ value, row }) => {
          const twin = row.original
          return (
            <Link color="#D9D9D9" fontWeight="500" twin={twin}>
              {value || t('plainText.unnamedTwin')}
            </Link>
          )
        },
      },
      {
        Header: t('twinExplorer.table.type'),
        id: 'type',
        accessor: ({ modelId }) =>
          getModelOfInterest(modelId, ontology, modelsOfInterest),
        Cell: ({ row }) => {
          const { modelId } = row.original
          const model = ontology.getModelById(modelId)
          const modelOfInterest = getModelOfInterest(
            modelId,
            ontology,
            modelsOfInterest
          )
          return (
            <TwinModelChip model={model} modelOfInterest={modelOfInterest} />
          )
        },
      },
      ...(searchType === 'sensors'
        ? [
            {
              Header: titleCase({
                text: t('plainText.capabilityOf'),
                language,
              }),
              id: 'capabilityOf',
              accessor: 'outRelationships',
              Cell: ({ value }) => (
                <RelatedTwinsContainer>
                  <RelatedTwins
                    modelsOfInterest={modelsOfInterest}
                    ontology={ontology}
                    relationships={value.filter(
                      (relationship) => relationship.name === 'isCapabilityOf'
                    )}
                  />
                </RelatedTwinsContainer>
              ),
            },
          ]
        : []),
      {
        Header: t('twinExplorer.table.location'),
        id: 'location',
        accessor: 'siteId',
        Cell: ({ value: siteId }) => {
          const site = sites.find((s) => s.id === siteId)
          return <SiteChip siteName={site.name} />
        },
      },
      ...(!featureFlags.hasFeatureToggle('cognitiveSearch')
        ? [
            {
              Header: t('twinExplorer.table.files'),
              id: 'numberOfFiles',
              accessor: ({ inRelationships, outRelationships }) => {
                const relationships = [...inRelationships, ...outRelationships]
                return relationships.filter(
                  (relationship) => relationship.name === 'hasDocument'
                ).length
              },
            },
            {
              id: floorColumnId,
              Header: t('labels.floor'),
              accessor: ({ outRelationships, floorName }) => {
                if (floorName != null) {
                  return floorName
                }

                return outRelationships?.find(
                  (relationship) =>
                    relationship?.name === 'locatedIn' &&
                    relationship?.modelId === levelModelId
                )?.twinName
              },
              Cell: ({ row }) =>
                getFloorName(row.original) &&
                levelModel != null && (
                  <TwinChip
                    variant="instance"
                    modelOfInterest={levelModel}
                    text={getFloorName(row.original)}
                  />
                ),
              sortType: 'floorSort',
            },
            {
              Header: t('twinExplorer.table.relatedTwins'),
              id: 'numberOfRelatedTwins',
              accessor: ({ inRelationships, outRelationships }) => {
                const relationships = [...inRelationships, ...outRelationships]
                return relationships.filter(
                  (relationship) =>
                    relationship.name !== 'hasDocument' &&
                    relationship.name !== 'isCapabilityOf'
                ).length
              },
            },
            ...(searchType !== 'sensors'
              ? [
                  {
                    Header: t('twinExplorer.table.sensors'),
                    id: 'numberOfSensors',
                    accessor: ({ inRelationships, outRelationships }) => {
                      const relationships = [
                        ...inRelationships,
                        ...outRelationships,
                      ]
                      return relationships.filter(
                        (relationship) => relationship.name === 'isCapabilityOf'
                      ).length
                    },
                  },
                ]
              : []),
          ]
        : []),
    ],
    [featureFlags, modelsOfInterest, ontology, sites, t]
  )

  const sortTypes = {
    floorSort: (rowA, rowB) => {
      // Default to an empty string if no floor name exists
      const a = getFloorName(rowA.original) || ''
      const b = getFloorName(rowB.original) || ''

      // If there's no number in the floor (such as "UG") set the floor to a very low number
      let aNum = +a.match(/\d+/)?.[0] || -1000
      let bNum = +b.match(/\d+/)?.[0] || -1000

      // If the floor appears to be a basement ("B1") make the number negative
      if (a[0]?.toLowerCase() === 'b') aNum *= -1
      if (b[0]?.toLowerCase() === 'b') bNum *= -1

      return aNum > bNum ? 1 : bNum > aNum ? -1 : 0
    },
  }

  const getRowId = useCallback((row) => row.id, [])
  const {
    getTableProps,
    getTableBodyProps,
    headerGroups,
    rows,
    prepareRow,
    toggleAllRowsSelected,
  } = useTable(
    {
      columns,
      data: twins,
      getRowId,
      autoResetSelectedRows: !_.isEqual(cachedQueryKeyRef.current, queryKey),
      autoResetSortBy: isLoadingNextPage,
      sortTypes,
    },
    useSortBy,
    useRowSelect,
    (hooks) => {
      hooks.visibleColumns.push((columns) => [
        {
          id: 'selection',
          Header: ({
            isAllRowsSelected,
            getToggleAllRowsSelectedProps,
            selectedFlatRows,
          }) => {
            const { onChange, ...toggleAllRowsSelectedProps } =
              getToggleAllRowsSelectedProps()
            return (
              <HeaderCheckbox
                isAllRowsSelected={isAllRowsSelected}
                numTwinsSelected={selectedFlatRows.length}
                searchType={searchType}
                toggleAllRowsSelectedProps={{
                  ...toggleAllRowsSelectedProps,
                  onChange: (event) => {
                    setCachedHasSelectedAll(event.target.checked)
                    onChange(event)
                  },
                }}
                onExport={() =>
                  isAllRowsSelected
                    ? exportAll()
                    : exportSelected(selectedFlatRows.map((r) => r.original))
                }
                onOpenInTimeSeries={() =>
                  openInTimeSeries(selectedFlatRows.map((r) => r.original))
                }
              />
            )
          },
          Cell: ({ row, selectedFlatRows, flatRows }) => {
            const { onChange, ...toggleRowSelectedProps } =
              row.getToggleRowSelectedProps()
            return (
              <div>
                <IndeterminateCheckbox
                  {...toggleRowSelectedProps}
                  onChange={(event) => {
                    const hasSelectedLastCheckbox =
                      selectedFlatRows.length === flatRows.length - 1 &&
                      event.target.checked
                    setCachedHasSelectedAll(hasSelectedLastCheckbox)
                    onChange(event)
                  }}
                />
              </div>
            )
          },
        },
        ...columns,
        {
          id: 'actions',
          Header: t('twinExplorer.table.actions'),
          Cell: ({ row }) => (
            <div>
              <ExportButton onClick={() => exportSelected([row.original])} />
            </div>
          ),
        },
      ])
    }
  )

  useEffect(() => {
    // Check if the search query has changed by comparing the queryKey. If there is a new
    // search query, we reset the cached "selectAll" state, with checkboxes being reset
    // automatically via the autoResetSelectedRows prop.
    // Otherwise, we ensure all rows to remain selected as the paginated results are loaded.
    if (!_.isEqual(cachedQueryKeyRef.current, queryKey)) {
      cachedQueryKeyRef.current = queryKey
      setCachedHasSelectedAll(false)
    } else if (cachedHasSelectedAll) {
      toggleAllRowsSelected(true)
    }
  }, [
    twins.length,
    cachedHasSelectedAll,
    toggleAllRowsSelected,
    cachedQueryKeyRef,
    queryKey,
  ])

  return (
    <Table {...getTableProps()}>
      <THead>
        {headerGroups.map((headerGroup) => (
          <TR {...headerGroup.getHeaderGroupProps()}>
            {headerGroup.headers.map((column) => {
              // only make floor column sortable per:
              // https://dev.azure.com/willowdev/Unified/_workitems/edit/76024
              const isFloorColumn = column.id === floorColumnId
              return (
                <TH
                  {...column.getHeaderProps(
                    isFloorColumn
                      ? column.getSortByToggleProps({ title: undefined }) // pass undefined title option to remove default tooltip
                      : undefined
                  )}
                >
                  {isFloorColumn ? (
                    <SortIndicator
                      isSorted={column.isSorted}
                      $transform={
                        column.isSortedDesc
                          ? 'translateY(12px)'
                          : 'translateY(-12px) rotate(-180deg)'
                      }
                    >
                      {column.render('Header')}
                    </SortIndicator>
                  ) : (
                    column.render('Header')
                  )}
                </TH>
              )
            })}
          </TR>
        ))}
      </THead>
      <TBody {...getTableBodyProps()}>
        {rows.map((row) => {
          prepareRow(row)
          return (
            <TR data-testid="display-table-list" {...row.getRowProps()}>
              {row.cells.map((cell) => (
                <TD
                  data-testid="display-table-list-data"
                  {...cell.getCellProps()}
                >
                  {cell.render('Cell')}
                </TD>
              ))}
            </TR>
          )
        })}
        <TR>
          <td colSpan="999">
            {isLoadingNextPage ? (
              <div style={{ margin: 'auto' }}>
                <Progress />
              </div>
            ) : hasNextPage ? (
              // to be detected inside an inner div, it needs to have content or content after
              // so put a non-breaking space in here
              <div ref={endOfPageRef}>{'\u00a0'}</div>
            ) : null}
          </td>
        </TR>
      </TBody>
    </Table>
  )
}

function HeaderCheckbox({
  isAllRowsSelected,
  numTwinsSelected,
  searchType,
  toggleAllRowsSelectedProps,
  onExport,
  onOpenInTimeSeries,
}) {
  const { t } = useTranslation()
  const checkboxRef = useRef()

  return (
    <div>
      <IndeterminateCheckbox
        ref={checkboxRef}
        {...toggleAllRowsSelectedProps}
      />

      {numTwinsSelected > 0 && (
        <Popover target={checkboxRef} position="top">
          <PopoverContent>
            <SelectionCount>
              {isAllRowsSelected
                ? t(
                    searchType === 'sensors'
                      ? 'interpolation.sensorsSelectedAll'
                      : 'interpolation.twinsSelectedAll'
                  )
                : t(
                    searchType === 'sensors'
                      ? 'interpolation.sensorsSelectedCount'
                      : 'interpolation.twinsSelectedCount',
                    {
                      count: numTwinsSelected,
                    }
                  )}
            </SelectionCount>
            <div tw="flex-initial content-center">
              <ExportButton onClick={onExport} />
              {searchType === 'sensors' && (
                <>
                  <ButtonDivider />
                  <OpenInTimeSeriesButton onClick={onOpenInTimeSeries} />
                </>
              )}
            </div>
          </PopoverContent>
        </Popover>
      )}
    </div>
  )
}

function OpenInTimeSeriesButton({ onClick }) {
  const [isOpening, setIsOpening] = useState(false)
  const snackbar = useSnackbar()

  const {
    i18n: { language },
    t,
  } = useTranslation()

  return (
    <StyledIcon
      data-tooltip={titleCase({
        text: t('plainText.openInTimeSeries'),
        language,
      })}
      data-tooltip-animate={false}
      data-tooltip-position="top"
      icon={isOpening ? 'progress' : 'graph'}
      onClick={async () => {
        if (isOpening) return
        setIsOpening(true)
        try {
          await onClick()
        } catch (err) {
          snackbar.show(err.message)
          setIsOpening(false)
        }
      }}
      size="small"
    />
  )
}

function ExportButton({ onClick }) {
  const { t } = useTranslation()
  const [isSaving, setIsSaving] = useState(false)
  const snackbar = useSnackbar()

  return (
    <StyledIcon
      data-tooltip={t('plainText.export')}
      data-tooltip-animate={false}
      data-tooltip-position="top"
      icon={isSaving ? 'progress' : 'export'}
      // The progress spinner looks a bit too big if we don't set it to a
      // smaller size than the export button.
      size={isSaving ? 'small' : 'medium'}
      onClick={async () => {
        setIsSaving(true)
        try {
          await onClick()
        } catch (e) {
          console.error(e)
          snackbar.show(t('plainText.errorExportingTwins'))
        }
        setIsSaving(false)
      }}
    />
  )
}

const ButtonDivider = styled.span(({ theme }) => ({
  borderLeft: `1px solid ${theme.color.neutral.border.default}`,
  margin: `0 ${theme.spacing.s6}`,
}))

const PopoverContent = styled.div({
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'space-between',
  color: '#d9d9d9',
  fontSize: 12,
  fontFamily: 'Poppins',
  fontWeight: 500,
})

const SelectionCount = styled.div(({ theme }) => ({
  marginRight: theme.spacing.s6,
}))

const StyledIcon = styled(IconNew)({
  verticalAlign: 'middle',
  cursor: 'pointer',
  // Fix the height so it doesn't resize when we switch to/from the loading
  // state
  height: 24,
  fill: '#7E7E7E',

  '&:hover': {
    fill: 'var(--primary5)',
  },

  '&:hover #fill': {
    fill: 'var(--primary5)',
  },
})

export default ResultsTable
