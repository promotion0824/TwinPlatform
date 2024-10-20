import { Box, Button, Grid, Stack, Typography } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import useLocalStorage from "../hooks/uselocalstorage";
import { DataGrid, GridColumnVisibilityModel, GridFilterModel, GridPaginationModel, GridSortModel } from "@mui/x-data-grid";
import { IBatchRequestDto, mapFilterSpecifications, mapSortSpecifications, numberOperators, stringOperators } from "../hooks/gridfunctions";
import { useNavigate } from "react-router-dom";

const ModelFormatter = (text: string) =>
  <span style={{ fontWeight: 600 }}>{text}</span>;

const HideAlternativeSubject = (text: string, index: number) => index == 0 ?
  <span>{text}</span> : <></>;


const Assignment = () => {

  const navigate = useNavigate();

  const filterKey = 'Ontology_FilterModel';
  const sortKey = 'Ontology_SortModel';
  const colsKey = 'Ontology_ColumnModel';
  const paginationKey = 'Ontology_PaginationModel';

  const [filters, setFilters] = useLocalStorage<GridFilterModel>(filterKey, { items: [] });
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, [{ field: 'LastFaultedDate', sort: 'desc' }]);
  const [columnVisibilityModel, setColumnVisibilityModel] = useLocalStorage<GridColumnVisibilityModel>(colsKey, { TimeZone: false, EquipmentId: false, Valid: false, CommandInsightId: false });
  const [paginationModel, setPaginationModel] = useLocalStorage<GridPaginationModel>(paginationKey, { pageSize: 10, page: 0 });

  const createRequest = function () {
    const request: IBatchRequestDto = {};
    request.filterSpecifications = mapFilterSpecifications(filters);
    request.sortSpecifications = mapSortSpecifications(sortModel);
    request.page = paginationModel.page + 1;//MUI grid is zero based
    request.pageSize = paginationModel.pageSize;
    return request;
  }

  const fetchMappings = async () => {
    const res = await fetch("/api/mapping/get-mappings", {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(createRequest())
    });
    return res.json();
  };

  const columns =
    [
      {
        field: 'source', headerName: 'Source', flex: 4, minWidth: 450,
        renderCell: (params: any) => {
          return HideAlternativeSubject(
            params.row.source?.id ?? "",
            params.row.index);
        },
        filterOperators: stringOperators()
      },
      {
        field: 'destination', headerName: 'Destination', flex: 4, minWidth: 450,
        renderCell: (params: any) => {
          return ModelFormatter(params.row.destination?.id ?? "");
        },
        filterOperators: stringOperators()
      },
      {
        field: 'score', headerName: 'Score', flex: 2, minWidth: 100,
        renderCell: (params: any) => { return params.row.score.toPrecision(3); },
        filterOperators: numberOperators()
      },
      {
        field: 'nameScore', headerName: 'NameScore', flex: 2, minWidth: 100,
        renderCell: (params: any) => { return params.row.nameScore.toPrecision(3); },
        filterOperators: numberOperators()
      },
      {
        field: 'ancestorScore', headerName: 'AncestorScore', flex: 2, minWidth: 100,
        renderCell: (params: any) => { return params.row.ancestorScore.toPrecision(3); },
        filterOperators: numberOperators()
      }
    ];

  const handleSortModelChange = (newModel: GridSortModel) => {
    setSortModel(newModel);
    setPaginationModel({ pageSize: paginationModel.pageSize, page: 0 });
  };

  const handleFilterChange = (newModel: GridFilterModel) => {
    setFilters(newModel);
    setPaginationModel({ pageSize: paginationModel.pageSize, page: 0 });
  };

  const {
    isLoading,
    data,
    isFetching,
    isError
  } = useQuery(['ontologies', paginationModel, sortModel, filters],
    async () => fetchMappings(), { keepPreviousData: true })

  if (isLoading) return (<div>Loading...</div>);
  if (isFetching) return (<div>Fetching...</div>);
  if (isError) return (<div>Error...</div>);

  return (
    <Box sx={{ flex: 1, paddingTop: 2 }}>

      <Stack spacing={2}>
        <Grid container>
          <Grid item xs={12} md={4}>
            <Typography variant="h1">Mappings</Typography>
          </Grid>
          <Grid item xs={12} md={8}>
            <div style={{ float: 'right', paddingLeft: 5 }}>
              <a href={"/api/File/download"} download>
                <Button variant="outlined" color="secondary">Download</Button>
              </a>
            </div>
          </Grid>
        </Grid>

        <DataGrid
          // getRowId={(data) => data.row.Source.id + data.row.Destination.id}
          autoHeight
          rows={data!.items}
          rowCount={data!.total}
          pageSizeOptions={[10, 20, 50]}
          columns={columns}
          pagination
          paginationModel={paginationModel}
          paginationMode="server"
          sortingMode="server"
          filterMode="server"
          onRowClick={(x: any) => navigate('/assignment/' + encodeURIComponent(x.row.id))}
          onPaginationModelChange={setPaginationModel}
          onSortModelChange={handleSortModelChange}
          onFilterModelChange={handleFilterChange}
          //            sortingOrder={sortingOrder}
          columnVisibilityModel={columnVisibilityModel}
          onColumnVisibilityModelChange={(newModel: any) => setColumnVisibilityModel(newModel)}
          disableRowSelectionOnClick
          hideFooterSelectedRowCount
          initialState={{
            filter: {
              filterModel: { ...filters },
            },
            sorting: {
              sortModel: [...sortModel]
            }
          }}
        />
      </Stack>

    </Box>
  );
}

export default Assignment;
