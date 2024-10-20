import { Box, Stack } from '@mui/material';
import { DataGridPro, GridPaginationModel, GridSortDirection, GridSortModel } from '@mui/x-data-grid-pro';
import { useEffect, useState } from 'react';
import useLocalStorage from '../../hooks/useLocalStorage';
import { OutputValueDto } from '../../Rules';
import { buildCacheKey, gridPageSizes } from './GridFunctions';

interface IOutputValuesForCalcPointProps {
  outputValues: OutputValueDto[] | undefined,
  key: string,
  pageId: string
}

const sortingOrder: GridSortDirection[] = ['desc', 'asc'];

const OutputValues = ({ props }: { props: IOutputValuesForCalcPointProps }) => {

  const sortKey = buildCacheKey(`${props.pageId}_${props.key}_OutputValues_SortModel`);
  const paginationKey = buildCacheKey(`${props.pageId}_OutputValues_PaginationModel`);
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, []);
  const [paginationModel, setPaginationModel] = useLocalStorage<GridPaginationModel>(paginationKey, { pageSize: 10, page: 0 });

  const columnsParameters = [
    {
      field: 'startTime', headerName: 'StartTime', width: 180,
      renderCell: (params: any) => params.row!.startTime!.format('ddd, MM/DD HH:mm:ss')
    },
    {
      field: 'endTime', headerName: 'EndTime', width: 180,
      renderCell: (params: any) => params.row!.endTime!.format('ddd, MM/DD HH:mm:ss')
    },
    {
      field: 'duration', headerName: 'Duration', width: 180
    },
    {
      field: 'isValid', headerName: 'IsValid', width: 120, sortable: false
    },
    {
      field: 'faulted', headerName: 'Faulted', width: 120, sortable: false
    },
    {
      field: 'text', headerName: 'Text', flex: 3
    }
  ];

  const handleSortModelChange = (newModel: GridSortModel) => {
    setSortModel(newModel);
  };

  // Some API clients return undefined while loading
  // Following lines are here to prevent `rowCountState` from being undefined during the loading
  const [rowCountState, setRowCountState] = useState(props.outputValues?.length || 0);
  useEffect(() => {
    setRowCountState((prevRowCountState) => props.outputValues?.length !== undefined ? props.outputValues?.length : prevRowCountState);
  }, [props.outputValues?.length, setRowCountState]);

  return (
    <Box sx={{ flex: 1 }}>
      <DataGridPro
        autoHeight
        rows={(props.outputValues ?? []).map((x: OutputValueDto, i) => ({ ...x, id: i }))}
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
        hideFooterSelectedRowCount
        initialState={{
          sorting: {
            sortModel: [...sortModel]
          }
        }}
        slots={{
          noRowsOverlay: () => (
            <Stack margin={2}>
              {'No rows to display'}
            </Stack>
          ),
        }}
      />
    </Box>);
};

export default OutputValues;
