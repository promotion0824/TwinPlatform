import { InteractionRequiredAuthError, InteractionStatus } from '@azure/msal-browser';
import { useMsal } from '@azure/msal-react';
import { Box, Chip, ChipProps, LinearProgress, MenuItem, Typography } from '@mui/material';
import { Button, DataGrid, GridColDef, GridFilterModel, GridMoreVertIcon, GridRenderCellParams, GridToolbar, Menu } from '@willowinc/ui';
import { useCallback, useEffect, useState } from 'react';
import { useLocation, useNavigate, useSearchParams } from 'react-router-dom';
import { loginRequest } from '../config';
import { callGetDeploymentManifests, callGetDeploymentsSearch } from '../services/dashboardService';
import { Connector } from '../types/Connector';
import { DeploymentRecord } from '../types/DeploymentRecord';
import { DataGridBorderless } from './DataGridBorderless';
import { ButtonNoBorder } from './ButtonNoBorder';
import { CONNECTORS_PAGE } from './Layout';

export default function DeploymentDataGrid(props: { setOpenError: (open: boolean) => void; }) {
  const { setOpenError } = props;

  function getChipProps(params: GridRenderCellParams): ChipProps {
    switch (params.value) {
      case "Pending":
        return {
          label: "Pending",
          color: "warning"
        };
      case "InProgress":
        return {
          label: "InProgress",
          color: "primary"
        };
      case "Succeeded":
        return {
          label: "Succeeded",
          color: "success"
        };
      case "Failed":
        return {
          label: "Failed",
          color: "error"
        };
      default:
        return {
          label: params.value
        };
    }
  }

  const columns: GridColDef[] = [
    { field: 'id', headerName: 'Deployment ID', flex: 1.2 },
    { field: 'name', headerName: 'Deployment Name' },
    { field: 'moduleId', headerName: 'Module ID' },
    { field: 'moduleName', headerName: 'Module Name', flex: 1 },
    { field: 'deviceName', headerName: 'Device Name', flex: 0.9 },
    { field: 'moduleType', headerName: 'Application Type',flex: 1.1 },
    { field: 'version', headerName: 'Version', flex: 0.4 },
    {
      field: 'dateTimeCreated', headerName: 'Date Created', flex: 0.75, type: 'dateTime',
      valueGetter: ({ value }) => value && new Date(value),
    },
    {
      field: 'dateTimeApplied', headerName: 'Date Applied', flex: 0.75, type: 'dateTime',
      valueGetter: ({ value }) => value && new Date(value),
    },
    { field: 'assignedBy', headerName: 'Assigned By', flex: 0.75 },
    {
      field: 'status', headerName: 'Status', flex: 0.5, renderCell: (params) => {
        return params.value && <Chip size="small" {...getChipProps(params)} />;
      }
    },
    { field: 'statusMessage', headerName: 'Status Message', flex: 1 },
    {
      field: "actions",
      headerName: "Actions",
      sortable: false,
      filterable: false,
      width: 75,
      renderCell: (params) => {
        return (
          <Box>
            <Menu id="deployment-actions-menu">
              <Menu.Target>
                <ButtonNoBorder
                  id="deployment-actions-button"
                  onClick={handleClick}
                  kind="secondary"
                  variant="transparent"
                  prefix={<GridMoreVertIcon />}
                />
              </Menu.Target>
              <Menu.Dropdown>
                <Menu.Item onClick={handleDownloadManifestActionClick}>Download Manifest</Menu.Item>
                <Menu.Item onClick={handleViewConnectorActionClick}>View Connector</Menu.Item>
              </Menu.Dropdown>
            </Menu>
          </Box>
        );
      }
    }
  ];

  // Extract filter module from state if any
  // This filter can be passed from Connectors view
  const { state } = useLocation();
  const filterModule = state as Connector;
  const defaultFilter = state
    ? {
      items: [
        {
          field: 'moduleId',
          operator: 'equals',
          value: filterModule?.id,
        },
      ],
    }
    : undefined;

  const [searchParams] = useSearchParams();
  const deviceSearch = searchParams.get('deviceName') || '';

  const [filterModel, setFilterModel] = useState<GridFilterModel | undefined>(defaultFilter);
  const [filterDeviceName, setFilterDeviceName] = useState<string>(deviceSearch);

  const { instance, inProgress, accounts } = useMsal();
  const [accessToken, setAccessToken] = useState<string>('');
  const [pageSize, setPageSize] = useState<number>(10);
  const [page, setPage] = useState<number>(0);
  const [rowCount, setRowCount] = useState<number>(0);
  const [moduleId, setModuleId] = useState<string | null>(filterModule?.id);
  const [apiData, setApiData] = useState(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [selectedDeployment, setSelectedDeployment] = useState<DeploymentRecord>({} as DeploymentRecord);

  const navigate = useNavigate();

  const handleRowClick = (rowData: DeploymentRecord) => {
    setSelectedDeployment({ ...rowData });
  };

  const handleViewConnectorActionClick = () => {
    navigate(CONNECTORS_PAGE, { state: selectedDeployment });
  };

  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);
  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };
  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleDownloadManifestActionClick = async () => {
    handleClose();

    try {
      const response = await callGetDeploymentManifests(accessToken, selectedDeployment?.id);
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', selectedDeployment?.id + `.zip`);
      document.body.appendChild(link);
      link.click();
      link.parentNode?.removeChild(link);
    } catch (error: any) {
      console.error(error.message);
      setOpenError(true);     // Show the error Notification
    }
  };

  useEffect(() => {
    const getDeploymentRows = async () => {
      // Already loaded or loading
      if (apiData || inProgress !== InteractionStatus.None) {
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
        const newAccessToken = accessTokenResponse.accessToken;
        setAccessToken(newAccessToken);

        // Call your API with token
        const response = await callGetDeploymentsSearch(newAccessToken, pageSize, page+1, moduleId, filterDeviceName);
        const responseObject = response.data;
        setApiData(responseObject.items);
        setRowCount(responseObject.totalCount);
        setLoading(false);
      } catch (error) {
        if (error instanceof InteractionRequiredAuthError) {
          // Acquire token interactive
          instance.acquireTokenRedirect(accessTokenRequest);
        }

        setLoading(false);
        console.error(error);
        setOpenError(true);     // Show the error Notification
      }
    }

    getDeploymentRows();
  }, [instance, accounts, inProgress, apiData, page, pageSize, moduleId, filterDeviceName, setOpenError]);

  return (
    <DataGridBorderless
      paginationModel={{
        pageSize: pageSize,
        page: page,

      }}
      onPaginationModelChange={(model) => {
        setPage(model.page);
        setPageSize(model.pageSize);
        setApiData(null); // Trigger useEffect to get data for new pageSize
      }}
      pageSizeOptions={[5, 10, 20]}
      pagination
      paginationMode="server"
      rowCount={rowCount}
      rows={apiData ?? []}
      columns={columns}
      slots={{ loadingOverlay: LinearProgress, toolbar: GridToolbar }}
      loading={loading}
      filterModel={filterModel}
      onFilterModelChange={(newFilterModel) => {
        // Note that most filter operations are applied after the data is retrieved from the API. Therefore, only a subset of data is displayed in the grid (after the filter is applied).
        // For ‘contains’ filter operations applied on the 'deviceName' column, the filter value is passed to the API but then the built-in filter is applied on the client side.
        // This can result in unusual behavior if the API and Client-side filters do not work in the same way. Given that the API filter works like 'contains' operation,
        // the contains filter is used to map to the API filter below
        const deviceName = newFilterModel.items.find(i => i.field === 'deviceName' && i.operator === 'contains')?.value;     // API Filter by device name
        setFilterDeviceName(deviceName);

        setFilterModel(newFilterModel);
        setModuleId(null);
        setApiData(null);
      }}
      onRowClick={(rowData) => handleRowClick(rowData.row)}
      initialState={{
        columns: {
          columnVisibilityModel: {
            // Hide specified columns, the other columns will remain visible
            name: false,
            moduleId: false,
            statusMessage: false
          },
        },
      }}
    />
  );
}
