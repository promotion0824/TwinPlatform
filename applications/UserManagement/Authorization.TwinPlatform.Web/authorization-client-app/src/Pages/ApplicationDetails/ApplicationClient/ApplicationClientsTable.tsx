import { Box, GridColDef, GridRenderCellParams, GridRowSelectionModel, Loader } from '@willowinc/ui';
import { Dispatch, SetStateAction, useState } from 'react';
import { CustomDataGrid } from '../../../Components/CustomDataGrid';
import { ApplicationClientModel } from '../../../types/ApplicationClientModel';
import { SecretCredentials } from '../../../types/ClientAppPasswordModel';

function ApplicationClientsTable({ selectionModel, setSelectionModel, rows, secretCredentials }:
  { selectionModel: GridRowSelectionModel, setSelectionModel: Dispatch<SetStateAction<GridRowSelectionModel>>, rows: ApplicationClientModel[], secretCredentials: { secrets: SecretCredentials, loaded: boolean } }) {
  const columns: GridColDef[] = [
    {
      field: 'name', headerName: 'Name', minWidth: 200, description: 'Name of the Client'
    },
    {
      field: 'description', headerName: 'Description', minWidth: 200, description: 'Description for the Client',
    },
    {
      field: 'clientId', headerName: 'Client ID', minWidth: 300, description: 'Client Identifier'
    },
    {
      field: 'expiryDate', headerName: 'Secret Expires On', minWidth: 300, description: 'Password Secret Expiry',
      renderCell: (params: GridRenderCellParams) =>
        <Box>
          {secretCredentials.loaded ?
            !secretCredentials?.secrets[params.row.clientId] ? "" : new Date(secretCredentials.secrets[params.row.clientId].endTime).toDateString()
            :
            <Loader size="sm" variant="dots" />
          }
        </Box>,
    },
  ];

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

export default ApplicationClientsTable;  
