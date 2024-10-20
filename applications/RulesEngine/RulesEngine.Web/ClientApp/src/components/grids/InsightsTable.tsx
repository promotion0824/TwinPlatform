import { Box, Stack, Typography } from '@mui/material';
import { DataGridPro, GridColDef, GridColumnVisibilityModel, GridFilterModel, GridPaginationModel, GridSortDirection, GridSortModel, GridToolbarColumnsButton, GridToolbarContainer, GridToolbarFilterButton } from '@mui/x-data-grid-pro';
import { Tooltip } from '@willowinc/ui';
import { useEffect, useState } from 'react';
import { useQuery } from 'react-query';
import useApi from '../../hooks/useApi';
import useLocalStorage from '../../hooks/useLocalStorage';
import { BatchRequestDto, InsightImpactDto } from '../../Rules';
import compareStringsLexNumeric from '../AlphaNumericSorter';
import { ExportToCsv } from '../ExportToCsv';
import { GetInsightStatusFilter, InsightStatusFormatter } from '../insights/InsightStatusFormatter';
import { CommandSyncFormatter, DateFormatter, InsightFaultyFormatter, InsightLastFaultedFormatter, InsightLinkFormatterOnEquipment, InsightLinkFormatterOnRuleName, InsightValidFormatter } from '../LinkFormatters';
import { FormatLocations } from '../StringOptions';
import { buildCacheKey, gridPageSizes, doubleOperators, guidOperators, mapFilterSpecifications, mapSortSpecifications, singleSelectOperators, singleSelectCollectionOperators, stringOperators } from './GridFunctions';

interface IInsightsTableProps {
  ruleId: string,
  /**
   * The id of the page that is making the request so that sort and filters are remembered per page
   */
  pageId: string
}

const sortingOrder: GridSortDirection[] = ['desc', 'asc'];

const InsightsTable3 = (props: IInsightsTableProps) => {
  const ruleId = props.ruleId ?? "none";

  const filterKey = buildCacheKey(`${props.pageId}_${ruleId}_FilterModel`);
  const sortKey = buildCacheKey(`${props.pageId}_SortModel`);
  const colsKey = buildCacheKey(`${props.pageId}_ColumnModel`);
  const paginationKey = buildCacheKey(`${props.pageId}_PaginationModel`);
  const [filters, setFilters] = useLocalStorage<GridFilterModel>(filterKey, { items: [] });
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, [{ field: 'LastFaultedDate', sort: 'desc' }]);
  const [columnVisibilityModel, setColumnVisibilityModel] = useLocalStorage<GridColumnVisibilityModel>(colsKey, { TimeZone: false, Id: false, Valid: false, CommandInsightId: false, LastUpdatedUTC: false, LastSyncDateUTC: false, Tags: false });
  const [paginationModel, setPaginationModel] = useLocalStorage<GridPaginationModel>(paginationKey, { pageSize: 10, page: 0 });

  const createRequest = function () {
    const request = new BatchRequestDto();
    request.filterSpecifications = mapFilterSpecifications(filters);
    request.sortSpecifications = mapSortSpecifications(sortModel);
    request.page = paginationModel.page + 1;//MUI grid is zero based
    request.pageSize = paginationModel.pageSize;
    return request;
  }

  const apiclient = useApi();

  const fetchInsights = async () => {
    const request = createRequest();
    return apiclient.getInsightsAfter(ruleId, request);
  };

  const csvExport = {
    downloadCsv: (request: BatchRequestDto) => {
      return apiclient.exportInsightsAfter(ruleId, request);
    },
    createBatchRequest: createRequest
  };

  const {
    isLoading,
    data,
    isFetching,
    isError
  } = useQuery(['insights', props.ruleId, paginationModel, sortModel, filters], async () => fetchInsights(), { keepPreviousData: true })

  const fetchTags = useQuery(["ruleTags"], async () => {
    try { return await apiclient.ruleTags(); }
    catch (error) { return []; }
  });

  // Define columns for insight
  let initialColumns: GridColDef[] =
    [
      {
        field: 'IsFaulty', headerName: 'Faulty', width: 70, type: 'boolean',
        renderCell: (params: any) => { return InsightFaultyFormatter(params.row!); }
      },
      {
        field: 'IsValid', headerName: 'Valid', width: 70, type: 'boolean',
        renderCell: (params: any) => { return InsightValidFormatter(params.row!); }
      },
      {
        field: 'Status', headerName: 'Status', width: 100, type: "singleSelect", filterOperators: singleSelectOperators(), valueOptions: () => { return GetInsightStatusFilter(); },
        renderCell: (params: any) => {
          return InsightStatusFormatter(params.row!);
        }
      },
      {
        field: 'LastFaultedDate', headerName: 'Last Faulted', width: 160, filterable: false,
        renderCell: (params: any) => { return InsightLastFaultedFormatter(params.row!); }
      },
      {
        field: 'LastUpdatedUTC', headerName: 'Last Update Date UTC', width: 160, filterable: false,
        renderCell: (params: any) => { return DateFormatter(params.row.lastUpdatedUTC!); }
      },
      {
        field: 'LastSyncDateUTC', headerName: 'Last Sync Date UTC', width: 160, filterable: false,
        renderCell: (params: any) => { return DateFormatter(params.row.lastSyncDateUTC!); }
      },
      {
        field: 'RuleName', headerName: 'Insight', flex: 2, minWidth: 200,
        sortComparator: (v1: any, v2: any, _param1: any, _param2: any) =>
          compareStringsLexNumeric(v1?.toString() ?? "", v2?.toString() ?? ""),
        renderCell: (params: any) => { return InsightLinkFormatterOnRuleName(params.row!); },
        filterOperators: stringOperators()
      },
      {
        field: 'EquipmentName', headerName: 'Equipment', flex: 2, minWidth: 250,
        valueGetter: (params: any) => params.row.equipmentName,
        filterOperators: stringOperators()
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
        field: 'CommandEnabled', headerName: 'Sync', width: 65, type: 'boolean',
        renderCell: (params: any) => { return CommandSyncFormatter(params.row!.commandEnabled); }
      },
      {
        field: 'EquipmentId', headerName: 'Equipment Id', flex: 2, minWidth: 250,
        renderCell: (params: any) => { return InsightLinkFormatterOnEquipment(params.row!); },
        filterOperators: stringOperators()
      },
      {
        field: 'Id', headerName: 'Id', flex: 2, minWidth: 250,
        valueGetter: (params: any) => params.row.id,
        filterOperators: stringOperators()
      },
      {
        field: 'CommandInsightId', headerName: 'Command Id', flex: 2, minWidth: 250,
        filterOperators: guidOperators(),
        valueGetter: (params: any) => params.row.commandInsightId
      },
      {
        field: 'TimeZone', headerName: 'Time Zone', flex: 1, minWidth: 180,
        valueGetter: (params: any) => params.row.timeZone,
        filterOperators: stringOperators()
      },
      {
        field: 'RuleTags', headerName: 'Tags', flex: 1, minWidth: 200, type: "singleSelect", sortable: false,
        valueOptions: () => (fetchTags.isLoading || fetchTags.isError) ? [] : fetchTags.data || [],
        valueGetter: (params: any) => params.row.ruleTags,
        filterOperators: singleSelectCollectionOperators(),
        valueFormatter: (params: any) => {
          const tagsArray = params.value as string[];
          return tagsArray?.join(", ");
        }
      }
    ];

  const [columns, setColumnState] = useState(initialColumns);

  useEffect(() => {
    if (data === undefined) {
      return;
    }

    data?.impactScoreNames?.forEach((v: InsightImpactDto) => {
      initialColumns.push(
        {
          flex: 1,
          field: v.fieldId!,
          headerName: v.name,
          minWidth: 160,
          type: 'number',
          renderCell: (params: any) => {
            const score = params.row!.impactScores?.find((v: any) => v.fieldId == params.field);
            if (score !== undefined) {
              return `${score?.score?.toFixed(2)} ${score?.unit ?? ''}`;
            }
            return '';
          },
          filterOperators: doubleOperators()
        });
    });

    setColumnState(initialColumns);
    setColumnVisibilityModel(columnVisibilityModel);

  }, [data]);

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
}

export default InsightsTable3;
