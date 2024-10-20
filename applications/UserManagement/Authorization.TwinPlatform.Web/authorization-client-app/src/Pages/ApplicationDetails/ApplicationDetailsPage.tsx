import { Box, Group, PageTitle, PageTitleItem, Tabs } from '@willowinc/ui';
import { ReactNode, Suspense, lazy, useEffect, useState } from 'react';
import { useParams } from 'react-router';
import { Link } from 'react-router-dom';
import { AppPermissions } from '../../AppPermissions';
import { AuthHandler } from '../../Components/AuthHandler';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { useLoading } from '../../Hooks/useLoading';
import { ApplicationApiClient, PermissionClient } from '../../Services/AuthClient';
import { ApplicationModel } from '../../types/ApplicationModel';
import { BatchRequestDto, FilterSpecificationDto } from '../../types/BatchRequestDto';
import { PermissionModel } from '../../types/PermissionModel';

interface LazyLoadedTabPanelProps {
  children: ReactNode;
  selectedValue: string;
  value: string;
}

const LazyLoadedTabPanel: React.FC<LazyLoadedTabPanelProps> = ({ children, selectedValue, value }) => {
  return (
    <div hidden={value !== selectedValue} style={{ height:'70vh'}}>
      {value === selectedValue && (
          <Suspense fallback={<div>Loading...</div>}>
            {children}
          </Suspense>
      )}
    </div>
  );
};

const LazyPermissionTab = lazy(() => import('./ApplicationPermission/ApplicationPermissionTab'));
const LazyApplicationClientsTab = lazy(() => import('./ApplicationClient/ApplicationClientsTab'));
const LazyApplicationAssignmentsTab = lazy(() => import('./ClientAssignment/ClientAssignmentsTab'));

function ApplicationDetailsPage() {
  let { name } = useParams();
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const [app, setApp] = useState<ApplicationModel>(new ApplicationModel());
  const [permissions, setPermissions] = useState<PermissionModel[]>([]);
  const [tabValue, setTabValue] = useState<string>("permissions");

  const handleChange = (value: string | null) => {
    if (!value)
      return;
    setTabValue(value);
  };

  useEffect(() => {
    async function fetchApplication() {
      try {

        if (!name) {
          enqueueSnackbar("Application name is required.", { variant: 'error' });

          return;
        }

        loader(true, 'Fetching application.');

        let application = await ApplicationApiClient.GetApplicationByName(name);
        setApp(application);

        loader(true, 'Fetching permissions.');
        const batchRequest = new BatchRequestDto();
        batchRequest.pageSize = 1000;
        batchRequest.filterSpecifications!.push(new FilterSpecificationDto("ApplicationId", "=", application.id, "AND"));
        const appPermissions = await PermissionClient.GetAllPermissions(batchRequest);
        setPermissions(appPermissions.items);
      } catch (e: any) {
        enqueueSnackbar("Error while fetching application", { variant: 'error' }, e);
      }
      finally {
        loader(false);
      }
    }
    fetchApplication();
  }, []);

  return (
    <AuthHandler requiredPermissions={[AppPermissions.CanReadApplication]}>
      <Group justify="space-between">
        <PageTitle>
          <PageTitleItem><Link to="/applications">Applications</Link></PageTitleItem>
          <PageTitleItem><Link to={`/applications/${name}`}>{name}</Link></PageTitleItem>
        </PageTitle>
        <Tabs onTabChange={handleChange} defaultValue="permissions">
          <Tabs.List>
            <AuthHandler requiredPermissions={[AppPermissions.CanReadPermission]}>
              <Tabs.Tab key="permissions" value="permissions">
                Permissions
              </Tabs.Tab>
            </AuthHandler>
            {!!app && app.supportClientAuthentication &&
              <>
                <AuthHandler requiredPermissions={[AppPermissions.CanReadApplicationClient]}>
                  <Tabs.Tab key="clients" value="clients">
                    Clients
                  </Tabs.Tab>
                </AuthHandler>
                <Tabs.Tab key="assignments" value="assignments">
                  Assignments
                </Tabs.Tab>
              </>
            }
          </Tabs.List>
        </Tabs>
      </Group>
        <LazyLoadedTabPanel selectedValue={tabValue} value="permissions">
          <AuthHandler requiredPermissions={[AppPermissions.CanReadPermission]}>
            <LazyPermissionTab application={app} />
          </AuthHandler>
        </LazyLoadedTabPanel>

        {!!app && app.supportClientAuthentication &&
          <>
            <LazyLoadedTabPanel selectedValue={tabValue} value="clients">
              <AuthHandler requiredPermissions={[AppPermissions.CanReadApplicationClient]}>
                <LazyApplicationClientsTab application={app} />
              </AuthHandler>
            </LazyLoadedTabPanel>
            <LazyLoadedTabPanel selectedValue={tabValue} value="assignments">
              <AuthHandler requiredPermissions={[AppPermissions.CanReadApplicationClient]}>
                <LazyApplicationAssignmentsTab application={app} appPermissions={permissions} />
              </AuthHandler>
            </LazyLoadedTabPanel>
          </>}
    </AuthHandler>);
}

export default ApplicationDetailsPage;
