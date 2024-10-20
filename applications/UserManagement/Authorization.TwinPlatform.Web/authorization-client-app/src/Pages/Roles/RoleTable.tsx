import { Avatar, GridColDef, GridFilterModel, GridPaginationModel, GridRowSelectionModel, GridSortModel, GridValueGetterParams, Icon } from '@willowinc/ui';
import { Dispatch, SetStateAction, useMemo } from 'react';
import { Link } from "react-router-dom";
import { AppIcons } from '../../AppIcons';
import { CustomChipGroup } from '../../Components/CustomChipGroup';
import { CustomDataGrid } from '../../Components/CustomDataGrid';
import { BatchDto } from '../../types/BatchDto';
import { BatchRequestDto, FilterSpecificationDto, SortSpecificationDto } from '../../types/BatchRequestDto';
import { RoleModel } from '../../types/RoleModel';

function RoleTable({ selectionModel, setSelectionModel, batch, isFetching, batchRequest, setBatchRequest }:
  {
    selectionModel: GridRowSelectionModel,
    setSelectionModel: Dispatch<SetStateAction<GridRowSelectionModel>>,
    batch: BatchDto<RoleModel> | undefined,
    isFetching: boolean,
    batchRequest: BatchRequestDto,
    setBatchRequest: Dispatch<SetStateAction<BatchRequestDto>>
  }) {
  const columns: GridColDef[] = useMemo(() => [
    {
      field: 'avatar',
      headerName: '',
      description: 'This column displays role avatar',
      sortable: false,
      filterable: false,
      align: 'center',
      flex: 0.5,
      renderCell: (params) => {
        return (
          <Avatar size="lg">
            <Icon icon='badge' />
          </Avatar>
        );
      }
    },
    {
      field: 'name', headerName: 'Name', description: 'Name of the Role', flex: 1,
      renderCell: (params) => {
        return (
          <Link className="tableLink" title={params.row.description} to={encodeURIComponent(params.row.name) + '/'} >{params.row.name}</Link>
        );
      }
    },
    {
      field: 'permissions',
      headerName: 'Permissions',
      description: 'Permission Avatar',
      sortable: false,
      filterable: false,
      flex: 2,
      valueGetter: (params: GridValueGetterParams) =>
        params.row.permissions ?? [],
      renderCell: (params) => {
        return (
          <CustomChipGroup key={params.value} data={params.value} icon={AppIcons.PermissionIcon} getName={(rec: any) => { return rec.name; }}></CustomChipGroup>
        )
      }
    }
  ], []);

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

export default RoleTable;  
