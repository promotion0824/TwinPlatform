import { Group as LayoutGroup, GridRowSelectionModel, PageTitle, PageTitleItem } from '@willowinc/ui';
import { useEffect, useState } from "react";
import { ApplicationModel } from "../../types/ApplicationModel";
import { useLoading } from "../../Hooks/useLoading";
import { useCustomSnackbar } from "../../Hooks/useCustomSnackbar";
import { ApplicationApiClient } from "../../Services/AuthClient";
import { AuthHandler } from "../../Components/AuthHandler";
import { AppPermissions } from "../../AppPermissions";
import { Link } from 'react-router-dom';
import ApplicationTable from './ApplicationTable';

function ApplicationPage() {
  const [selectionModel, setSelectionModel] = useState<GridRowSelectionModel>([]);
  const [rows, setRows] = useState<ApplicationModel[]>([]);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const [dataModified, setDataModified] = useState<{ modified: Date }>({ modified: new Date() });

  const refreshTable = () => {
    setDataModified({ modified: new Date() });
  };

  useEffect(() => {
    async function fetchGroups() {
      try {
        loader(true, 'Fetching Applications.');
        setRows(await ApplicationApiClient.GetAllApplications());

      } catch (e: any) {
        enqueueSnackbar("Error while fetching applications.", { variant: 'error' }, e);
      }
      finally {
        loader(false);
      }
    }
    fetchGroups();
  }, [dataModified]);

  return (
    <AuthHandler requiredPermissions={[AppPermissions.CanReadApplication]}>
      <LayoutGroup justify="space-between">
        <PageTitle>
          <PageTitleItem><Link to="/applications">Applications</Link></PageTitleItem>
        </PageTitle>
      </LayoutGroup>
      <ApplicationTable selectionModel={selectionModel} setSelectionModel={setSelectionModel} rows={rows} />
    </AuthHandler>);
}
export default ApplicationPage;  
