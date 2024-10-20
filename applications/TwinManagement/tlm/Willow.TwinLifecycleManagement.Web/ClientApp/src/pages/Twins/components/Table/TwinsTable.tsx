import { useMemo, useState, useEffect } from 'react';
import { useTwins } from '../../TwinsProvider';
import useOntology from '../../../../hooks/useOntology/useOntology';
import useLocations from '../../../../hooks/useLocations';
import { Box, styled } from '@mui/material';
import {
  GridToolbarContainer,
  GridToolbarColumnsButton,
  GridToolbarFilterButton,
  GridToolbarExport,
  GRID_DETAIL_PANEL_TOGGLE_COL_DEF,
  GridFilterModel,
  GridColumnVisibilityModel,
  GridColDef,
  GridRenderCellParams,
} from '@mui/x-data-grid-pro';
import { usePersistentGridState } from '../../../../hooks/usePersistentGridState';
import TwinsTablePanelContent from './TwinsTablePanelContent';
import { SourceType } from '../../../../services/Clients';
import CustomDetailPanelToggle from '../../../../components/Grid/CustomDetailPanelToggle';
import { mapFilterSpecifications, stringOperators, guidOperators } from '../../../../components/Grid/GridFunctions';
import { StyledDataGrid } from '../../../../components/Grid/StyledDataGrid';

/**
 * Table for displaying twins
 */
export default function TwinsTable() {
  const { apiRef, getTwinsQuery, selectedRows, setSelectedRows, deleteTwinsMutation } = useTwins();

  const { data: ontology, isLoading: isOntologyLoading, isSuccess: isOntologySuccess } = useOntology();
  const { data: locations, isLoading: isLocationsLoading, isSuccess: isLocationsSuccess } = useLocations();
  const isDependenciesLoaded = isLocationsSuccess && isOntologySuccess;

  const { savedState } = usePersistentGridState(apiRef, 'twins', isDependenciesLoaded);
  const {
    query,
    setContinuationToken,
    pageSize: twinsPageSize,
    setPageSize,
    filtersStates,
    sourceType,
  } = getTwinsQuery;

  const columns: GridColDef[] = [
    { field: '__check__', hideable: false, sortable: false, filterable: false, width: 50 },
    {
      field: 'name',
      headerName: 'Name',
      filterOperators: stringOperators(),
      flex: 0.75,
      valueGetter: (params: any) => params.row.twin?.name || '',
    },
    {
      field: 'id',
      headerName: 'Id',
      filterOperators: stringOperators(),
      flex: 0.9,
      valueGetter: (params: any) => params.row.twin?.$dtId,
    },
    {
      field: 'model',
      headerName: 'Model',
      filterable: false,
      flex: 0.75,
      valueGetter: (params: any) =>
        ontology?.getModelById(params.row.twin?.$metadata?.$model)?.name ||
        `${params.row.twin?.$metadata?.$model || ''} (Model not defined)`,

      renderCell: (param: GridRenderCellParams) => <StyledBox title={param.value}>{param.value}</StyledBox>,
    },
    {
      field: 'siteName',
      headerName: 'Site Name',
      flex: 0.75,
      filterable: false,
      valueGetter: (params: any) =>
        locations?.getLocationById(params.row.twin?.siteID)?.twin?.name || `(No top-level twin for site)`,

      renderCell: (param: GridRenderCellParams) => <StyledBox title={param.value}>{param.value}</StyledBox>,
    },
    {
      field: 'siteId',
      headerName: 'Site Id',
      flex: 1,
      filterOperators: sourceType === SourceType.AdtQuery ? stringOperators() : guidOperators(),
      valueGetter: (params: any) => params.row.twin?.siteID || '',

      renderCell: (param: GridRenderCellParams) => <StyledBox title={param.value}>{param.value}</StyledBox>,
    },
    {
      field: 'uniqueID',
      headerName: 'Unique Id',
      flex: 1,
      filterOperators: sourceType === SourceType.AdtQuery ? stringOperators() : guidOperators(),
      valueGetter: (params: any) => params.row.twin?.uniqueID || ``,

      renderCell: (param: GridRenderCellParams) => <StyledBox title={param.value}>{param.value}</StyledBox>,
    },
    {
      field: 'externalID',
      headerName: 'External Id',
      flex: 1,
      filterOperators: stringOperators(),
      valueGetter: (params: any) => params.row.twin?.externalID || '',
      renderCell: (param: GridRenderCellParams) => <StyledBox title={param.value}>{param.value}</StyledBox>,
    },
    {
      ...GRID_DETAIL_PANEL_TOGGLE_COL_DEF,
      hideable: false,
      renderCell: (params: any) => <CustomDetailPanelToggle id={params.id} value={params.value} />,
    },
  ];

  const {
    selectedOrphanState: [selectedOrphan],
    selectedLocationState: [selectedLocation],
    selectedModelState: [selectedModels],
    searchTextState: [searchText],
    filterSpecificationsState,
  } = filtersStates;
  const { data, isLoading: isQueryLoading, isFetching, isSuccess } = query;
  const twinsData = useMemo(() => data?.content || [], [data]);

  const [rowCount, setRowCount] = useState<number>(0);

  let parsedContinuationToken = data?.continuationToken && JSON.parse(data.continuationToken);

  const { Total } = parsedContinuationToken || {};

  const [page, setPage] = useState<number>(0);

  // Set table's total row count
  useEffect(() => {
    setRowCount((prevRowCount: number) => {
      // case for when pagination query have only one page
      if (page === 0 && !data?.continuationToken) {
        return twinsData.length;
      }
      return Total ? Total : prevRowCount ? prevRowCount : twinsData.length;
    });
  }, [Total, setPage, twinsData, data, page]);

  const { mutateDeleteTwins } = deleteTwinsMutation;
  const { isLoading: isDeleteTwinsLoading } = mutateDeleteTwins;

  // Modify twins query's continuation token when page is changed
  useEffect(() => {
    // todo: abort previous requests when new page is selected.
    if (!!parsedContinuationToken) {
      parsedContinuationToken.NextPage = page;
      setContinuationToken(JSON.stringify(parsedContinuationToken));
    } else if (isSuccess) {
      // case when we're on the last page, endpoint does not return continuationToken, so use previous state to get the previous page
      setContinuationToken((prevState: string) => {
        let parsedContinuationToken = prevState && JSON.parse(prevState);
        // there are times when there is no twins, parsedContinuationToken is empty string - it will passed as null to the endpoint
        if (typeof parsedContinuationToken === 'object') parsedContinuationToken.NextPage = page;
        return JSON.stringify(parsedContinuationToken);
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page]);

  // Reset page when filters is changed
  useEffect(() => {
    setPage(0);
  }, [selectedOrphan, selectedModels, selectedLocation, twinsPageSize, searchText, setPage]);

  // Default column visibility settings. Display/hide columns fields.
  const [columnVisibilityModel, setColumnVisibilityModel] = useState<GridColumnVisibilityModel>({
    name: true,
    id: true,
    model: true,
    siteName: true,
    siteId: true,
    uniqueID: true,
    externalID: false,
    GRID_DETAIL_PANEL_TOGGLE_COL_DEF: true,
  });

  const { columnVisibilityModel: columnVisibilityModelState } = savedState?.columns || {};
  const {
    name = true,
    id = true,
    model = true,
    siteName = true,
    siteId = true,
    uniqueID = true,
    externalID = false,
  } = columnVisibilityModelState || {};

  useEffect(() => {
    setColumnVisibilityModel({
      name,
      id,
      model,
      siteName,
      siteId,
      uniqueID,
      externalID,
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const isGridLoading = isQueryLoading || isFetching;

  return (
    <>
      <StyledDataGrid
        apiRef={apiRef}
        initialState={savedState}
        rows={twinsData || []}
        getRowId={(row) => row.twin?.$dtId!}
        getDetailPanelContent={({ row }) => <TwinsTablePanelContent row={row} />}
        getDetailPanelHeight={() => 'auto'}
        columns={columns}
        slots={{ toolbar: CustomToolBar }}
        loading={isGridLoading}
        rowCount={isDeleteTwinsLoading ? 0 : sourceType === SourceType.AdtQuery && !Total ? Number.MAX_VALUE : rowCount} // disable next/prev page buttons when deleting twins
        localeText={{
          MuiTablePagination: {
            labelDisplayedRows: ({ from, to, count }) =>
              `${from} - ${to} of ${count === Number.MAX_VALUE ? '?' : count}`,
          },
        }}
        isRowSelectable={() => !isDeleteTwinsLoading} // disable row selection when deleting twins
        checkboxSelection
        onRowSelectionModelChange={(ids) => {
          setSelectedRows(ids);
        }}
        rowSelectionModel={selectedRows}
        onFilterModelChange={(newModel: GridFilterModel) => {
          filterSpecificationsState[1](mapFilterSpecifications(newModel));
          setPage(0);
          setSelectedRows([]);
        }}
        pagination
        paginationMode="server"
        pageSizeOptions={[100, 250, 1000]}
        paginationModel={{ page, pageSize: twinsPageSize }}
        onPaginationModelChange={(pageModel) => {
          let { page, pageSize } = pageModel;
          setPageSize(pageSize);
          setPage(page);
        }}
        columnVisibilityModel={columnVisibilityModel}
        onColumnVisibilityModelChange={(newModel) => setColumnVisibilityModel(newModel)}
      />
    </>
  );
}

function CustomToolBar() {
  return (
    <Flex>
      <StyledToolBarContainer>
        <GridToolbarColumnsButton />
        <GridToolbarFilterButton />
        <GridToolbarExport />
      </StyledToolBarContainer>
    </Flex>
  );
}

const Flex = styled('div')({
  display: 'flex',
  flexDirection: 'row',
  width: '100%',
  justifyContent: 'space-between',
  '&:last-child': { padding: '0 10px' },
});
const StyledToolBarContainer = styled(GridToolbarContainer)({ gap: 10, flexWrap: 'nowrap', padding: '4px 3px' });

const StyledBox = styled(Box)({
  overflowWrap: 'anywhere',
  width: '100%',
  textOverflow: 'ellipsis',
  display: 'inline-block',
  overflow: 'hidden',
});
