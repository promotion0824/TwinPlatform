import { DataGridPro, GridPaginationModel, GridSortDirection, GridSortModel, GridToolbarContainer } from '@mui/x-data-grid-pro';
import useApi from '../../hooks/useApi';
import { useQuery } from 'react-query';
import { ModelFormatter2, LinkFormatter2, WithTooltipFormatter } from '../LinkFormatters';
import { ExportToCsv } from '../ExportToCsv';
import { BatchRequestDto } from '../../Rules';
import { buildCacheKey, gridPageSizes, createCsvFileResponse, stringOperators } from './GridFunctions';
import { Box, Grid, Stack, Tooltip, Typography } from '@mui/material';
import useLocalStorage from '../../hooks/useLocalStorage';
import { useEffect, useState } from 'react';

interface IEquipmentCapabilityTableProps {
  twinId: string | undefined,
  pageId: string
}

const sortingOrder: GridSortDirection[] = ['desc', 'asc'];

const EquipmentCapabilityTable = ({ props }: { props: IEquipmentCapabilityTableProps }) => {

  if (!props.twinId) return <>Waiting...</>

  const sortKey = buildCacheKey(`${props.pageId}_${props.twinId}_EquipmentCapabilityTable_SortModel`);
  const paginationKey = buildCacheKey(`${props.pageId}_EquipmentCapabilityTable_PaginationModel`);
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, []);
  const [paginationModel, setPaginationModel] = useLocalStorage<GridPaginationModel>(paginationKey, { pageSize: 10, page: 0 });

  const apiclient = useApi();

  const equipmentQuery = useQuery(["equipmentWithRels", props.twinId], async (s) => {

    const equipment = await apiclient.equipmentWithRelationships(props.twinId);
    console.log('equipment', equipment);

    return equipment;
  });

  const equipment = equipmentQuery.data;

  const csvExport = {
    downloadCsv: (request: BatchRequestDto) => {
      return createCsvFileResponse(equipment?.capabilities?.map((v) => {
        return {
          Name: v.name,
          Model: v.modelId,
          Id: v.id,
          Tags: v.tags,
          Unit: v.unit
        };
      }), "AvailableBindings.csv");
    },
    createBatchRequest: () => new BatchRequestDto()
  };

  const columnsParameters = [
    {
      field: 'name', headerName: 'Name', flex: 2, minWidth: 200,
      renderCell: (params: any) => { return LinkFormatter2(params.row) },
      filterOperators: stringOperators()
    },

    {
      field: 'id', headerName: 'Id', flex: 2, minWidth: 200,
      renderCell: (params: any) => { return WithTooltipFormatter(params.row) },
      filterOperators: stringOperators()
    },

    {
      field: 'modelId', headerName: 'Model', flex: 2, minWidth: 200,
      valueGetter: (params: any) => params.row.modelId,
      renderCell: (params: any) => { return ModelFormatter2(params.row); },
    filterOperators: stringOperators()
    },

    {
      field: 'tags', headerName: 'Tags', flex: 2, minWidth: 200,
      renderCell: (params: any) => { return (<Tooltip title={params.row.tags}><Typography>{params.row.tags}</Typography></Tooltip>) },
      filterOperators: stringOperators()
    },
    {
      field: 'unit', headerName: 'Unit', flex: 1, minWidth: 160,
      filterOperators: stringOperators() }
  ];

  const handleSortModelChange = (newModel: GridSortModel) => {
    setSortModel(newModel);
    setPaginationModel({ pageSize: paginationModel.pageSize, page: 0 });
  };

  // Some API clients return undefined while loading
  // Following lines are here to prevent `rowCountState` from being undefined during the loading
  const [rowCountState, setRowCountState] = useState(equipment?.capabilities?.length || 0);
  useEffect(() => {
    setRowCountState((prevRowCountState) => equipment?.capabilities?.length !== undefined ? equipment?.capabilities?.length : prevRowCountState);
  }, [equipment?.capabilities?.length, setRowCountState]);

  return (
    <Box sx={{ flex: 1 }}>
      <DataGridPro
        autoHeight 
        loading={!equipmentQuery.isFetched}
        rows={equipment?.capabilities ?? []}
        rowCount={rowCountState}
        pageSizeOptions={gridPageSizes()}
        columns={columnsParameters}
        pagination
        paginationModel={paginationModel}
        onPaginationModelChange={setPaginationModel}
        onSortModelChange={handleSortModelChange}
        sortingOrder={sortingOrder}
        disableColumnSelector
        disableColumnFilter
        disableRowSelectionOnClick
        hideFooterSelectedRowCount
        initialState={{
          sorting: {
            sortModel: [...sortModel]
          }
        }}
        slots={{
          toolbar: () => (
            <GridToolbarContainer>
              <Grid container item>
                <ExportToCsv source={csvExport} />
              </Grid>
            </GridToolbarContainer>
          ),
          noRowsOverlay: () => (
            <Stack margin={2}>
              {'No rows to display'}
            </Stack>
          ),
        }}
        sx={{
          '& .MuiDataGrid-row:hover': {
            backgroundColor: "transparent"
          },
          '& .MuiDataGrid-cell:focus': {
            outline: ' none'
          }
        }}
      />
    </Box>);
};

export default EquipmentCapabilityTable;
