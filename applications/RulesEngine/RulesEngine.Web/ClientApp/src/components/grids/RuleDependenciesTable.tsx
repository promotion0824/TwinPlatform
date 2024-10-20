import WarningAmberIcon from '@mui/icons-material/WarningAmber';
import { Checkbox, Stack, Tooltip, Typography } from '@mui/material';
import Box from '@mui/material/Box';
import { DataGridPro, GridColDef, GridColumnVisibilityModel, GridEditSingleSelectCell, GridEditSingleSelectCellProps, gridExpandedSortedRowIdsSelector, GridFooter, GridFooterContainer, GridPaginationModel, GridSortDirection, GridSortModel, GridToolbarColumnsButton, GridToolbarContainer, GridToolbarFilterButton, useGridApiContext } from '@mui/x-data-grid-pro';
import { useEffect, useState } from 'react';
import { useQuery, useQueryClient } from 'react-query';
import useApi from '../../hooks/useApi';
import useLocalStorage from '../../hooks/useLocalStorage';
import { BatchRequestDto, RuleDependencyDto, RuleDependencyListItemDto, RuleDto } from '../../Rules';
import { ExportToCsv } from '../ExportToCsv';
import { ModelFormatter2, RuleLinkFormatter, YesNoFormatter } from '../LinkFormatters';
import { buildCacheKey, gridPageSizes, createCsvFileResponse, numberOperators, stringOperators } from './GridFunctions';

interface IRuleDependenciesTableProps {
  rule: RuleDto,
  revision: number,
  updateDependencies: (dependencies: RuleDependencyDto[]) => void
}

const sortingOrder: GridSortDirection[] = ['desc', 'asc'];

const api = useApi();

const RuleDependenciesTable = (params: IRuleDependenciesTableProps) => {
  const [rule, setRule] = useState(params.rule);
  const sortKey = buildCacheKey(`${rule.id}_RuleDependenciesTable_SortModel`);
  const paginationKey = buildCacheKey(`RuleDependenciesTable_PaginationModel`);
  const colsKey = buildCacheKey(`${rule.id}_RuleDependenciesTable_ColumnModel`);
  const [sortModel, setSortModel] = useLocalStorage<GridSortModel>(sortKey, [{ field: 'enabled', sort: 'desc' }]);
  const [paginationModel, setPaginationModel] = useLocalStorage<GridPaginationModel>(paginationKey, { pageSize: 10, page: 0 });
  const [columnVisibilityModel, setColumnVisibilityModel] = useLocalStorage<GridColumnVisibilityModel>(colsKey,
    { ruleId: false, validInstanceCount: false, ruleInstanceCount: false });
  const revision = params.revision;
  const queryClient = useQueryClient();

  // Whenever params.rule is invalidated, we need to refresh our copy.
  useEffect(() => {
    setRule(params.rule);
  }, [params.rule]);

  const fetchDependencies = async () => {
    //Set the JSON to empty for security reasons
    rule!.json = '';
    var dependencies = await api.getRuleDependencies(rule.id, rule);

    for (var i = rule.dependencies!.length - 1; i >= 0; i--) {
      if (!dependencies.some(v => v.ruleId == rule.dependencies![i].ruleId)) {
        rule.dependencies!.splice(i, 1);
      }
    }

    setRule(rule);

    return dependencies;
  };

  const {
    isLoading,
    data,
    isFetching
  } = useQuery(['getRuleDependencies', rule.id, revision], async () => await fetchDependencies())

  const getExport = () => {
    const apiRef = useGridApiContext();
    return {
      downloadCsv: (_: BatchRequestDto) => {
        const ids = gridExpandedSortedRowIdsSelector(apiRef);

        return createCsvFileResponse(data?.filter(v => ids.indexOf(v.ruleId!) >= 0).map((v) => {
          return {
            ruleId: v.ruleId,
            ruleName: v.ruleName,
            rulePrimaryModelId: v.rulePrimaryModelId,
            ruleCategory: v.ruleCategory,
            isRelated: v.isRelated,
            isReferencedCapability: v.isReferencedCapability,
            isSibling: v.isSibling,
            relationship: v.relationship,
          };
        }), "SkillDependencies.csv");
      },
      createBatchRequest: () => new BatchRequestDto()
    };
  };

  const toggleSelected = (row: RuleDependencyListItemDto, checked: boolean) => {
    const index = findIndex(row);
    if (checked) {
      if (index < 0) {
        const obj: RuleDependencyDto = new RuleDependencyDto();
        obj.init({ ruleId: row.ruleId, relationship: row.relationship });
        rule.dependencies!.push(obj);
        setRule(rule);
      }
    }
    else {
      const index = findIndex(row);
      if (index >= 0) {
        rule.dependencies!.splice(index, 1);
        setRule(rule);
      }
    }

    queryClient.invalidateQueries(['getRuleDependencies', rule.id, params.revision]);
  };

  const updateRelationship = (row: RuleDependencyListItemDto, relationship: string) => {
    const index = findIndex(row);
    if (index >= 0) {
      rule!.dependencies![index].relationship = relationship;
      setRule(rule);
    }
  };

  const isEnabled = (row: RuleDependencyListItemDto) => {
    return findIndex(row) >= 0;
  }

  const findIndex = (row: RuleDependencyListItemDto): number => {
    return rule.dependencies!.findIndex(v => v.ruleId == row.ruleId);
  }

  function EditRelationshipCell(props: GridEditSingleSelectCellProps) {
    const handleValueChange = (_: any, v: any) => {
      updateRelationship(props.row, v);
    };
    return <GridEditSingleSelectCell onValueChange={handleValueChange} {...props} />;
  }

  const columnsParameters: GridColDef[] = [
    {
      field: 'enabled', headerName: 'Enabled', width: 100, cellClassName: "MuiDataGrid-cell--textCenter", type: 'boolean',
      valueGetter: (params: any) => {
        return findIndex(params.row!) >= 0;
      },
      renderCell: (params: any) => {
        return (<><Checkbox
          color="primary"
          checked={isEnabled(params.row!)}
          onChange={(_, b: boolean) => toggleSelected(params.row, b)}
        />
          {params.row.circularDependency &&
            <Tooltip title="Enabling this skill will create a circular dependency">
              <WarningAmberIcon color="warning" />
            </Tooltip>}
        </>);
      },
    },
    {
      field: 'relationship', headerName: 'Relationship', width: 150,
      type: 'singleSelect',
      valueOptions: (params: any) => params.row?.availableRelationships ?? ["isSibling", "isRelated", "isReferencedCapability"],
      valueGetter: (params: any) => {
        const index = findIndex(params.row!);
        if (index >= 0) {
          var relationship = rule!.dependencies![index].relationship;
          return relationship == "isFedBy" ? "isRelated" : relationship;
        }
        return params.row!.relationship;
      },
      renderEditCell: (params) => <EditRelationshipCell {...params} />,
      editable: true
    },
    {
      field: 'distance', headerName: 'Distance', width: 100, filterOperators: numberOperators(), type: "number",
      valueGetter: (params: any) => {
        //0 distance should be sorted below largest distances so that distance of 1 can go to top of asc order
        return params.row!.distance == 0 ? 1000000 : params.row!.distance;
      },
      renderCell: (params: any) => {
        return params.row!.distance == 0 ? "-" : params.row!.distance;
      }
    },
    {
      field: 'ruleName', headerName: 'Skill', minWidth: 500, filterOperators: stringOperators(),
      renderCell: (params: any) => {
        //force reload of the rule, form params aren't refreshing correctly when the single rule page changes from one rule to the next
        return RuleLinkFormatter(params.row!.ruleId, params.row!.ruleName, true);
      }
    },
    {
      field: 'rulePrimaryModelId', headerName: 'Primary Model', flex: 1, maxWidth: 300, filterOperators: stringOperators(),
      renderCell: (params: any) => {
        return ModelFormatter2({ modelId: params.row!.rulePrimaryModelId });
      }
    },
    {
      field: 'ruleCategory', headerName: 'Category', flex: 1, maxWidth: 200, filterOperators: stringOperators()
    },
    {
      field: 'isRelated', headerName: 'Related', minWidth: 50, type: 'boolean',
      renderCell: (params: any) => { return YesNoFormatter(params.row!.isRelated) }
    },
    {
      field: 'isReferencedCapability', headerName: 'Bound To Referenced Capability', minWidth: 50, type: 'boolean',
      renderCell: (params: any) => { return YesNoFormatter(params.row!.isReferencedCapability) }
    },
    {
      field: 'isSibling', headerName: 'Sibling', minWidth: 50, type: 'boolean',
      renderCell: (params: any) => { return YesNoFormatter(params.row!.isSibling) }
    },
    {
      field: 'ruleId', headerName: 'Id', filterOperators: stringOperators()
    },
    {
      field: 'ruleInstanceCount', headerName: 'Instances', filterOperators: numberOperators()
    },
    {
      field: 'validInstanceCount', headerName: 'Valid Instances', filterOperators: numberOperators()
    }
  ];

  return (
    <Box sx={{ flex: 1, width: '100%' }}>
      <DataGridPro
        autoHeight
        rows={(data ?? []).map((x: RuleDependencyListItemDto) => ({ ...x, id: x.ruleId, enabled: isEnabled(x) }))}
        loading={isLoading || isFetching}
        pageSizeOptions={gridPageSizes()}
        columns={columnsParameters}
        pagination
        paginationModel={paginationModel}
        onPaginationModelChange={setPaginationModel}
        onSortModelChange={setSortModel}
        sortingOrder={sortingOrder}
        columnVisibilityModel={columnVisibilityModel}
        onColumnVisibilityModelChange={(newModel: any) => setColumnVisibilityModel(newModel)}
        hideFooterSelectedRowCount
        initialState={{
          sorting: {
            sortModel: [...sortModel]
          }
        }}
        slots={{
          footer: () => (
            <GridFooterContainer>
              {data && <Typography component="p" sx={{ paddingLeft: 1 }}>{rule.dependencies!.length} selected of {(data ?? []).length} possible dependencies</Typography>}
              <GridFooter sx={{
                border: 'none', // To delete double border.
              }} />
            </GridFooterContainer>
          ),
          toolbar: () => (
            <GridToolbarContainer>
              <Box sx={{ display: 'flex', flexGrow: 1, gap: 2 }}>
                <GridToolbarColumnsButton />
                <GridToolbarFilterButton />
                <ExportToCsv source={getExport()} />
              </Box>
            </GridToolbarContainer>
          ),
          noRowsOverlay: () => (
            <Stack margin={2}>
              {'No skills found'}
            </Stack>
          ),
        }}
      />
    </Box>);
};

export default RuleDependenciesTable;
