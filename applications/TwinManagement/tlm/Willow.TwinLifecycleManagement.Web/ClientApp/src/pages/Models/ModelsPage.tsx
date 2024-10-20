import { useMemo } from 'react';
import { Link } from 'react-router-dom';
import { GridColDef, GridToolbar, useGridApiRef } from '@mui/x-data-grid-pro';
import { Button } from '@mui/material';
import { StyledHeader } from '../../components/Common/StyledComponents';
import { useState } from 'react';
import { ApiException, ErrorResponse } from '../../services/Clients';
import { PopUpExceptionTemplate } from '../../components/PopUps/PopUpExceptionTemplate';
import useGetModels from '../../hooks/useGetModels';
import { AuthHandler } from '../../components/AuthHandler';
import { AppPermissions } from '../../AppPermissions';
import { usePersistentGridState } from '../../hooks/usePersistentGridState';
import { DataGrid } from '@willowinc/ui';
import styled from '@emotion/styled';

export default function ModelsPage() {
  const apiRef = useGridApiRef();
  const { savedState } = usePersistentGridState(apiRef, 'models');
  const [openPopUp, setOpenPopUp] = useState(false);

  const [errorMessage, setErrorMessage] = useState<ErrorResponse | ApiException>();

  const { data: models = [], isLoading } = useGetModels({
    select: (data) => {
      let arrayForSort = [...data];
      arrayForSort.sort((a, b) => {
        let { totalCount: aCount = 0, name: aName = '' } = a;
        let { totalCount: bCount = 0, name: bName = '' } = b;

        // first compare based on totalCount
        if (aCount > bCount) {
          return -1;
        } else if (aCount < bCount) {
          return 1;
        } else {
          // if count is the same, compare based on name
          if (aName < bName) {
            return -1;
          } else if (aName > bName) {
            return 1;
          } else {
            return 0;
          }
        }
      });
      return arrayForSort;
    },
    onError: (error) => {
      setErrorMessage(error);
      setOpenPopUp(true);
    },
  });

  const columns: GridColDef[] = useMemo(
    () => [
      {
        field: 'name',
        headerName: 'Model',
        flex: 1.2,
      },
      { field: 'id', headerName: 'ID', flex: 1.2 },
      { field: 'description', headerName: 'Description', flex: 2 },
      {
        field: 'uploadTime',
        headerName: 'Upload Time',
        flex: 0.7,
        sortComparator: (x: any, y: any) => new Date(x).getTime() - new Date(y).getTime(),
        valueFormatter: (params: any) => {
          return new Date(params.value).toLocaleDateString('en-US', {
            weekday: 'short',
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: 'numeric',
            minute: 'numeric',
            timeZoneName: 'short',
          });
        },
      },
      { field: 'exactCount', headerName: 'Exact Twins', flex: 0.5 },
      { field: 'totalCount', headerName: 'Total Twins', flex: 0.5 },
    ],
    []
  );

  return (
    <AuthHandler requiredPermissions={[AppPermissions.CanReadModels]} noAccessAlert>
      <StyledHeader variant="h1">Models</StyledHeader>
      <HeaderContainer>
        <AuthHandler requiredPermissions={[AppPermissions.CanImportModelsFromGit]}>
          <Link to="/import-models">
            <Button variant="contained" sx={{ float: 'right' }}>
              Upload
            </Button>
          </Link>
        </AuthHandler>

        <AuthHandler requiredPermissions={[AppPermissions.CanDeleteModels]}>
          <Link to="/delete-all-models">
            <Button variant="contained" color="error" sx={{ float: 'right' }}>
              Delete All
            </Button>
          </Link>
        </AuthHandler>
      </HeaderContainer>
      <div style={{ height: '81vh', width: '100%', backgroundColor: '#242424' }}>
        <DataGrid
          apiRef={apiRef}
          rows={models}
          columns={columns}
          slots={{ toolbar: GridToolbar }}
          loading={isLoading}
          initialState={savedState}
        />

        {<PopUpExceptionTemplate isCurrentlyOpen={openPopUp} onOpenChanged={setOpenPopUp} errorObj={errorMessage} />}
      </div>
    </AuthHandler>
  );
}
const HeaderContainer = styled('div')({
  display: 'flex',
  width: '100%',
  alignItems: 'center',
  justifyContent: 'space-between',
  marginBottom: '1rem',
});
