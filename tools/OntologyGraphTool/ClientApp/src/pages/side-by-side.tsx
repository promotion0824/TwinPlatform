import { Box } from "@mui/material";
import { useQuery } from "@tanstack/react-query";
import useLocalStorage from "../hooks/uselocalstorage";
import { DataGrid, GridColumnVisibilityModel, GridFilterModel, GridPaginationModel, GridSortModel } from "@mui/x-data-grid";
import { IBatchRequestDto, mapFilterSpecifications, mapSortSpecifications, stringOperators } from "../hooks/gridfunctions";


const SideBySide = () => {

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

  const fetchOntologies = async () => {
    const res = await fetch("/api/ontology/get-mapped-ontology", {
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
        field: 'id', headerName: 'Id', flex: 4, minWidth: 300,
        renderCell: (params: any) => { return params.row.Id; },
        filterOperators: stringOperators()
      },
      {
        field: 'name', headerName: 'Name', flex: 2, minWidth: 300,
        renderCell: (params: any) => { return params.row.displayName?.en ?? ""; },
        filterOperators: stringOperators()
      },
      {
        field: 'extends', headerName: 'Extends', flex: 2, minWidth: 300,
        renderCell: (params: any) => { return params.row.extends; },
        filterOperators: stringOperators()
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
    async () => fetchOntologies(), { keepPreviousData: true })



  if (isLoading) return (<div>Loading...</div>);
  if (!data) return (<div>No data...</div>);
  if (!data.items) return (<div>No items...</div>);

  return (
    <Box sx={{ flex: 1 }}>
      <DataGrid
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
    </Box>
  );
}


export default SideBySide;
