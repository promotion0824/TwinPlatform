import { GridColDef, GridRowSelectionModel } from '@willowinc/ui';
import { Dispatch, SetStateAction, useMemo, useState } from 'react';
import CustomAvatar from '../../Components/CustomAvatar';
import { CustomDataGrid } from '../../Components/CustomDataGrid';
import { Link } from 'react-router-dom';
import { ApplicationModel } from '../../types/ApplicationModel';
import Check from '@mui/icons-material/Check';

function ApplicationTable({ selectionModel, setSelectionModel, rows }:
  { selectionModel: GridRowSelectionModel, setSelectionModel: Dispatch<SetStateAction<GridRowSelectionModel>>, rows: ApplicationModel[] }) {
  const columns: GridColDef[] = useMemo(() => [
    {
      field: 'avatar',
      headerName: '',
      description: 'This column displays user avatar',
      sortable: false,
      filterable: false,
      align: 'center',
      renderCell: (params) => {
        return (
          <CustomAvatar name={params.row.name}></CustomAvatar>
        );
      }
    },
    {
      field: 'name', headerName: 'Application Name', minWidth: 200, description: 'Name of the Application',
      renderCell: (params) => {
        return (
          <Link className="tableLink" to={encodeURIComponent(params.row.name) + '/'} >{params.row.name}</Link>
        );
      }
    },
    {
      field: 'description', headerName: 'Description', minWidth: 500, description: 'Application Description',
    },
    {
      field: 'supportClientAuthentication', headerName: 'Supports Client Authentication', minWidth: 250, description: 'Specify whether the app support client authentication',
      renderCell: (params) => {
        return (
          params.value && <Check fontSize="small" />
        );
      },
    }
  ], []);

  const [pageSize, setPageSize] = useState<number>(10);
  const [page, setPage] = useState<number>(0);

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

export default ApplicationTable;  
