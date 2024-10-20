import { DataGridPro, GridColDef, GridPaginationModel, GridSortDirection, GridSortModel, GridToolbarContainer } from '@mui/x-data-grid-pro';
import { BatchRequestDto, NamedPointDto } from '../../Rules';
import { ExportToCsv } from '../ExportToCsv';
import { buildCacheKey, gridPageSizes, createCsvFileResponse, stringOperators } from './GridFunctions';
import StyledLink from '../styled/StyledLink';
import useLocalStorage from '../../hooks/useLocalStorage';
import { Box, Grid, Stack, Typography } from '@mui/material';
import { useEffect, useState } from 'react';
import { Tooltip } from '@willowinc/ui';
import { FormatLocations } from '../StringOptions';

interface IPointEntitiesTableProps {
  calculatedPointId: string
  points: NamedPointDto[] | undefined,
  key: string,
  pageId: string
}

export const LinkFormatterNamedPoint = (_id: string, a: NamedPointDto) =>
  (<StyledLink to={"/equipment/" + encodeURIComponent(a.id!)}> {a.variableName}</StyledLink >);

const LinkFormatterId = (id: string) =>
  (<StyledLink to={"/equipment/" + encodeURIComponent(id)}> {id}</StyledLink >);

const sortingOrder: GridSortDirection[] = ['desc', 'asc'];

const PointEntitiesTable = ({ props }: { props: IPointEntitiesTableProps }) => {

  if (!props.points) return <>Waiting...</>

  const sortKey = buildCacheKey(`${props.pageId}_${props.key}_PointEntitiesTable_SortModel`);
  const paginationKey = buildCacheKey(`${props.pageId}_PointEntitiesTable_PaginationModel`);
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, []);
  const [paginationModel, setPaginationModel] = useLocalStorage<GridPaginationModel>(paginationKey, { pageSize: 10, page: 0 });

  const csvExport = {
    downloadCsv: (_: BatchRequestDto) => {
      return createCsvFileResponse(props.points?.map((v) => {
        return {
          Id: v.id,
          Name: v.variableName,
          Unit: v.unit,
          Location: FormatLocations(v.locations!)
        };
      }), "PointEntityIds.csv");
    },
    createBatchRequest: () => new BatchRequestDto()
  };

  const columnsParameters: GridColDef[] = [
    {
      field: 'id', headerName: 'Id', flex: 1,
      renderCell: (params: any) => { return LinkFormatterId(params.row.id); },
      filterOperators: stringOperators(),
    },
    {
      field: 'variableName', headerName: 'Name', flex: 1,
      renderCell: (params: any) => { return LinkFormatterNamedPoint(props.calculatedPointId, params.row); },
      filterOperators: stringOperators(),
    },
    {
      field: 'unit', headerName: 'Unit', width: 100,
      filterOperators: stringOperators(),
    },
    {
      field: 'locations', headerName: 'Location', flex: 2,
      renderCell: (params: any) => {
        var location = FormatLocations(params.row.locations);
        return (
          <Tooltip label={location} multiline>
            <Typography variant='body2'>{location}</Typography>
          </Tooltip>);
      },
      filterOperators: stringOperators(),
    }
  ];

  const handleSortModelChange = (newModel: GridSortModel) => {
    setSortModel(newModel);
    setPaginationModel({ pageSize: paginationModel.pageSize, page: 0 });
  };

  // Some API clients return undefined while loading
  // Following lines are here to prevent `rowCountState` from being undefined during the loading
  const [rowCountState, setRowCountState] = useState(props.points?.length || 0);
  useEffect(() => {
    setRowCountState((prevRowCountState) => props.points?.length !== undefined ? props.points?.length : prevRowCountState);
  }, [props.points?.length, setRowCountState]);

  return (
    <Box sx={{ flex: 1 }}>
      <DataGridPro
        autoHeight
        rows={props.points}
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

export default PointEntitiesTable;

