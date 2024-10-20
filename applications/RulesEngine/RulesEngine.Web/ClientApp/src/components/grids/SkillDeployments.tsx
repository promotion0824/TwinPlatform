import { Alert, Box, Checkbox, Divider, Grid, Snackbar, Stack, Typography } from '@mui/material';
import { DataGridPro, DataGridProProps, GridApi, GridColDef, GridColumnVisibilityModel, GridEditSingleSelectCell, GridEditSingleSelectCellProps, GridFilterModel, GridPaginationModel, GridRowId, GridSortDirection, GridSortModel, GridToolbarColumnsButton, GridToolbarContainer, GridToolbarFilterButton, useGridApiContext } from '@mui/x-data-grid-pro';
import { Icon, IconButton, Tooltip, Button, useDisclosure, Drawer } from '@willowinc/ui';
import { Suspense, useCallback, useEffect, useState } from 'react';
import { useQuery, useQueryClient } from 'react-query';
import useApi from '../../hooks/useApi';
import useLocalStorage from '../../hooks/useLocalStorage';
import { BatchRequestDto, RuleInstanceDto, RuleInstanceStatus } from '../../Rules';
import { VisibleIf } from '../auth/Can';
import Comments from '../Comments';
import { ExportToCsv } from '../ExportToCsv';
import { DateFormatter, RuleInstanceLink, RuleLinkFormatter, TwinLinkFormatterById, ValidFormatter } from '../LinkFormatters';
import { RuleInstanceBooleanFilter, RuleInstanceReviewStatusLookup, RuleInstanceStatusLookup } from '../Lookups';
import RuleInstanceBindings from '../RuleInstanceBindings';
import { RuleInstanceStatusLegend } from '../RuleInstanceStatus';
import SkillDeploymentBulkEditor from '../SkillDeploymentBulkEditor';
import { FormatLocations } from '../StringOptions';
import { boolOperators, buildCacheKey, gridPageSizes, mapFilterSpecifications, mapSortSpecifications, numberOperators, singleSelectOperators, stringOperators } from './GridFunctions';

interface ISkillDeploymentProps {
  ruleId: string,
  pageId: string,
  showRuleColumnsByDefault?: boolean,
  actions?: (ruleInstance: RuleInstanceDto) => React.ReactNode
}

const SkillDeployments = (query: ISkillDeploymentProps) => {

  const ruleId = query.ruleId;
  const actions = query.actions;
  const showRuleColumnsByDefault = query.showRuleColumnsByDefault ?? false;

  const filterKey = buildCacheKey(`${query.pageId}_${ruleId}_SkillDeployment_FilterModel`);
  const sortKey = buildCacheKey(`${query.pageId}_${ruleId}_SkillDeployment_SortModel`);
  const colsKey = buildCacheKey(`${query.pageId}_${ruleId}_SkillDeployment_ColumnModel`);
  const paginationKey = buildCacheKey(`${query.pageId}_SkillDeployment_PaginationModel`);

  const [filters, setFilters] = useLocalStorage<GridFilterModel>(filterKey, { items: [] });
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, [{ field: 'Status', sort: 'desc' }]);
  const [columnVisibilityModel, setColumnVisibilityModel] = useLocalStorage<GridColumnVisibilityModel>(colsKey, showRuleColumnsByDefault ? { TotalComments: false, TriggerCount: false, RuleId: false } : { RuleId: false, RuleName: false });
  const [paginationModel, setPaginationModel] = useLocalStorage<GridPaginationModel>(paginationKey, { pageSize: 10, page: 0 });
  const sortingOrder: GridSortDirection[] = ['desc', 'asc'];

  const apiclient = useApi();
  const queryClient = useQueryClient();

  const [enabledActioned, setEnabledActioned] = useState(false);
  const handleCloseAlert = () => { setEnabledActioned(false); }

  const createRequest = function () {
    const request = new BatchRequestDto();
    request.filterSpecifications = mapFilterSpecifications(filters);
    request.sortSpecifications = mapSortSpecifications(sortModel);
    request.page = paginationModel.page + 1;//MUI grid is zero based
    request.pageSize = paginationModel.pageSize;
    return request;
  }

  const fetchRuleInstances = async () => {
    const request = createRequest();
    return apiclient.getInstancesAfter(ruleId, request);
  };

  const csvExport = {
    downloadCsv: (request: BatchRequestDto) => {
      return apiclient.exportRuleInstancesAfter(ruleId, request);
    },
    createBatchRequest: () => {
      const request = createRequest();
      request.sortSpecifications = [];
      return request;
    },
  };

  const zipExport = {
    downloadCsv: (request: BatchRequestDto) => {
      return apiclient.downloadRuleInstances(ruleId, request);
    },
    createBatchRequest: () => {
      const request = createRequest();
      request.sortSpecifications = [];
      return request;
    },
    downloadType: "zip"
  };

  const {
    isLoading,
    data,
    isFetching,
    isError,
    refetch
  } = useQuery(['ruleInstances', query.ruleId, paginationModel, sortModel, filters], async () => await fetchRuleInstances(), { keepPreviousData: true })

  const handleRefresh = () => {
    queryClient.invalidateQueries(['calculatedPoints', 'ruleInstances'], { exact: true });
    refetch();
  };

  const checkChanged = useCallback(async (api: GridApi, row: RuleInstanceDto, checked: boolean) => {
    await apiclient.enableRuleInstance(row.id, !checked);

    if (row.isCalculatedPointTwin) {
      setEnabledActioned(true);
    }

    api.updateRows([{ id: row.id, disabled: !checked } as any]);
    api.forceUpdate();
  }, []);


  function EditRelationshipCell(props: GridEditSingleSelectCellProps) {
    const apiRef = useGridApiContext();
    const handleValueChange = async (_: any, v: any) => {
      await apiclient.updateRuleInstanceReviewStatus(props.row.id, v);

      apiRef.current.updateRows([{
        id: props.row.id,
        reviewStatus: v,
      } as any]);

    };
    return <GridEditSingleSelectCell onValueChange={handleValueChange} {...props} />;
  }

  function DetailPanelContent(row: any) {
    const ruleInstance = row.row as RuleInstanceDto;
    const isValid = (ruleInstance.status! & RuleInstanceStatus._1) == RuleInstanceStatus._1;
    const apiRef = useGridApiContext();

    //const tagsEditorProps = {
    //  id: "ri_Tags",
    //  key: "ri_TagsKey",
    //  queryKey: "ri_TagsQuery",
    //  defaultValue: ruleInstance.tags,
    //  allowFreeText: false,
    //  queryFn: async (_: any): Promise<string[]> => {
    //    try {
    //      const tags = await apiclient.ruleInstanceTags();
    //      return tags;
    //    } catch (error) {
    //      return [];
    //    }
    //  },
    //  valueChanged: async (newValue: string[]) => {
    //    if (ruleInstance.tags != newValue) {
    //      ruleInstance.tags = newValue;
    //      await apiclient.updateRuleInstanceTags(ruleInstance.id, newValue);
    //      apiRef.current.updateRows([{
    //        id: ruleInstance.id,
    //        tags: newValue,
    //      } as any]);
    //    }
    //  }
    //};

    return (
      <Stack spacing={2} p={2}>

        {actions && <Box>{actions(ruleInstance)}</Box>}

        <RuleInstanceBindings ruleInstance={ruleInstance} pageId={query.pageId} showInvalidOnly={!isValid} showBindingsToggle={!isValid} actions={actions} />

        {!isValid &&
          <Grid container>
            <Grid item xs={6}>
              <Stack mt={1}>
                {/*disabled for now<TagsEditor {...tagsEditorProps} />*/}

                <Comments id={ruleInstance.id!} comments={ruleInstance.comments!} commentAdded={(comment) => {
                  const comments = [...ruleInstance.comments!, comment];
                  apiRef.current.updateRows([{
                    id: ruleInstance.id,
                    comments: comments,
                    totalComments: comments.length,
                    lastCommentPosted: comment.created
                  } as any]);
                }} />
              </Stack>
            </Grid>
          </Grid>}
      </Stack>
    );
  }

  //const fetchTags = useQuery(["ruleInstanceTags"], async () => {
  //  try {
  //    return await apiclient.ruleInstanceTags();
  //  } catch (error) {
  //    return []; // Return empty array in case of error
  //  }
  //});

  const initialColumns: GridColDef[] =
    [
      {
        field: 'Status', headerName: 'Status', width: 100, type: "singleSelect", cellClassName: "MuiDataGrid-cell--textCenter",
        valueOptions: () => { return RuleInstanceStatusLookup.GetStatusFilter(); },
        filterOperators: singleSelectOperators(),
        renderCell: (p: any) => { return ValidFormatter(p.row); }
      },
      {
        field: 'Disabled', headerName: 'Enabled', width: 100, type: "singleSelect",
        valueOptions: () => { return RuleInstanceBooleanFilter.GetInvertedBooleanFilter(); },
        filterOperators: boolOperators(),
        renderCell: (params: any) => {
          return (<Checkbox
            color="primary"
            checked={!(params.row!.disabled)}
            onChange={(_x, b: boolean) => checkChanged(params.api, params.row, b)}
          />);
        }
      },
      {
        field: 'ReviewStatus', headerName: 'Review Status', width: 150,
        type: 'singleSelect',
        valueOptions: (_: any) => RuleInstanceReviewStatusLookup.GetStatusFilter(),
        valueGetter: (params: any) => {
          return params.row!.reviewStatus;
        },
        renderEditCell: (params) => <EditRelationshipCell {...params} />,
        editable: true
      },
      {
        field: 'EquipmentId', headerName: 'Id', flex: 1, minWidth: 200, filterOperators: stringOperators(),
        renderCell: (params: any) => { return RuleInstanceLink(params.row!); },
      },
      {
        field: 'EquipmentName', headerName: 'Equipment', flex: 1.5, minWidth: 200, valueGetter: (params: any) => params.row.equipmentName,
        renderCell: (params: any) => {
          return TwinLinkFormatterById(params.row.equipmentId, params.row.equipmentName);
        },
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
        field: 'CapabilityCount', headerName: 'Capabilities', width: 120,
        renderCell: (params: any) => { return params.row!.capabilityCount; },
        filterOperators: numberOperators(),
      },
      //{
      //  field: 'Tags', headerName: 'Tags', flex: 1, minWidth: 200, type: "singleSelect",
      //  valueOptions: () => {
      //    if (fetchTags.isLoading || fetchTags.isError) {
      //      return []; // return empty array or loading indicator if data is not yet available
      //    }
      //    return fetchTags.data || []; // return the tags data if available
      //  },
      //  valueGetter: (params: any) => params.row.tags,
      //  filterOperators: singleSelectCollectionOperators(),
      //  valueFormatter: (params: any) => {
      //    const tagsArray = params.value as string[];
      //    return tagsArray?.join(", ");
      //  }
      //},
      {
        field: 'LastCommentPosted', headerName: 'Last Post', width: 150, filterable: false,
        renderCell: (params: any) => { return DateFormatter(params.row!.lastCommentPosted); },
      },
      {
        field: 'TotalComments', headerName: 'Comments', width: 120,
        renderCell: (params: any) => { return params.row!.totalComments; },
        filterOperators: numberOperators(),
      },
      {
        field: 'TriggerCount', headerName: 'Received', width: 120, filterable: false,
        renderCell: (params: any) => { return params.row!.triggerCount; },
        filterOperators: stringOperators(),
      },
      {
        field: 'RuleId', headerName: 'Skill Id', flex: 1, minWidth: 200, filterOperators: stringOperators(), valueGetter: (params: any) => params.row.ruleId
      },
      {
        field: 'RuleName', headerName: 'Skill', flex: 1, minWidth: 200, filterOperators: stringOperators(),
        renderCell: (params: any) => { return RuleLinkFormatter(params.row!.ruleId, params.row!.ruleName, false); },
      },
    ];

  const [columns, setColumnState] = useState(initialColumns);
  useEffect(() => {
    if (!data?.items?.some((ri) => ri.isCalculatedPointTwin)) {
      initialColumns.push(
        {
          field: 'RuleDependencyCount', headerName: 'Dependencies', width: 120,
          renderCell: (params: any) => { return params.row!.ruleDependencyCount; },
          filterOperators: numberOperators(),
        }
      );

      setColumnState(initialColumns);
      setColumnVisibilityModel(columnVisibilityModel);
    }
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
  }, [data, data?.total, setRowCountState]);

  const getDetailPanelContent = useCallback<NonNullable<DataGridProProps['getDetailPanelContent']>>(({ row }) => <DetailPanelContent row={row} />, []);

  const [selectedRows, setSelectedRows] = useState<GridRowId[]>([]);
  const clearSelectedRows = () => {
    setSelectedRows([]);
  };

  const [editPropsDrawerOpened, { open: editPropsDrawerOpen, close: editPropsDrawerClose }] = useDisclosure(false);
  const getDeployments = (): string[] => {
    if (!data || !data.items) {
      return [];
    }

    //Filter the items to include only those whose id is in selectedRows
    return data.items
      .filter(item => selectedRows.includes(item.id!))
      .map(item => item.id as string);
  }

  return (
    <>
      <Stack>
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
          checkboxSelection
          disableRowSelectionOnClick
          onRowSelectionModelChange={(ids) => { setSelectedRows(ids as GridRowId[]); }}
          rowSelectionModel={selectedRows}
          getDetailPanelHeight={() => 'auto'}
          getDetailPanelContent={getDetailPanelContent}
          onSortModelChange={handleSortModelChange}
          onFilterModelChange={handleFilterChange}
          sortingOrder={sortingOrder}
          columnVisibilityModel={columnVisibilityModel}
          onColumnVisibilityModelChange={(newModel: any) => setColumnVisibilityModel(newModel)}
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
                <GridToolbarColumnsButton />
                <GridToolbarFilterButton />
                <ExportToCsv source={csvExport} />
                <VisibleIf canExportRules>
                  <ExportToCsv source={zipExport} />
                </VisibleIf>
                <Box sx={{ flexGrow: 1 }} />
                <Button kind="secondary" prefix={<Icon icon="edit_note" />}
                  disabled={selectedRows.length < 1} onClick={editPropsDrawerOpen}>Edit selected</Button>
                <Divider orientation="vertical" variant="middle" flexItem />
                <IconButton kind="secondary" onClick={handleRefresh} >
                  <Icon icon="refresh" />
                </IconButton>
              </GridToolbarContainer>
            ),
            noRowsOverlay: () => (
              <Stack margin={2}>
                {isError ? 'An error occured...' : 'No rows to display.'}
              </Stack>
            )
          }}
        />
        <RuleInstanceStatusLegend />
      </Stack >

      <Drawer opened={editPropsDrawerOpened} onClose={editPropsDrawerClose} withOverlay={false} header="Edit Properties" size="lg" lockScroll={false} >
        <SkillDeploymentBulkEditor deployments={getDeployments()} onPropertiesChanged={() => { editPropsDrawerClose(); clearSelectedRows(); handleRefresh(); }}></SkillDeploymentBulkEditor>
      </Drawer >

      <Suspense fallback={<div>Loading...</div>}>
        <Snackbar open={enabledActioned} onClose={handleCloseAlert} autoHideDuration={10000} >
          <Alert onClose={handleCloseAlert} sx={{ width: '100%' }} variant="filled" severity='warning'>
            <Typography variant='body1' color='white'>Please regenerate skill to process calculated points.</Typography>
          </Alert>
        </Snackbar>
      </Suspense>
    </>
  );
}

export default SkillDeployments;
