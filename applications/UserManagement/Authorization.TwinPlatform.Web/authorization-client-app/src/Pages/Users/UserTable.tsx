import { LocalPoliceTwoTone } from '@mui/icons-material';
import { Badge } from '@mui/material';
import { GridColDef, GridFilterModel, GridPaginationModel, GridRowSelectionModel, GridSortModel, GridValueGetterParams, getGridNumericOperators, getGridSingleSelectOperators } from '@willowinc/ui';
import { Dispatch, SetStateAction, useMemo } from 'react';
import { Link } from 'react-router-dom';
import CustomAvatar from '../../Components/CustomAvatar';
import { CustomDataGrid } from '../../Components/CustomDataGrid';
import { BatchDto } from '../../types/BatchDto';
import { BatchRequestDto, FilterSpecificationDto, SortSpecificationDto } from '../../types/BatchRequestDto';
import { UserFieldNames, UserModel } from '../../types/UserModel';

function UserTable({ selectionModel, setSelectionModel, batch, isFetching, batchRequest, setBatchRequest }:
  {
    selectionModel: GridRowSelectionModel,
    setSelectionModel: Dispatch<SetStateAction<GridRowSelectionModel>>,
    batch: BatchDto<UserModel> | undefined,
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
          <Badge
            overlap="circular"
            anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
            badgeContent={params.row.isAdmin ? <LocalPoliceTwoTone style={{ fontSize: "1rem" }} titleAccess="Super Administrator" /> : <></>}
          >
            <CustomAvatar name={params.value}></CustomAvatar>
          </Badge>
        );
      },
      valueGetter: (params: GridValueGetterParams) =>
        `${params.row.firstName || ''} ${params.row.lastName || ''}`,
    },
    {
      field: 'email', headerName: 'Email', flex: 1, description: 'Email Address of the User',
      renderCell: (params) => {
        return (
          <Link className="tableLink" to={encodeURIComponent(params.row.email) + '/'} >{params.row.email}</Link>
        );
      }
    },
    { field: UserFieldNames.firstName.field, headerName: UserFieldNames.firstName.label, flex: 1, description: 'First Name of the User' },
    { field: UserFieldNames.lastName.field, headerName: UserFieldNames.lastName.label, flex: 1, description: 'Last Name of the User' },
    {
      field: UserFieldNames.status.field,
      headerName: UserFieldNames.status.label,
      description: 'Status Of the User',
      flex: 1,
      type: 'singleSelect',
      valueOptions: [
        { value: 0, label: 'Active' },
        { value: 1, label: 'Inactive' },
      ],
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

export default UserTable;  
