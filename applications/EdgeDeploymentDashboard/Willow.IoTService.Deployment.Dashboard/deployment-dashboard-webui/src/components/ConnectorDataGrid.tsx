import { InteractionRequiredAuthError, InteractionStatus } from '@azure/msal-browser';
import { useMsal } from '@azure/msal-react';
import { Chip, ChipProps, LinearProgress } from '@mui/material';
import Box from '@mui/material/Box';
import { GridColDef, GridFilterModel, GridMoreVertIcon, GridRenderCellParams, GridToolbar, Menu } from '@willowinc/ui';
import { useCallback, useEffect, useState } from 'react';
import { useLocation, useNavigate, useSearchParams } from 'react-router-dom';
import { loginRequest } from '../config';
import { callGetModule, callGetModulesSearch } from '../services/dashboardService';
import { Connector } from '../types/Connector';
import { DeploymentRecord } from '../types/DeploymentRecord';
import ConnectorCreateModule from './ConnectorCreateModule';
import ConnectorDialog from './ConnectorDialog';
import DeploymentDialogCreateDeployment from './DeploymentDialogCreateDeployment';
import { DataGridBorderless } from './DataGridBorderless';
import { ButtonNoBorder } from './ButtonNoBorder';
import { DEPLOYMENTS_PAGE } from './Layout';

export default function ConnectorDataGrid(props: { setOpenError: (open: boolean) => void; }) {
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
      case true:
        return {
          label: "Yes",
          style: {
            backgroundColor: "#90CAF9",
            color: "#000000"
          }
        };
      case false:
        return {
          label: "No",
          color: "secondary"
        };
      default:
        return {
          label: params.value
        };
    }
  }

  const columns: GridColDef[] = [
    { field: 'id', headerName: 'ID', flex: 1 },
    { field: 'name', headerName: 'Connector', flex: 1 },
    { field: 'deviceName', headerName: 'Device Name', flex: 0.75 },
    { field: 'moduleType', headerName: 'Application Type', flex: 1 },
    { field: 'ioTHubName', headerName: 'IoT Hub', flex: 1 },
    { field: 'environment', headerName: 'Environment', flex: 1 },
    {
      field: 'dateTimeApplied', headerName: 'Deployment Date', width: 160, type: 'dateTime',
      valueGetter: ({ value }) => value && new Date(value)
    },
    { field: 'version', headerName: 'Version', flex: 0.5 },
    { field: 'assignedBy', headerName: 'Assigned By', flex: 0.75 },
    {
      field: 'status', headerName: 'Status', flex: 0.75, renderCell: (params) => {
        return params.value !== null && <Chip size="small" {...getChipProps(params)} />;
      }
    },
    { field: 'statusMessage', headerName: 'Status Message' },
    {
      field: 'isAutoDeployment', headerName: 'Auto Deploy', flex: 0.5, renderCell: (params) => {
        return params.value !== null && <Chip size="small" {...getChipProps(params)} />;
      }
    },
    { field: 'platform', headerName: 'Platform', flex: 0.5 },
    {
      field: 'isSynced', headerName: 'Synced', flex: 0.5, renderCell: (params) => {
        return params.value !== null && <Chip size="small" {...getChipProps(params)} />;
      }
    },
    {
      field: "actions",
      headerName: "Actions",
      sortable: false,
      filterable: false,
      width: 75,
      renderCell: (params) => {
        return (
          <Box>
            <Menu id="connector-actions-menu">
              <Menu.Target>
                <ButtonNoBorder
                  id="connector-actions-button"
                  onClick={handleClick}
                  prefix={<GridMoreVertIcon />}
                  kind="secondary"
                  variant="transparent"
                />
              </Menu.Target>
              <Menu.Dropdown>
                <Menu.Item onClick={handleCreateDeployment}>Create Deployment</Menu.Item>
                <Menu.Item onClick={handleCopyIdClick}>Copy ID</Menu.Item>
                <Menu.Item onClick={handleUpdateConfigurationActionClick}>Update Configuration</Menu.Item>
                <Menu.Item onClick={handleViewDeploymentsActionClick}>View Deployments</Menu.Item>
              </Menu.Dropdown>
            </Menu>
          </Box>
        );
      }
    }
  ];

  // Extract filter module from state if any
  // This filter can be passed from Deployments view
  const { state } = useLocation();
  const filterModule = state as DeploymentRecord;
  const defaultFilter = state
    ? {
      items: [
        {
          field: 'id',
          operator: 'equals',
          value: filterModule?.moduleId,
        },
      ],
    } as GridFilterModel
    : undefined;

  const [searchParams] = useSearchParams();
  const deviceSearch = searchParams.get('deviceName') || '';

  const [filterModel, setFilterModel] = useState<GridFilterModel | undefined>(defaultFilter);
  const [filterModuleId, setFilterModuleId] = useState<string | null>(filterModule?.moduleId);
  const [filterConnectorName, setFilterConnectorName] = useState<string>();
  const [filterDeviceName, setFilterDeviceName] = useState<string>(deviceSearch);

  const { instance, inProgress, accounts } = useMsal();
  const [pageSize, setPageSize] = useState<number>(10);
  const [page, setPage] = useState<number>(0);
  const [rowCount, setRowCount] = useState<number>(0);
  const [apiData, setApiData] = useState(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [openEditDialog, setOpenEditDialog] = useState<boolean>(false);
  const [selectedConnector, setSelectedConnector] = useState<Connector>({} as Connector);
  const [openDialogCreateDeployment, setOpenDialogCreateDeployment] = useState<boolean>(false);

  const navigate = useNavigate();

  const handleRowClick = (rowData: Connector) => {
    setSelectedConnector({ ...rowData });
  };

  const handleViewDeploymentsActionClick = () => {
    handleCloseRowMenu();
    navigate(DEPLOYMENTS_PAGE, { state: selectedConnector });
  };

  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);
  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };
  const handleCloseRowMenu = () => {
    setAnchorEl(null);
  };

  const handleUpdateConfigurationActionClick = () => {
    handleCloseRowMenu();
    setOpenEditDialog(true);
  };

  const handleRowDoubleClick = (rowData: Connector) => {
    setSelectedConnector({ ...rowData });
    setOpenEditDialog(true);
  };

  const closeDialogHandler = useCallback(() => {
    setOpenEditDialog(false);
  }, []);

  const saveHandler = useCallback((change: boolean) => {
    if (change) {
      // To trigger data grid refresh after saved
      setApiData(null);
    }
  }, []);

  const handleCreateDeployment = () => {
    handleCloseRowMenu();
    setOpenDialogCreateDeployment(true);
  };

  const handleCloseDialogCreateDeployment = useCallback(() => {
    setOpenDialogCreateDeployment(false);
  }, []);

  const handleCopyIdClick = () => {
    selectedConnector.id && navigator.clipboard.writeText(selectedConnector.id);
  }

  useEffect(() => {
    const getModuleRows = async () => {
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
        let accessToken = accessTokenResponse.accessToken;

        // Call your API with token
        if (filterModuleId) {
          const response = await callGetModule(accessToken, filterModuleId);
          let data: any = [response.data];
          setApiData(data);
          setRowCount(1);
          setLoading(false);
        } else {
          const response = await callGetModulesSearch(accessToken, pageSize, page + 1, filterConnectorName, false, undefined, undefined, filterDeviceName);
          let responseObject = response.data;

          setApiData(responseObject.items);
          setRowCount(responseObject.totalCount);
          setLoading(false);
        }
      } catch (error) {
        if (error instanceof InteractionRequiredAuthError) {
          instance.acquireTokenRedirect(accessTokenRequest);
        }

        setLoading(false);
        console.error(error);
        setOpenError(true);     // Show the error Notification
      }
    }

    getModuleRows();
  }, [instance, accounts, inProgress, apiData, page, pageSize, filterModuleId, filterConnectorName, filterDeviceName, setOpenError]);

  return (
    <>
      <DataGridBorderless
        paginationModel={{
          pageSize: pageSize,
          page: page,
        }}
        onPaginationModelChange={(newModel) => {
          setPageSize(newModel.pageSize);
          setPage(newModel.page);
          setApiData(null); // Trigger useEffect to get data for new pageSize and page
        }}
        pageSizeOptions={[5, 10, 20]}
        pagination
        paginationMode="server"
        rowCount={rowCount}
        rows={apiData ?? []}
        columns={columns}
        slots={{ loadingOverlay: LinearProgress, toolbar: GridToolbar }}
        loading={loading}
        onRowDoubleClick={(rowData) => handleRowDoubleClick(rowData.row)}
        onRowClick={(rowData) => handleRowClick(rowData.row)}
        filterModel={filterModel}
        onFilterModelChange={(newFilterModel) => {
          // Note that most filter operations are applied after the data is retrieved from the API. Therefore, only a subset of data is displayed in the grid (after the filter is applied).
          // For ‘contains’ filter operations applied on the 'name' and 'deviceName' columns, the filter value is passed to the API but then the built-in filter is applied on the client side.
          // This can result in unusual behavior if the API and Client-side filters do not work in the same way. Given that the API filter works like 'contains' operation,
          // the contains filter is used to map to the API filter below
          const name = newFilterModel.items.find(idx => idx.field === 'name' && idx.operator === 'contains')?.value;     // API Filter by connector name
          setFilterConnectorName(name);

          const deviceName = newFilterModel.items.find(idx => idx.field === 'deviceName' && idx.operator === 'contains')?.value;     // API Filter by device name
          setFilterDeviceName(deviceName);

          setFilterModel(newFilterModel);
          setFilterModuleId(null);
          setApiData(null);
        }}
        initialState={{
          columns: {
            columnVisibilityModel: {
              // Hide specified columns, the other columns will remain visible
              id: false,
              statusMessage: false,
              isAutoDeployment: false,
            },
          },
        }}
      />
      <ConnectorDialog
        connector={selectedConnector}
        open={openEditDialog}
        closeHandler={closeDialogHandler}
        onConfirm={saveHandler}
        setOpenError={setOpenError}
      />
      <DeploymentDialogCreateDeployment
        open={openDialogCreateDeployment}
        closeHandler={handleCloseDialogCreateDeployment}
        onConfirm={saveHandler}
        connector={selectedConnector}
        setOpenError={setOpenError}
      />
    </>
  );
}
