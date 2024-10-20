import { Box, Grid, Stack, Typography, Tooltip as MuiTooltip } from '@mui/material';
import { DataGridPro, GridColDef, GridPaginationModel, GridSortDirection, GridToolbarContainer } from '@mui/x-data-grid-pro';
import { Tooltip } from '@willowinc/ui';
import { useEffect, useState } from 'react';
import { FaRuler } from 'react-icons/fa';
import { HiOutlineLightBulb } from 'react-icons/hi';
import { RiBuilding2Fill, RiRulerLine } from 'react-icons/ri';
import { useQuery } from 'react-query';
import { useLocation } from 'react-router-dom';
import { ExportToCsv } from '../components/ExportToCsv';
import { buildCacheKey, gridPageSizes, stringOperators } from '../components/grids/GridFunctions';
import IconForModel from '../components/icons/IconForModel';
import { stripPrefix } from '../components/LinkFormatters';
import { FormatLocations } from '../components/StringOptions';
import StyledLink from '../components/styled/StyledLink';
import useApi from '../hooks/useApi';
import useLocalStorage from '../hooks/useLocalStorage';
import { BatchRequestDto, SearchLineDto } from '../Rules';

const resultFormatter = (props: SearchLineDto) => {
  const id = props.linkId ?? "NONE";
  const idsafe = encodeURIComponent(id);
  if (props.type == "insight") return (<StyledLink to={"/insight/" + idsafe}><HiOutlineLightBulb />Insight</StyledLink>);
  if (props.type == "ruleinstance") return (<StyledLink to={"/ruleinstance/" + idsafe}><RiRulerLine /> {id}</StyledLink>);
  if (props.type == "rule") return (<StyledLink to={"/rule/" + id}><FaRuler />{idsafe}</StyledLink>);
  if (props.type == "calculatedpoint") return (<StyledLink to={"/calculatedPoints/" + idsafe}><FaRuler />{id}</StyledLink>);
  if (props.type == "twin") return (<StyledLink to={"/equipment/" + idsafe}><RiBuilding2Fill />{id}</StyledLink>);
  if (props.type == "timeseries") return (<StyledLink to={"/equipment/" + idsafe}><RiBuilding2Fill />{id}</StyledLink>);
  if (props.type == "model")
    return (<StyledLink to={"/model/" + idsafe}>
      <IconForModel modelId={id} size={14} />&nbsp;
      <MuiTooltip title={id ?? 'missing id'} enterDelay={1000}>
        <span>{stripPrefix(id || '')}</span>
      </MuiTooltip>
    </StyledLink>);
  return (<span>{props.type}</span>);
}

const sortingOrder: GridSortDirection[] = ['desc', 'asc'];

const SearchPage = () => {

  const paginationKey = buildCacheKey(`SearchPage_PageSize`);
  //only store page size for search grid
  const [pageSize, setPageSize] = useLocalStorage(paginationKey, 10);
  const [paginationModel, setPaginationModel] = useState<GridPaginationModel>({ pageSize: pageSize, page: 0 });

  const query = new URLSearchParams(useLocation().search);
  const searchString = query.get("query");
  const apiclient = useApi();
  const searchResults = useQuery(['search', searchString], (_x) => {
    setPaginationModel({ pageSize: paginationModel.pageSize, page: 0 });
    return apiclient.search(searchString ?? "", 0, 1000);
  });

  const csvExport = {
    downloadCsv: (_: BatchRequestDto) => {
      return apiclient.exportSearch(searchString ?? "", 1000);
    },
    createBatchRequest: () => new BatchRequestDto()
  };

  const columns: GridColDef[] = [
    {
      field: 'type', headerName: 'Type', width: 160
    },
    {
      field: 'id', headerName: 'Link', flex: 2, minWidth: 200,
      renderCell: (params: any) => { return resultFormatter(params.row); }
    },
    {
      field: 'TwinLocations', headerName: 'Location', flex: 2, minWidth: 300, sortable: false,
      renderCell: (params: any) => {
        var location = FormatLocations(params.row.locations);
        return (
          <Tooltip label={location} position='bottom' multiline>
            <Typography variant='body2'>{location}</Typography>
          </Tooltip>);
      },
      filterOperators: stringOperators()
    },
    {
      field: 'description', headerName: 'Description', flex: 2, minWidth: 200
    }
  ];

  const handlePageModelChange = (newModel: GridPaginationModel) => {
    setPaginationModel(newModel);
    setPageSize(newModel.pageSize);
  };

  // Some API clients return undefined while loading
  // Following lines are here to prevent `rowCountState` from being undefined during the loading
  const [rowCountState, setRowCountState] = useState(searchResults.data?.results?.length || 0);
  useEffect(() => {
    setRowCountState((prevRowCountState) => searchResults.data?.results?.length !== undefined ? searchResults.data?.results?.length : prevRowCountState);
  }, [searchResults.data?.results?.length, setRowCountState]);

  return (
    <div>
      <h2>Search "{searchString}"</h2>
      {searchResults &&
        <Box sx={{ flex: 1 }}>
          <DataGridPro
            autoHeight
            rows={searchResults.data?.results ?? []}
            rowCount={rowCountState}
            pageSizeOptions={gridPageSizes()}
            loading={searchResults.isLoading}
            columns={columns}
            pagination
            paginationModel={paginationModel}
            onPaginationModelChange={handlePageModelChange}
            sortingOrder={sortingOrder}
            disableColumnSelector
            disableColumnFilter
            disableRowSelectionOnClick
            hideFooterSelectedRowCount
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
                  {searchResults.isError ? 'An error occured...' : 'No rows to display'}
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
        </Box>
      }
    </div>
  );
}

export default SearchPage;
