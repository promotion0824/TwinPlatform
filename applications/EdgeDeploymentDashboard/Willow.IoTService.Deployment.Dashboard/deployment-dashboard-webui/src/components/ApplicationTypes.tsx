import { InteractionRequiredAuthError, InteractionStatus } from '@azure/msal-browser';
import { useMsal } from '@azure/msal-react';
import { Box, LinearProgress } from '@mui/material';
import { GridColDef, GridMoreVertIcon, GridToolbar, Menu, Panel, PanelGroup } from '@willowinc/ui';
import { useCallback, useEffect, useState } from 'react';
import { loginRequest } from '../config';
import { callGetModuleTypes, callGetModuleTypesSearch } from '../services/dashboardService';
import { ApplicationType } from '../types/ApplicationType';
import ApplicationTypeUploadTemplate from './ApplicationTypeUploadTemplate';
import { DataGridBorderless } from './DataGridBorderless';
import { ButtonNoBorder } from './ButtonNoBorder';

export default function ApplicationTypes(props: { setOpenError: (open: boolean) => void; }) {
  const { setOpenError } = props;

  const columns: GridColDef[] = [
    { field: 'id', headerName: 'ID' },
    { field: 'name', headerName: 'Name', flex: 1 },
    { field: 'version', headerName: 'Version', flex: 0.5 },
    {
      field: 'actions',
      headerName: 'Actions',
      sortable: false,
      filterable: false,
      width: 75,
      renderCell: (params) => {
        return (
          <Box>
            <Menu id="module-types-actions-menu">
              <Menu.Target>
                <ButtonNoBorder
                  id="module-types-actions-button"
                  kind="secondary"
                  prefix={<GridMoreVertIcon />}
                />
              </Menu.Target>
              <Menu.Dropdown>
                <Menu.Item onClick={handleDownloadTemplateActionClick}>Download Template</Menu.Item>
                <Menu.Item onClick={handleUploadTemplateActionClick}>Upload Template</Menu.Item>
              </Menu.Dropdown>
            </Menu>
          </Box>
        );
      }
    }
  ];

  const handleDownloadTemplateActionClick = async () => {

    try {
      const response = await callGetModuleTypes(accessToken, selectedApplicationType.name, selectedApplicationType.version);

      const url = window.URL.createObjectURL(new Blob([response.data]));

      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `${selectedApplicationType.name}_${selectedApplicationType.version}.json`);
      document.body.appendChild(link);
      link.click();
      link.parentNode?.removeChild(link);
    } catch (error: any) {
      console.error(error.message);
      setOpenError(true);     // Show the error Notification
    }
  };

  const { instance, inProgress, accounts } = useMsal();
  const [moduleTypes, setModuleTypes] = useState<any>();
  const [loading, setLoading] = useState<boolean>(false);
  const [pageSize, setPageSize] = useState<number>(10);
  const [page, setPage] = useState<number>(0);
  const [rowCount, setRowCount] = useState<number>(0);
  const [accessToken, setAccessToken] = useState<string>('');
  const [openDialogUploadTemplate, setOpenDialogUploadTemplate] = useState<boolean>(false);
  const [selectedApplicationType, setSelectedApplicationType] = useState<ApplicationType>({} as ApplicationType);

  const handleUploadTemplateActionClick = () => {
    setOpenDialogUploadTemplate(true);
  };

  const handleCloseDialogUploadTemplate = useCallback(() => {
    setOpenDialogUploadTemplate(false);
  }, []);

  const saveHandler = useCallback((change: boolean) => {
    if (change) {
      // To trigger data grid refresh after saved
      setModuleTypes(null);
    }
  }, []);

  const handleRowClick = (rowData: ApplicationType) => {
    setSelectedApplicationType({ ...rowData });
  };

  const handleRowDoubleClick = (rowData: ApplicationType) => {
    setSelectedApplicationType({ ...rowData });
  };

  useEffect(() => {
    const getData =
      setTimeout(async () => {
        // Already loaded or loading
        if (moduleTypes || inProgress !== InteractionStatus.None) {
          return;
        }

        const accessTokenRequest = {
          scopes: loginRequest.scopes,
          account: accounts[0],
        };

        try {
          setLoading(true);

          const accessTokenResponse = await instance.acquireTokenSilent(accessTokenRequest);

          // Acquire token silent success
          const accessToken = accessTokenResponse.accessToken;
          setAccessToken(accessToken);

          const response = await callGetModuleTypesSearch(accessToken, pageSize, page + 1);
          const responseObject = response.data;
          const types = responseObject.items as [];
          let map = types.map((value: any, index) => {
            return { id: index, name: value.moduleType, version: value.latestVersion };
          });

          setModuleTypes(map);
          setRowCount(responseObject.totalCount);
          setLoading(false);
        } catch (error) {
          if (error instanceof InteractionRequiredAuthError) {
            instance.acquireTokenRedirect(accessTokenRequest);
          }

          console.log(error);
          setOpenError(true);     // Show the error Notification
          setLoading(false);
        }
      }, 250);

    return () => clearTimeout(getData);
  }, [instance, accounts, inProgress, moduleTypes, page, pageSize, setOpenError]);

  return (
    <PanelGroup>
      <Panel title="Application Types">
        <DataGridBorderless
          paginationModel={{
            pageSize: pageSize,
            page: page,
          }}
          onPaginationModelChange={(newModel) => {
            setPageSize(newModel.pageSize);
            setPage(newModel.page);
            setModuleTypes(null); // Trigger useEffect to get data for new pageSize
          }}
          pageSizeOptions={[5, 10, 20]}
          pagination
          paginationMode="server"
          rowCount={rowCount}
          rows={moduleTypes ?? []}
          columns={columns}
          slots={{ loadingOverlay: LinearProgress, toolbar: GridToolbar }}
          loading={loading}
          onRowDoubleClick={(rowData) => handleRowDoubleClick(rowData.row)}
          onRowClick={(rowData) => handleRowClick(rowData.row)}
          initialState={{
            columns: {
              columnVisibilityModel: {
                // Hide specified columns, the other columns will remain visible
                id: false
              },
            },
          }}
        />
        <ApplicationTypeUploadTemplate
          open={openDialogUploadTemplate}
          closeHandler={handleCloseDialogUploadTemplate}
          onConfirm={saveHandler}
          moduleType={selectedApplicationType}
          setOpenError={setOpenError}
        />
      </Panel>
    </PanelGroup>
  );
}
