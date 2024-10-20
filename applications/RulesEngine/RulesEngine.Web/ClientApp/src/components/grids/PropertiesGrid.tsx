import { Box, Stack } from '@mui/material';
import { DataGridPro, GridPaginationModel, GridSortDirection, GridSortModel, GridToolbarContainer, GridToolbarFilterButton } from '@mui/x-data-grid-pro';
import { useQuery } from 'react-query';
import useApi from '../../hooks/useApi';
import useLocalStorage from '../../hooks/useLocalStorage';
import { BatchRequestDto } from '../../Rules';
import { ExportToCsv } from '../ExportToCsv';
import { ModelFormatter2 } from '../LinkFormatters';
import { buildCacheKey, createCsvFileResponse, gridPageSizes } from './GridFunctions';

export interface IPropertyDto {
  propertyName: string,
  propertyType?: string,
  propertyValue?: string,
  propertyLookupKey?: string,
  modelId?: string,
  declaredCount?: number,
  usedCount?: number,
}

const sortingOrder: GridSortDirection[] = ['desc', 'asc'];

const PropertiesGrid = (params: {
  properties: IPropertyDto[],
  pageId: string,
  showValue?: boolean,
  showType?: boolean,
  showModelId?: boolean
}) => {

  const properties = params.properties;
  const showValue = params.showValue ?? false;
  const showType = params.showType ?? false;
  const showModelId = params.showModelId ?? false;

  const apiclient = useApi();
  const sortKey = buildCacheKey(`${params.pageId}_Properties_SortModel`);
  const paginationKey = buildCacheKey(`${params.pageId}_Properties_PaginationModel`);
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, []);
  const [paginationModel, setPaginationModel] = useLocalStorage<GridPaginationModel>(paginationKey, { pageSize: 500, page: 0 });

  const summary = useQuery("summary", async () => {
    const data = await apiclient.systemSummary();
    return data;
  });

  const columnsParameters = [
    {
      field: 'propertyName', headerName: 'Property', width: 250
    }
  ];

  if (showValue) {
    columnsParameters.push({
      field: 'propertyValue', headerName: 'Value', width: 250
    });
  }

  if (showType) {
    columnsParameters.push({
      field: 'propertyType', headerName: 'Type', width: 250,
      renderCell: (params: any) => (params.row!.propertyType!.startsWith("dtmi") ? <ModelFormatter2 modelId={params.row!.propertyType!} /> : <>{params.row!.propertyType}</>)
    } as any);
  }

  if (showModelId) {
    columnsParameters.push({
      field: 'modelId', headerName: 'Model', width: 250,
      renderCell: (params: any) => <ModelFormatter2 modelId={params.row!.modelId!} />
    } as any);
  }

  columnsParameters.push({
    field: 'usage', headerName: 'Property Used/Total', width: 250, filterable: false, sortable: false,
    renderCell: (params: any) => {
      if (params.row!.declaredCount) {
        return (<>{params.row!.usedCount} / {params.row!.declaredCount}</>); 
      }

      return (<></>);
    }
  } as any);

  const handleSortModelChange = (newModel: GridSortModel) => {
    setSortModel(newModel);
  };

   const csvExport = {
    downloadCsv: (_: BatchRequestDto) => {
       return createCsvFileResponse(properties.map((v) => {

         var result: any = {};

         columnsParameters.forEach(c => {
           if (c.field == "usage") {
             result.usage = v.declaredCount ? `${v.usedCount} out of ${v.declaredCount}` : "";
           }
           else {
             const value = Reflect.get(v, c.field);
             Reflect.set(result, c.field, value === undefined ? "" : value);
           }           
         });

         return result;
      }), "Properties.csv");
    },
    createBatchRequest: () => new BatchRequestDto()
  };

  if (!summary.data) {
    return (<>Loading properties...</>)
  }

  if (summary.data) {
    properties.forEach(p => {
      const modelSummary = summary.data!.modelSummary!.find(v => v.modelId == p.modelId);

      if (modelSummary) {
        const propertyName = p.propertyLookupKey ?? p.propertyName!;

        if (modelSummary.propertiesDelared!.hasOwnProperty(propertyName)) {
          p.declaredCount = modelSummary.propertiesDelared![propertyName];
          p.usedCount = 0;

          if (modelSummary.propertiesUsed!.hasOwnProperty(propertyName)) {
            p.usedCount = modelSummary.propertiesUsed![propertyName];
          }
        }
      }
    });
  }

  return (
    <Box sx={{ flex: 1 }}>
      <DataGridPro
        autoHeight
        rows={properties.map((x: IPropertyDto) => ({ ...x, id: x.propertyName }))}
        rowCount={properties.length}
        pageSizeOptions={gridPageSizes()}
        columns={columnsParameters}
        pagination
        paginationModel={paginationModel}
        onPaginationModelChange={setPaginationModel}
        onSortModelChange={handleSortModelChange}
        sortingOrder={sortingOrder}
        hideFooterSelectedRowCount
        initialState={{
          sorting: {
            sortModel: [...sortModel]
          }
        }}
        slots={{
          toolbar: () => (
            <GridToolbarContainer>
              <Box sx={{ display: 'flex', flexGrow: 1, gap: 2 }}>
                <GridToolbarFilterButton />
                <ExportToCsv source={csvExport} />
              </Box>
            </GridToolbarContainer>
          ),
          noRowsOverlay: () => (
            <Stack margin={2}>
              {'No properties to display'}
            </Stack>
          ),
        }}
      />
    </Box>);
};

export default PropertiesGrid;
