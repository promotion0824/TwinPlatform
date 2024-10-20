import { Box, CircularProgress, Stack } from '@mui/material';
import { DataGridPro, GridColumnVisibilityModel, GridFilterModel, GridPaginationModel, GridSortDirection, GridSortModel, GridToolbarColumnsButton, GridToolbarContainer, GridToolbarFilterButton } from '@mui/x-data-grid-pro';
import { useEffect, useState } from 'react';
import { BsClockHistory } from 'react-icons/bs';
import { FaCircle } from 'react-icons/fa';
import { useQuery } from 'react-query';
import useApi from '../../hooks/useApi';
import useLocalStorage from "../../hooks/useLocalStorage";
import { BatchRequestDto, FileResponse, RuleDtoBatchDto, RuleMetadataDto } from '../../Rules';
import { ExportToCsv } from '../ExportToCsv';
import { CommandSyncFormatter, ModelFormatter2, ValidFormatterStatus, DateFormatter } from '../LinkFormatters';
import { RuleInstanceStatusLookup } from '../Lookups';
import StyledLink from '../styled/StyledLink';
import { buildCacheKey, gridPageSizes, mapFilterSpecifications, mapSortSpecifications, numberOperators, singleSelectCollectionOperators, singleSelectOperators, stringOperators } from './GridFunctions';

/**
 * Formats the valid column to show how many instances or a ? if rule has not yet been processed.
 * Adds a clock icon if the rule was processed recently.
 * @param m
 * @returns
 */
const ruleValidFormatter = (m: RuleMetadataDto) => {
  switch (m?.scanState) {
    case 1: return <>{m.validInstanceCount}&nbsp;<CircularProgress color="secondary" size={12} /></>;
    case 2: return <>{m.validInstanceCount} {((Date.now() - m.scanStateAsOf!.toDate().getDate()) < 10 * 60 * 1000) ?
      (<span>&nbsp;&nbsp;<BsClockHistory color="#70C" /></span>) : ""}</>;
  }
  return "?";
}

const scanErrorFormatter = (m: RuleMetadataDto) => {
  if (m && m.scanError !== null && m.scanError!.length > 0) {
    return <span><FaCircle color="red" title={m.scanError} /> </span>;
  }
  return "";
}

interface RulesGridProps {
  invokeQuery: (request: BatchRequestDto) => Promise<RuleDtoBatchDto>,
  downloadCsv: (request: BatchRequestDto) => Promise<FileResponse>,
  key: string,
  pageId: string
}

/**
 * Displays a grid of rules
 * */

const sortingOrder: GridSortDirection[] = ['desc', 'asc'];

const RulesGrid = ({ query }: { query: RulesGridProps }) => {

  const filterKey = buildCacheKey(`${query.pageId}_${query.key}_FilterModel`);
  const sortKey = buildCacheKey(`${query.pageId}_SortModel`);
  const colsKey = buildCacheKey(`${query.pageId}_ColumnModel`);
  const paginationKey = buildCacheKey(`${query.pageId}_PaginationModel`);
  const [filters, setFilters] = useLocalStorage<GridFilterModel>(filterKey, { items: [] });
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, [{ field: 'Name', sort: 'desc' }]);
  const [columnVisibilityModel, setColumnVisibilityModel] = useLocalStorage<GridColumnVisibilityModel>(colsKey, { Id: false, ScanError: false, TemplateId: false, Tags: false, ModifiedBy: false });
  const [paginationModel, setPaginationModel] = useLocalStorage<GridPaginationModel>(paginationKey, { pageSize: 10, page: 0 });

  const createRequest = function () {
    const request = new BatchRequestDto();
    request.filterSpecifications = mapFilterSpecifications(filters);
    request.sortSpecifications = mapSortSpecifications(sortModel);
    request.page = paginationModel.page + 1;//MUI grid is zero based
    request.pageSize = paginationModel.pageSize;
    return request;
  }

  const fetchRules = async () => {
    const request = createRequest();
    return query.invokeQuery(request);
  }

  const csvExport = {
    downloadCsv: (request: BatchRequestDto) => {
      return query.downloadCsv(request);
    },
    createBatchRequest: createRequest
  };

  const {
    isLoading,
    data,
    isFetching,
    isError
  } = useQuery(['ruleswithfilter', query.key, paginationModel, sortModel, filters], async () => await fetchRules(), { keepPreviousData: true })

  const apiclient = useApi();
  const fetchTags = useQuery(["ruleTags"], async () => {
    try {
      return await apiclient.ruleTags();
    } catch (error) {
      return []; // Return empty array in case of error
    }
  });

  const columns = [
    {
      field: 'Name', headerName: 'Name', flex: 2, minWidth: 200,
      valueGetter: (params: any) => params.row.name,
      // This link only works when used at the same level in url hierarchy
      renderCell: (params: any) => <StyledLink to={'/rule/' + encodeURIComponent(params.row.id)}>{params.row.name}</StyledLink>,
      filterOperators: stringOperators()
    },
    {
      field: 'LastModified', headerName: 'Last Modified', flex: 1, minWidth: 150, filterable: false,
      renderCell: (params: any) => {return DateFormatter(params.row.ruleMetadata?.lastModified);}
    },
    {
      field: 'ModifiedBy', headerName: 'Modified By', flex: 1, minWidth: 150,
      valueGetter: (params: any) => params.row.ruleMetadata?.modifiedBy,
      filterOperators: stringOperators()
    },
    {
      field: 'Category', headerName: 'Category', flex: 1, minWidth: 150,
      valueGetter: (params: any) => params.row.category,
      filterOperators: stringOperators()
    },
    {
      field: 'PrimaryModelId', headerName: 'Equipment', flex: 1, minWidth: 150,
      valueGetter: (params: any) => params.row.primaryModelId,
      renderCell: (params: any) => { return ModelFormatter2({ modelId: params.row.primaryModelId }); },
      filterOperators: stringOperators()
    },
    {
      field: 'RuleInstanceCount', headerName: 'Instances', flex: 1, minWidth: 100,
      valueGetter: (params: any) => params.row.ruleMetadata?.ruleInstanceCount,
      filterOperators: numberOperators()
    },
    {
      field: 'ValidInstanceCount', headerName: 'Valid', width: 100,
      renderCell: (params: any) => ruleValidFormatter(params.row.ruleMetadata),
      valueGetter: (params: any) => params.row.ruleMetadata?.validInstanceCount,
      filterOperators: numberOperators()
    },
    {
      field: 'RuleInstanceStatus', headerName: 'Status', width: 100, type: "singleSelect", cellClassName: "MuiDataGrid-cell--textCenter",
      valueOptions: () => { return RuleInstanceStatusLookup.GetStatusFilter(); },
      filterOperators: singleSelectOperators(),
      renderCell: (p: any) => { return ValidFormatterStatus(p.row.ruleMetadata?.ruleInstanceStatus); }
    },
    {
      field: 'ScanError', headerName: 'Scan Error', flex: 1, minWidth: 100,
      renderCell: (params: any) => scanErrorFormatter(params.row.ruleMetadata),
      filterOperators: stringOperators()
    },
    {
      field: 'CommandEnabled', headerName: 'Sync', width: 100, type: 'boolean',
      renderCell: (params: any) => { return CommandSyncFormatter(params.row!.commandEnabled); }
    },
    {
      field: 'InsightsGenerated', headerName: 'Insights', width: 100,
      valueGetter: (params: any) => params.row.ruleMetadata?.insightsGenerated,
      filterOperators: numberOperators()
    },
    {
      field: 'CommandsGenerated', headerName: 'Commands', width: 100,
      valueGetter: (params: any) => params.row.ruleMetadata?.commandsGenerated,
      filterOperators: numberOperators()
    },
    {
      field: 'Id', headerName: 'Id', flex: 1,
      valueGetter: (params: any) => params.row.id,
      filterOperators: stringOperators()
    },
    {
      field: 'TemplateId', headerName: 'Template Id', width: 100,
      valueGetter: (params: any) => params.row.templateId,
      filterOperators: stringOperators()
    },
    {
      field: 'Tags', headerName: 'Tags', flex: 1, minWidth: 200, type: "singleSelect",
      valueOptions: () => {
        if (fetchTags.isLoading || fetchTags.isError) {
          return []; // return empty array or loading indicator if data is not yet available
        }
        return fetchTags.data || []; // return the tags data if available
      },
      valueGetter: (params: any) => params.row.tags,
      filterOperators: singleSelectCollectionOperators(),
      valueFormatter: (params: any) => {
        const tagsArray = params.value as string[];
        return tagsArray?.join(", ");
      }
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

  // Some API clients return undefined while loading
  // Following lines are here to prevent `rowCountState` from being undefined during the loading
  const [rowCountState, setRowCountState] = useState(data?.total || 0);
  useEffect(() => {
    setRowCountState((prevRowCountState) => data?.total !== undefined ? data?.total : prevRowCountState);
  }, [data?.total, setRowCountState]);

  return (
    <Box sx={{ flex: 1 }}>
      <DataGridPro
        autoHeight
        loading={isLoading || isFetching}
        rows={data?.items || []}
        rowCount={rowCountState}
        pageSizeOptions={gridPageSizes()}
        columns={columns}
        pagination
        paginationModel={paginationModel}
        onPaginationModelChange={setPaginationModel}
        paginationMode="server"
        sortingMode="server"
        filterMode="server"
        onSortModelChange={handleSortModelChange}
        onFilterModelChange={handleFilterChange}
        sortingOrder={sortingOrder}
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
        slots={{
          toolbar: () => (
            <GridToolbarContainer>
              <Box sx={{ display: 'flex', flexGrow: 1, gap: 2 }}>
                <GridToolbarColumnsButton />
                <GridToolbarFilterButton />
                <ExportToCsv source={csvExport} />
              </Box>
            </GridToolbarContainer>
          ),
          noRowsOverlay: () => (
            <Stack margin={2}>
              {isError ? 'An error occured...' : 'No rows to display.'}
            </Stack>
          )
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
}

export default RulesGrid;
