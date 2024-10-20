import { Badge, Box, Chip, Tooltip } from '@mui/material';
import { Avatar, GridColDef, GridFilterModel, GridPaginationModel, GridRenderCellParams, GridRowSelectionModel, GridSortModel, GridValueGetterParams, Icon, Loader } from '@willowinc/ui';
import { Dispatch, SetStateAction, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { AppIcons } from '../../AppIcons';
import { CustomDataGrid } from '../../Components/CustomDataGrid';
import ExpressionWithStatusIndicator from '../../Components/ExpressionStatusIndicator';
import { AssignmentModel } from '../../types/AssignmentModel';
import { FlatSelectTreeModel, FlattenSelectTree, ILocationTwinModel, UnFormatExpressionIntoValues } from '../../types/SelectTreeModel';
import { BatchRequestDto, FilterSpecificationDto, SortSpecificationDto } from '../../types/BatchRequestDto';
import { BatchDto } from '../../types/BatchDto';

function AssignmentTable(
  {
    selectionModel,
    setSelectionModel,
    batch,
    isFetching,
    batchRequest,
    setBatchRequest,
    locationResponse
  }:
    {
      selectionModel: GridRowSelectionModel,
      setSelectionModel: Dispatch<SetStateAction<GridRowSelectionModel>>,
      batch: BatchDto<AssignmentModel> | undefined,
      isFetching: boolean,
      batchRequest: BatchRequestDto,
      setBatchRequest: Dispatch<SetStateAction<BatchRequestDto>>,
      locationResponse: { locations: ILocationTwinModel[], loaded: boolean }
    }) {

  const flatLocations = useMemo(() => {
    return FlattenSelectTree(locationResponse.locations, "");
  }, [locationResponse]);

  const parseExp = (exp: string): FlatSelectTreeModel[] => {
    if (!exp) {
      return [];
    }
    const matches = UnFormatExpressionIntoValues(exp);
    return flatLocations.filter(f => matches.findIndex(x => x === f.id) > -1)
  };

  const columns: GridColDef[] = useMemo(() => [
    {
      field: 'avatar',
      headerName: '',
      description: 'This column displays assignment avatar',
      sortable: false,
      filterable: false,
      align: 'center',
      flex: 0.5,
      renderCell: (params) => {
        return (
          <Badge overlap="circular" anchorOrigin={{ vertical: 'top', horizontal: 'right' }}
            badgeContent={
              <Icon size={16} icon="assignment" />
            }
          >
            <Avatar size="lg">
              {params.row.type === 'U' ? AppIcons.UserIcons : AppIcons.GroupIcon}
            </Avatar>
          </Badge>
        );
      }
    },
    {
      field: 'UserOrGroupName', headerName: 'User or Group', flex: 1, description: 'Assigned User / Group', sortable: false,
      valueGetter: (params: GridValueGetterParams) =>
        params.row.type === 'U' ? params.row.user.firstName + ' ' + params.row.user.lastName : params.row.group.name,
      renderCell: (params) => {
        return (
          params.row.type === 'U' ?
            <Link className="tableLink" to={'/users/' + encodeURIComponent(params.row.user.email)} >{params.value}</Link>
            :
            <Link className="tableLink" to={'/groups/' + encodeURIComponent(params.row.group.name) + '/'} >{params.value}</Link>
        );
      }
    },
    {
      field: 'Role.Name',
      headerName: 'Role',
      description: 'Assigned Role',
      sortable: true,
      flex: 1,
      renderCell: (params) => {
        return (
          <Link className="tableLink" to={'/roles/' + encodeURIComponent(params.row.role.name) + '/'} >{params.row.role.name}</Link>
        );
      }
    },
    {
      field: 'scope', headerName: 'Scope', flex: 2, description: 'Scope', sortable: false,
      renderCell: (params: GridRenderCellParams) =>
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
          {locationResponse.loaded ?
            parseExp(params.row.expression).map((value: any) => (
              <Tooltip key={value.id} title={params.row.expression} >
                <Chip key={value.id} icon={AppIcons.ApartmentIcon} label={value.displayName} variant="outlined" />
              </Tooltip>))
            :
            <Loader size="sm" variant="dots" />
          }
        </Box>,
    },
    {
      field: 'expression', headerName: 'Expression', flex: 1, description: 'Assigned Expression', sortable: false,
      renderCell: (params: GridRenderCellParams) =>
        <Tooltip title={params.row.expression} >
          <span style={{ textOverflow: 'ellipsis', overflow: 'hidden' }}>{params.row.expression}</span>
        </Tooltip>,
    },
    {
      field: 'condition', headerName: 'Condition', flex: 1, description: 'Assigned Condition', sortable: false,
      renderCell: (params: GridRenderCellParams) =>
        <ExpressionWithStatusIndicator expression={params.value} status={params.row.conditionExpressionStatus} />
    },
  ], [locationResponse]);

  const ApplyFilter = (gridFilterModel: GridFilterModel) => {
    setBatchRequest((request) => ({
      ...request,
      filterSpecifications: FilterSpecificationDto.MapFrom(gridFilterModel)
    }));
  };

  const ApplySort = (sortModel: GridSortModel) => {
    setBatchRequest((request) => ({ ...request, sortSpecifications: SortSpecificationDto.MapFrom(sortModel) }));
  };

  const ApplyPagination = (paginationModel: GridPaginationModel) => {
    setBatchRequest((request) => ({ ...request, page: paginationModel.page, pageSize: paginationModel.pageSize }));
  }


  return (
    <CustomDataGrid
      initialState={{ columns: { columnVisibilityModel: { expression: false } } }}
      rows={batch?.items ?? []}
      rowCount={batch?.total ?? 0}
      getRowId={(r) => r.id}
      columns={columns}
      pagination
      paginationModel={{
        pageSize: batchRequest.pageSize,
        page: batchRequest.page,
      }}
      onPaginationModelChange={ApplyPagination}
      pageSizeOptions={[50, 100, 200]}
      onRowSelectionModelChange={(newSelectionModel) => {
        setSelectionModel(newSelectionModel);
      }}
      rowSelectionModel={selectionModel}
      checkboxSelection
      disableColumnMenu={false}
      loading={isFetching}
      paginationMode="server"
      filterMode="server"
      onFilterModelChange={ApplyFilter}
      onSortModelChange={ApplySort}
    />);
}

export default AssignmentTable;  
