import { Avatar, GridColDef, GridFilterModel, GridPaginationModel, GridRowSelectionModel, GridSortModel, GridValueGetterParams, Icon } from '@willowinc/ui';
import { Link } from "react-router-dom";
import { Dispatch, SetStateAction, useMemo, useState } from 'react';
import { PermissionFieldNames, PermissionModel } from '../../types/PermissionModel';
import { CustomDataGrid } from '../../Components/CustomDataGrid';
import { BatchRequestDto, FilterSpecificationDto, SortSpecificationDto } from '../../types/BatchRequestDto';
import { BatchDto } from '../../types/BatchDto';

function PermissionTable(
  { selectionModel,
    setSelectionModel,
    batch,
    isFetching,
    batchRequest,
    setBatchRequest
  }:
    {
      selectionModel: GridRowSelectionModel,
      setSelectionModel: Dispatch<SetStateAction<GridRowSelectionModel>>,
      batch: BatchDto<PermissionModel> | undefined,
      isFetching: boolean,
      batchRequest: BatchRequestDto,
      setBatchRequest: Dispatch<SetStateAction<BatchRequestDto>>
    }) {
  const columns: GridColDef[] = useMemo(() => [
    {
      field: 'avatar',
      headerName: '',
      description: 'This column displays permission avatar',
      sortable: false,
      filterable: false,
      align: 'center',
      flex: 0.5,
      renderCell: (params) => {
        return (
          <Avatar size="lg">
            <Icon icon="key" />
          </Avatar>
        );
      }
    },
    {
      field: PermissionFieldNames.name.field, headerName: PermissionFieldNames.name.label, description: 'Name of the Permission', flex: 1,
    },
    {
      field: PermissionFieldNames.applicationName.field, headerName: PermissionFieldNames.applicationName.label, description: 'Name of the Application', flex: 1,
      renderCell: (params) => {
        return (
          <Link className="tableLink" title={params.row?.application?.name} to={'/applications/' + encodeURIComponent(params.row?.application?.name)}>{params.row?.application?.name}</Link>
        );
      }
    },
    {
      field: PermissionFieldNames.description.field, headerName: PermissionFieldNames.description.label, description: 'Description', flex: 1,
    },
    {
      field: 'fullName',
      headerName: 'Permission Full Name',
      description: 'Full Name',
      sortable: false,
      filterable: false,
      flex: 1,
      valueGetter: (params: GridValueGetterParams) =>
        params.row.fullName,
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

export default PermissionTable;  
