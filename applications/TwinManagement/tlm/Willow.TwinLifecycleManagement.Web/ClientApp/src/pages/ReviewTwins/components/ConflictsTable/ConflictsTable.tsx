import { useMemo, useState } from 'react';
import {
  GridColumnVisibilityModel,
  DataGridProProps,
  GRID_DETAIL_PANEL_TOGGLE_COL_DEF,
  useGridApiRef,
  GridHeaderCheckbox,
  GridCellCheckboxRenderer,
  selectedIdsLookupSelector,
  GridValueGetterParams,
} from '@mui/x-data-grid-pro';
import { LinearProgress } from '@mui/material';
import { usePersistentGridState } from '../../../../hooks/usePersistentGridState';
import { UpdateMappedTwinRequestResponse, IMappedEntry } from '../../../../services/Clients';
import useGetUpdateTwinRequests from '../../hooks/useGetUpdateTwinRequests';
import useGetUpdateTwinRequestsCount from '../../hooks/useGetUpdateTwinRequestsCount';
import { useEffect } from 'react';
import { useMappings, TabsName } from '../../MappingsProvider';
import CustomDetailPanelToggle from '../../../../components/Grid/CustomDetailPanelToggle';
import ConflictsTablePanelContent from './ConflictsTablePanelContent';
import { StyledDataGrid } from '../../../../components/Grid/StyledDataGrid';

export default function ConflictsTable({
  slots,
  tableApiRef,
  rowsState,
}: {
  slots: any;
  tableApiRef: ReturnType<typeof useGridApiRef>;
  rowsState: [
    Record<TabsName, IMappedEntry[] | UpdateMappedTwinRequestResponse[]>,
    React.Dispatch<React.SetStateAction<Record<TabsName, IMappedEntry[] | UpdateMappedTwinRequestResponse[]>>>
  ];
}) {
  const { selectedRowsState, syncRowsState, selectAllState } = useMappings();
  const { savedState } = usePersistentGridState(tableApiRef, 'conflictsTable-v1');

  const columns: any = useMemo(
    () => [
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
        renderCell: (params: any) => (
          <GridCellCheckboxRenderer
            {...params}
            onClick={() => {
              selectAllState[1]((prev) => ({ ...prev, selectAll: false }));
            }}
          />
        ),
        renderHeader: (params: any) => {
          return (
            <GridHeaderCheckbox
              {...params}
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
      },
      {
        field: 'willowTwinId',
        headerName: 'Willow TwinID',
        flex: 0.5,
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
      {
        field: 'changedProperties',
        headerName: 'Conflicts',
        flex: 1,
        valueGetter: (params: any) => {
          return params?.row?.changedProperties?.length || 0;
        },
      },
      {
        ...GRID_DETAIL_PANEL_TOGGLE_COL_DEF,
        renderCell: (params: any) => <CustomDetailPanelToggle id={params.id} value={params.value} />,
      },
    ],
    // eslint-disable-next-line react-hooks/exhaustive-deps
    []
  );

  const { query, pageSizeState, offsetState } = useGetUpdateTwinRequests();
  const { data: totalCountData = 0 } = useGetUpdateTwinRequestsCount();

  useEffect(() => {
    selectAllState[1]((prev) => ({ ...prev, totalCount: totalCountData }));
  }, [totalCountData, selectAllState]);

  const { data = [], isLoading: isUpdateTwinRequestsLoading, isFetching } = query;

  const isGridLoading = isUpdateTwinRequestsLoading;

  // Default column visibility settings. Display/hide columns fields.
  const [columnVisibilityModel, setColumnVisibilityModel] = useState<GridColumnVisibilityModel>({
    name: true,
    mappedId: true,
    mappedModelId: true,
    timeCreated: true,
    timeLastUpdated: false,
  });

  const { columnVisibilityModel: columnVisibilityModelState } = savedState?.columns || {};
  const {
    id = true,
    willowTwinId = true,
    changedProperties = true,
    timeCreated = true,
    timeLastUpdated,
  } = columnVisibilityModelState || {};

  useEffect(() => {
    setColumnVisibilityModel({
      id,
      willowTwinId,
      changedProperties,
      timeCreated,
      timeLastUpdated,
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isGridLoading]);

  const [rows, setRows] = rowsState;
  const handleOnRowsScrollEnd: DataGridProProps['onRowsScrollEnd'] = () => {
    if (!isFetching && tableApiRef.current.getAllRowIds().length < totalCountData && rows['conflicts'].length !== 0) {
      offsetState[1]((prev) => prev + pageSizeState[0]);
    }
  };

  useEffect(() => {
    setRows((prev) => {
      // Todo: fix janky way of syncing rows with new data, this is a temporary fix. use react-query's infinite query instead
      let newRows = [...prev['conflicts'], ...data] as UpdateMappedTwinRequestResponse[];
      let newRowIds = newRows.map(({ id }) => id);

      return {
        ...prev,
        conflicts: newRows.filter(({ id }, index) => !newRowIds.includes(id, index + 1)),
      };
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [offsetState[0], isUpdateTwinRequestsLoading]);

  // sync new rows with all previous rows updates
  useEffect(
    () => {
      if (syncRowsState[0]['conflicts']?.length > 0) {
        tableApiRef.current.updateRows(syncRowsState[0]['conflicts']);
      }
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [rows]
  );

  const [selectedRows, setSelectedRows] = selectedRowsState;

  return (
    <StyledDataGrid
      apiRef={tableApiRef}
      initialState={savedState}
      rows={rows['conflicts']}
      getRowId={(row) => row.id!}
      columns={columns}
      slots={{ ...slots, loadingOverlay: LinearProgress }}
      loading={isGridLoading}
      disableRowSelectionOnClick={isGridLoading}
      rowCount={totalCountData}
      columnVisibilityModel={columnVisibilityModel}
      onColumnVisibilityModelChange={(newModel) => setColumnVisibilityModel(newModel)}
      checkboxSelection
      onRowSelectionModelChange={(ids) => {
        setSelectedRows(ids);
        if (selectAllState[0].selectAll) {
          selectAllState[1]((prev) => ({ ...prev, selectAll: false }));
        }
      }}
      rowSelectionModel={selectedRows}
      onFilterModelChange={() => {
        setSelectedRows([]);
      }}
      hideFooterPagination
      onRowsScrollEnd={handleOnRowsScrollEnd}
      getDetailPanelContent={({ row }) => <ConflictsTablePanelContent row={row} />}
      getDetailPanelHeight={() => 'auto'}
    />
  );
}
