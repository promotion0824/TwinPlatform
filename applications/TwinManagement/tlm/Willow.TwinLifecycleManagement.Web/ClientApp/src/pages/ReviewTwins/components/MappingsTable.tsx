import { useState } from 'react';
import {
  GridColumnHeaderParams,
  GridColumnVisibilityModel,
  DataGridProProps,
  gridExpandedSortedRowIdsSelector,
  gridVisibleColumnDefinitionsSelector,
  GridCellParams,
  useGridApiRef,
  GridRenderEditCellParams,
  GridPreProcessEditCellProps,
  GridHeaderCheckbox,
  GridCellCheckboxRenderer,
  GridValueGetterParams,
  selectedIdsLookupSelector,
} from '@mui/x-data-grid-pro';
import LinearProgress from '@mui/material/LinearProgress';
import { usePersistentGridState } from '../../../hooks/usePersistentGridState';
import { useMappings, TabsName } from '../MappingsProvider';
import { IMappedEntry, UpdateMappedEntry, Status, MappedEntry } from '../../../services/Clients';
import EditIcon from '@mui/icons-material/Edit';
import { Icon, Tooltip } from '@willowinc/ui';
import { useEffect } from 'react';
import StatusBadge from './StatusBadge';
import { IGetMappedEntries } from '../hooks/useGetMappedEntries';
import styled from '@emotion/styled';
import TwinIdSelector from './TwinIdSelector';
import useOntology from '../../../hooks/useOntology/useOntology';
import ModelIdSelector from './ModelIdSelector';
import { StyledDataGrid } from '../../../components/Grid/StyledDataGrid';

export default function MappingsTable({
  getMappedEntriesQuery,
  slots,
  tableApiRef,
  rowsState,
}: {
  getMappedEntriesQuery?: IGetMappedEntries;
  slots: any;
  tableApiRef: ReturnType<typeof useGridApiRef>;
  rowsState: [Record<TabsName, IMappedEntry[]>, React.Dispatch<React.SetStateAction<Record<TabsName, IMappedEntry[]>>>];
}) {
  const {
    putMappedEntryMutate,
    selectedRowsState,
    isLoadingState,
    cellCoordinateState,
    syncRowsState,
    tabState,
    buildingIdsState,
    connectorIdState,
    twinsLookup,
    selectAllState,
  } = useMappings();
  const { data: ontology, isSuccess: isOntologySuccess } = useOntology();

  const { data: twinsLookupData, isSuccess: isTwinsLookupSuccess } = twinsLookup;

  const isDependenciesLoaded = isOntologySuccess && isTwinsLookupSuccess;

  const { savedState } = usePersistentGridState(tableApiRef, 'mappings-v9', isDependenciesLoaded);

  const columns = [
    {
      field: '__check__',
      type: 'checkboxSelection',
      width: 50,
      resizable: false,
      sortable: false,
      filterable: false,
      hideable: false,
      disableColumnMenu: true,
      disableReorder: true,
      disableExport: true,
      renderHeader: (params: any) => {
        return (
          <StyledGridHeaderCheckbox
            {...params}
            checked={selectAllState[0].selectAll}
            onClick={() => {
              const isCheckboxIndeterminate = tableApiRef.current
                .getColumnHeaderElement('__check__')
                ?.querySelector('input[type="checkbox"]')
                ?.getAttribute('data-indeterminate');

              const isCheckboxChecked = tableApiRef.current
                .getColumnHeaderElement('__check__')
                ?.querySelector('div.MuiDataGrid-checkboxInput')
                ?.getAttribute('data-checked');

              if (isCheckboxIndeterminate) {
                selectAllState[1]((prev) => ({ ...prev, selectAll: false }));
              } else if (!isCheckboxChecked && !isCheckboxIndeterminate) {
                selectAllState[1]((prev) => ({ ...prev, selectAll: true }));
              } else {
                selectAllState[1]((prev) => ({ ...prev, selectAll: false }));
              }
            }}
          />
        );
      },
      renderCell: (params: any) => (
        <GridCellCheckboxRenderer
          {...params}
          onClick={() => {
            selectAllState[1]((prev) => ({ ...prev, selectAll: false }));
          }}
        />
      ),
      valueGetter: (params: any) => {
        const selectionLookup = selectedIdsLookupSelector((params as GridValueGetterParams).api.state);

        if (selectAllState[0].selectAll) {
          return true;
        }

        return selectionLookup[params.id] !== undefined;
      },
    },
    {
      field: 'name',
      headerName: 'Twin Name',
      flex: 0.9,
      editable: true,
      renderHeader: (params: GridColumnHeaderParams) => <EditableHeader headerName={params?.colDef?.headerName!} />,
    },
    {
      field: 'willowId',
      headerName: 'Willow Twin ID',
      flex: 0.6,
      valueGetter: (params: any) => params.row.willowId,
      editable: true,
      renderHeader: (params: GridColumnHeaderParams) => <EditableHeader headerName={params?.colDef?.headerName!} />,
      renderEditCell: (params: GridRenderEditCellParams) => {
        return <TwinIdSelector {...params} />;
      },
    },
    ...(tabState[0] !== 'miscellaneous'
      ? [
          {
            field: 'buildingId',
            headerName: 'Building',
            flex: 0.6,
            renderCell: ({ value }: { value: string }) => {
              let buildingValue = twinsLookupData?.getTwinById(value)?.name ?? `${value || ''}`;
              return <span title={buildingValue}>{buildingValue}</span>;
            },
          },
        ]
      : []),
    ...(!['spaces', 'miscellaneous'].includes(tabState[0] || '')
      ? [
          {
            field: 'connectorId',
            headerName: 'Connector',
            flex: 0.6,
            renderCell: ({ value }: { value: string }) => {
              let connectorValue = twinsLookupData?.getTwinById(value)?.name ?? `${value || ''}`;
              return <span title={connectorValue}>{connectorValue}</span>;
            },
          },
        ]
      : []),
    {
      field: 'mappedModelId',
      headerName: 'Mapped Model',
      flex: 1,
    },
    {
      field: 'willowModelId',
      headerName: 'Willow Model',
      flex: 1,
      editable: true,
      renderCell: ({ value }: { value: string }) => {
        return <span title={value}>{ontology?.getModelById(value)?.name ?? `${value ?? '(Model not defined)'}`}</span>;
      },
      renderHeader: (params: GridColumnHeaderParams) => <EditableHeader headerName={params?.colDef?.headerName!} />,
      renderEditCell: (params: GridRenderEditCellParams) => {
        return <ModelIdSelector {...params} />;
      },
      preProcessEditCellProps: (params: GridPreProcessEditCellProps) => {
        const hasError = !ontology?.getModelById(params.props.value)?.name;

        return { ...params.props, error: hasError };
      },
    },
    {
      field: 'mappedIdAndWillowExternalId',
      headerName: 'Mapped ID/Willow External ID',
      flex: 1,
      valueGetter: (params: any) => params.row.mappedId,
      renderCell: (params: any) => <DuplicateTwinFoundCell row={params.row} />,
    },
    {
      field: 'isExistingTwin',
      headerName: 'Existing Twin (Y/N)',
      flex: 1,
      valueGetter: (params: any) => (params.row.isExistingTwin ? 'Yes' : 'No'),
    },
    {
      field: 'parentMappedId',
      headerName: 'Parent Mapped Id',
      flex: 0.6,
    },
    {
      field: 'parentWillowId',
      headerName: 'Related Twin Id',
      flex: 0.6,
      // todo: set to twin name from the id
      valueGetter: (params: any) => params.row.parentWillowId,
    },
    {
      field: 'willowParentRel',
      headerName: 'Relationship Type',
      flex: 0.8,
      valueGetter: (params: any) => params.row.willowParentRel,
    },
    {
      field: 'status',
      headerName: 'Status',
      flex: 0.6,
      editable: true,
      type: 'singleSelect',
      valueOptions: ({ row }: { row: MappedEntry }) => {
        return getStatusValues(row);
      },
      renderHeader: (params: GridColumnHeaderParams) => <EditableHeader headerName={params?.colDef?.headerName!} />,
      valueGetter: (params: any) => params.row.status || Status.Pending,
      renderCell: (params: any) => <StatusBadge status={params.value} />,
    },
    {
      field: 'statusNotes',
      headerName: 'Status Notes',
      flex: 1,
      editable: true,
      renderHeader: (params: GridColumnHeaderParams) => <EditableHeader headerName={params?.colDef?.headerName!} />,
    },
    {
      field: 'description',
      headerName: 'Description',
      flex: 1,
      editable: true,
      renderHeader: (params: GridColumnHeaderParams) => <EditableHeader headerName={params?.colDef?.headerName!} />,
    },

    {
      field: 'modelInformation',
      headerName: 'Model Information',
      flex: 1,
    },

    {
      field: 'auditInformation',
      headerName: 'Audit Information',
      flex: 1,
    },
    {
      field: 'timeCreated',
      headerName: 'Time Created',
      flex: 1,
    },
    {
      field: 'timeLastUpdated',
      headerName: 'Time Last Updated',
      flex: 1,
    },
  ];

  const { query, pageSizeState = [0, () => {}], offsetState = [0, () => {}] } = getMappedEntriesQuery || {};

  const { data = { items: [], total: 0 }, isLoading: isMappedEntriesLoading, isFetching } = query || {};

  const totalCountState = useState<number>(0);

  useEffect(() => {
    totalCountState[1](data.total!);
  }, [data.total, totalCountState]);

  const { mutateAsync, isLoading: isPutLoading } = putMappedEntryMutate;

  const isGridLoading = isMappedEntriesLoading || isPutLoading || isLoadingState[0] !== null || isFetching;

  // Default column visibility settings. Display/hide columns fields.
  const [columnVisibilityModel, setColumnVisibilityModel] = useState<GridColumnVisibilityModel>({
    name: true,
    willowId: true,
    mappedId: true,
    mappedModelId: true,
    willowModelId: true,
    mappedIdAndWillowExternalId: true,
    isExistingTwin: false,
    status: true,
    parentMappedId: true,
    parentWillowId: true,
    connectorId: true,
    buildingId: true,
    willowParentRel: true,
    statusNotes: false,
    description: false,
    modelInformation: false,
    auditInformation: false,
    timeCreated: false,
    timeLastUpdated: false,
  });

  const { columnVisibilityModel: columnVisibilityModelState } = savedState?.columns || {};
  const {
    name = true,
    willowId = true,
    mappedId = true,
    mappedModelId = true,
    willowModelId = true,
    connectorId = true,
    buildingId = true,
    mappedIdAndWillowExternalId = true,
    isExistingTwin = false,
    status = true,
    willowParentRel = true,
    parentWillowId = true,
    parentMappedId = false,
    statusNotes = false,
    description = false,
    modelInformation = false,
    auditInformation = false,
    timeCreated = false,
    timeLastUpdated = false,
  } = columnVisibilityModelState || {};

  useEffect(() => {
    setColumnVisibilityModel({
      name,
      willowId,
      mappedId,
      mappedModelId,
      willowModelId,
      mappedIdAndWillowExternalId,
      isExistingTwin,
      status,
      buildingId,

      connectorId,
      willowParentRel,
      parentWillowId,
      parentMappedId,
      statusNotes,
      description,
      modelInformation,
      auditInformation,
      timeCreated,
      timeLastUpdated,
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const [selectedRows, setSelectedRows] = selectedRowsState;
  const [rows, setRows] = rowsState;

  // sync new rows with previous rows updates
  useEffect(
    () => {
      if (!tabState[0]) return;

      // update only the rows that are visible in the current tab
      const rowIds = rows[tabState[0]].map((row) => row.mappedId!);
      const updateRows = syncRowsState[0][tabState[0]!].filter((row) => rowIds.includes(row?.mappedId));
      if (updateRows.length > 0) tableApiRef.current.updateRows(updateRows);

      // remove rows that have been deleted
      const deletedRows = syncRowsState[0][tabState[0]!]
        .filter(({ _action }) => _action === 'delete')
        .map((row) => row.mappedId!);

      if (deletedRows.length > 0) {
        setRows((prev) => {
          prev[tabState[0]!] = prev[tabState[0]!].filter((row) => !deletedRows.includes(row.mappedId));
          return prev;
        });
      }
    },

    // eslint-disable-next-line react-hooks/exhaustive-deps
    [rows, syncRowsState[0], tabState[0]]
  );

  const handleOnRowsScrollEnd: DataGridProProps['onRowsScrollEnd'] = () => {
    if (
      isLoadingState[0] === null &&
      !isFetching &&
      !isMappedEntriesLoading &&
      rows[tabState[0]!].length < totalCountState[0] &&
      rows[tabState[0]!].length !== 0 &&
      offsetState[0] < totalCountState[0]
    ) {
      offsetState[1]((prev) => prev + pageSizeState[0]);
    }
  };

  // Todo: fix janky way of syncing rows with new data, this is a temporary fix. use react-query's infinite query instead
  useEffect(() => {
    setRows((prev) => {
      let newRows = [...prev[tabState[0]!], ...data.items!];

      let newRowIds = newRows.map(({ mappedId }) => mappedId);

      return {
        ...prev,
        [tabState[0]!]: newRows.filter(({ mappedId }, index) => !newRowIds.includes(mappedId, index + 1)),
      };
    });

    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isMappedEntriesLoading, data, buildingIdsState[0], connectorIdState[0]]);

  // keep cell focus position insync with arrow keys shortcuts
  const handleCellClick = (params: GridCellParams) => {
    const rowIndex = gridExpandedSortedRowIdsSelector(tableApiRef).findIndex((id) => id === params.id);
    const colIndex = gridVisibleColumnDefinitionsSelector(tableApiRef).findIndex(
      (column) => column.field === params.field
    );
    cellCoordinateState[1]({ rowIndex, colIndex });
  };

  return (
    <StyledDataGrid
      apiRef={tableApiRef}
      initialState={savedState}
      rows={rows[tabState[0]!]}
      getRowId={(row) => row.mappedId!}
      // @ts-ignore
      columns={columns}
      slots={{ ...slots, loadingOverlay: LinearProgress }}
      loading={isGridLoading}
      disableRowSelectionOnClick={isGridLoading}
      rowCount={totalCountState[0]}
      columnVisibilityModel={columnVisibilityModel}
      onColumnVisibilityModelChange={(newModel) => setColumnVisibilityModel(newModel)}
      processRowUpdate={(updatedRow: IMappedEntry) => {
        let newRow = new UpdateMappedEntry(updatedRow);
        return mutateAsync(newRow);
      }}
      onProcessRowUpdateError={() => {
        let editRowIds = Object.keys(tableApiRef.current.state.editRows);
        editRowIds.forEach((id: string) => {
          tableApiRef.current.stopRowEditMode({ id, ignoreModifications: true });
        });
      }}
      checkboxSelection
      onRowSelectionModelChange={(ids) => {
        if (isLoadingState[0] === null) setSelectedRows(ids);
        if (selectAllState[0].selectAll) {
          selectAllState[1]((prev) => ({ ...prev, selectAll: false }));
        }
      }}
      rowSelectionModel={selectedRows}
      onFilterModelChange={() => {
        setSelectedRows([]);
      }}
      editMode="row"
      onCellKeyDown={(params, event) => {
        // disable key shortcuts default behavior
        if (printableKeys.includes(event.key)) event.defaultMuiPrevented = true;
      }}
      hideFooterPagination
      onRowsScrollEnd={handleOnRowsScrollEnd}
      onCellClick={handleCellClick}
      isCellEditable={(params) => params.row.status !== Status.Created}
    />
  );
}

const printableKeys = [
  'ArrowUp',
  'ArrowDown',
  'ArrowLeft',
  'ArrowRight',
  'Tab',
  'a',
  'b',
  'c',
  'd',
  'e',
  'f',
  'g',
  'h',
  'i',
  'j',
  'k',
  'l',
  'm',
  'n',
  'o',
  'p',
  'q',
  'r',
  's',
  't',
  'u',
  'v',
  'w',
  'x',
  'y',
  'z',
  'A',
  'B',
  'C',
  'D',
  'E',
  'F',
  'G',
  'H',
  'I',
  'J',
  'K',
  'L',
  'M',
  'N',
  'O',
  'P',
  'Q',
  'R',
  'S',
  'T',
  'U',
  'V',
  'W',
  'X',
  'Y',
  'Z',
  '0',
  '1',
  '2',
  '3',
  '4',
  '5',
  '6',
  '7',
  '8',
  '9',
  ' ', // Space
  '!',
  '@',
  '#',
  '$',
  '%',
  '^',
  '&',
  '*',
  '(',
  ')',
  '_',
  '-',
  '+',
  '{',
  '}',
  '[',
  ']',
  '|',
  '\\',
  ':',
  ';',
  '<',
  '>',
  '?',
  ',',
  '.',
  '/',
  "'",
  '"',
  '`',
  '~',
];

function getStatusValues(row: MappedEntry) {
  const { status, isExistingTwin } = row || {};

  if (isExistingTwin) {
    return [Status.Ignore];
  }

  if (row === undefined) return [Status.Pending, Status.Approved, Status.Ignore, Status.Created];

  switch (status) {
    case Status.Pending:
      return [Status.Pending, Status.Approved, Status.Ignore];
    case Status.Approved:
      return [Status.Approved, Status.Pending, Status.Ignore];
    case Status.Ignore:
      return [Status.Ignore, Status.Pending, Status.Approved];
    case Status.Created:
    default:
      return [];
  }
}

function EditableHeader({ headerName }: { headerName: string }) {
  return (
    <Flex>
      <span>{headerName}</span>
      <EditIcon sx={{ fontSize: 16, margin: '0px 0px 3px 6px' }} />
    </Flex>
  );
}

function DuplicateTwinFoundCell({ row }: { row: any }) {
  return (
    <Flex>
      {row.isExistingTwin && (
        <IconWrapper>
          <Tooltip label={<TooltipContent />} position="bottom-start">
            <Icon icon="info" size={16} />
          </Tooltip>
        </IconWrapper>
      )}
      <span title={row.mappedId}>{row.mappedId}</span>
    </Flex>
  );
}

function TooltipContent() {
  return (
    <TooltipContentWrapper>
      <p>
        Mapped Twin matches an existing Willow Twin that is already mapped to a Mapped twin (via the externalID field).
      </p>
      Contact Mapped to resolve this issue.
    </TooltipContentWrapper>
  );
}

const TooltipContentWrapper = styled('div')({ textWrap: 'wrap', maxWidth: 170 });

const Flex = styled('div')({
  display: 'flex',
  flexDirection: 'row',
  gap: 4,
  alignItems: 'center',
  overflow: 'hidden',
  textOverflow: 'ellipsis',
});

const IconWrapper = styled('div')({ color: '#D77570', display: 'flex', alignItems: 'center' });

const StyledGridHeaderCheckbox = styled(GridHeaderCheckbox)({
  '& .mantine-Checkbox-input[data-indeterminate="true"]': {
    backgroundColor: '#5945d7 !important',
    borderColor: '#5945d7 !important',
  },
});
