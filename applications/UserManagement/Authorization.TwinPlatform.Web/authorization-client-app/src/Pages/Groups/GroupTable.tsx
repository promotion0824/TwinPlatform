import { GridColDef, GridFilterModel, GridPaginationModel, GridRowSelectionModel, GridSortModel, GridValueGetterParams } from '@willowinc/ui';
import { Dispatch, SetStateAction, useMemo } from 'react';
import { Link } from 'react-router-dom';
import CustomAvatar from '../../Components/CustomAvatar';
import { CustomAvatarGroup } from '../../Components/CustomAvatarGroup';
import { CustomDataGrid } from '../../Components/CustomDataGrid';
import { BatchDto } from '../../types/BatchDto';
import { BatchRequestDto, FilterSpecificationDto, SortSpecificationDto } from '../../types/BatchRequestDto';
import { GroupModel } from '../../types/GroupModel';

function GroupTable({ selectionModel, setSelectionModel, batch, isFetching, batchRequest, setBatchRequest }:
  {
    selectionModel: GridRowSelectionModel,
    setSelectionModel: Dispatch<SetStateAction<GridRowSelectionModel>>,
    batch: BatchDto<GroupModel> | undefined,
    isFetching: boolean,
    batchRequest: BatchRequestDto,
    setBatchRequest: Dispatch<SetStateAction<BatchRequestDto>>
  }) {
  const columns: GridColDef[] = useMemo(() => [
    {
      field: 'avatar',
      headerName: '',
      description: 'This column displays user avatar',
      sortable: false,
      filterable: false,
      align: 'center',
      flex: 0.5,
      renderCell: (params) => {
        return (
          <CustomAvatar name={params.row.name}></CustomAvatar>
        );
      }
    },
    {
      field: 'name', headerName: 'Name', flex: 1, description: 'Name of the Group',
      renderCell: (params) => {
        return (
          <Link className="tableLink" to={encodeURIComponent(params.row.name) + '/'} >{params.row.name}</Link>
        );
      }
    },
    {
      field: 'groupType.name', headerName: 'Group Type', flex: 0.5, description: 'Type of the Group',
      valueGetter: (params: GridValueGetterParams) =>
        `${params.row.groupType?.name || ''}`,
    },
    {
      field: 'users',
      headerName: 'Users',
      description: 'Users Avatar',
      sortable: false,
      filterable: false,
      flex: 2,
      valueGetter: (params: GridValueGetterParams) =>
        params.row.users == null ? [] : params.row.users,
      renderCell: (params) => {
        return (
          <CustomAvatarGroup key={params.value} data={params.value} getName={(rec: any) => { return (rec.firstName || '') + ' ' + (rec.lastName || ''); }}></CustomAvatarGroup>
        );
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

export default GroupTable;  
