import { Tooltip } from '@mui/material';
import { GridColDef, GridRenderCellParams, GridRowSelectionModel, GridValueGetterParams, Icon, Avatar } from '@willowinc/ui';
import { Dispatch, SetStateAction, useState } from 'react';
import { CustomDataGrid } from '../../../Components/CustomDataGrid';
import ExpressionWithStatusIndicator from '../../../Components/ExpressionStatusIndicator';
import { ClientAssignmentModel } from '../../../types/ClientAssignmentModel';

function ClientAssignmentsTable({ selectionModel, setSelectionModel, rows }:
  { selectionModel: GridRowSelectionModel, setSelectionModel: Dispatch<SetStateAction<GridRowSelectionModel>>, rows: ClientAssignmentModel[] }) {

  const [pageSize, setPageSize] = useState<number>(10);
  const [page, setPage] = useState<number>(0);

  const columns: GridColDef[] = [
    {
      field: 'avatar',
      headerName: '',
      description: 'This column displays assignment avatar',
      sortable: false,
      filterable: false,
      align: 'center',
      minWidth: 100,
      renderCell: (params) => {
        return (
          <Avatar>
            <Icon size={16} icon="assignment" />
          </Avatar>
        );
      }
    },
    {
      field: 'applicationClient', headerName: 'Client Name', minWidth: 200, description: 'Name of the client',
      valueGetter: (params: GridValueGetterParams) =>
       params.row.applicationClient.name,
    },
    {
      field: 'expression', headerName: 'Expression', minWidth: 200, description: 'Assigned Expression', sortable: false,
      renderCell: (params: GridRenderCellParams) =>
        <Tooltip title={params.row.expression} >
          <span style={{ textOverflow: 'ellipsis', overflow: 'hidden' }}>{params.row.expression}</span>
        </Tooltip>,
    },
    {
      field: 'condition', headerName: 'Condition', minWidth: 200, description: 'Assigned Condition', sortable: false,
      renderCell: (params: GridRenderCellParams) =>
        <ExpressionWithStatusIndicator expression={params.value} status={params.row.conditionExpressionStatus} />
    },
    {
      field: 'permissions', headerName: 'Permissions', description: 'Assigned Permissions Count', sortable: false, minWidth: 200,
      valueGetter: (params: GridValueGetterParams) =>
        params.row.permissions?.length ?? 0
    },
  ];

  return (
    <CustomDataGrid
      rows={rows}
      getRowId={(r) => r.id}
      columns={columns}
      pagination
      paginationModel={{
        pageSize: pageSize,
        page: page,
      }}
      onPaginationModelChange={(newPageModel) => {
        setPageSize(newPageModel.pageSize);
        setPage(newPageModel.page);
      }}
      pageSizeOptions={[5, 10, 20]}
      onRowSelectionModelChange={(newSelectionModel) => {
        setSelectionModel(newSelectionModel);
      }}
      rowSelectionModel={selectionModel}
      checkboxSelection
    />);
}

export default ClientAssignmentsTable;  
