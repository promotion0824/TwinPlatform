import { Group as LayoutGroup, GridRowSelectionModel, ButtonGroup } from "@willowinc/ui";
import { useEffect, useState } from "react";
import { useCustomSnackbar } from "../../../Hooks/useCustomSnackbar";
import { useLoading } from "../../../Hooks/useLoading";
import { ApplicationApiClient } from "../../../Services/AuthClient";
import ApplicationClientsTable from "./ApplicationClientsTable";
import { AuthHandler } from "../../../Components/AuthHandler";
import { AppPermissions } from "../../../AppPermissions";
import AddApplicationClient from "./AddApplicationClient";
import { ApplicationModel } from "../../../types/ApplicationModel";
import EditApplicationClient from "./EditApplicationClient";
import DeleteApplicationClient from "./DeleteApplicationClient";
import { ApplicationClientModel } from "../../../types/ApplicationClientModel";
import { SecretCredentials } from "../../../types/ClientAppPasswordModel";

function ApplicationClientsTab({ application }: { application: ApplicationModel }) {
  const [selectionModel, setSelectionModel] = useState<GridRowSelectionModel>([]);
  const [rows, setRows] = useState<ApplicationClientModel[]>([]);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const [dataModified, setDataModified] = useState<{ modified: Date }>({ modified: new Date() });
  const [editModel, setEditModel] = useState<ApplicationClientModel>();
  const [credentialList, setCredentialList] = useState<{ secrets: SecretCredentials, loaded: boolean }>({ secrets: {}, loaded: false });

  const refreshTable = () => {
    setDataModified({ modified: new Date() });
  };

  useEffect(() => {
    var selectedRecord = rows.find((x) => x.id === selectionModel.at(0)?.toString());
    setEditModel(structuredClone(selectedRecord));
  }, [selectionModel, rows]);

  useEffect(() => {
    async function fetchApplicationClients() {
      try {
        loader(true, 'Fetching registered clients.');
        let data = await ApplicationApiClient.GetApplicationClients(application.name);
        setRows(data);
        return data;
      } catch (e: any) {
        enqueueSnackbar("Error while fetching clients", { variant: 'error' }, e);
      }
      finally {
        loader(false);
      }
    }
    fetchApplicationClients().then(async (value: ApplicationClientModel[] | undefined) => {
      if (!value)
        return;
      try {
        setCredentialList({ secrets: {}, loaded: false });
        var secretCredentials = await ApplicationApiClient.GetClientCredentials(value.map(m => m.clientId));
        setCredentialList({ secrets: secretCredentials, loaded: true });
      } catch (e:any) {
        setCredentialList({ secrets: {}, loaded: true });
        enqueueSnackbar("Error while fetching client secrets.", { variant: 'error' }, e);
      }

    });
  }, [dataModified]);

  return (
    <>
      <LayoutGroup justify="space-between">
        <p>The table displays the list of clients registered for the application: {application.name}.</p>
        <ButtonGroup>
          <AuthHandler requiredPermissions={[AppPermissions.CanCreateApplicationClient]}>
            {selectionModel.length === 0 && <AddApplicationClient refreshData={refreshTable} application={application} />}
          </AuthHandler>

          <AuthHandler requiredPermissions={[AppPermissions.CanEditApplicationClient]}>
            {selectionModel.length === 1 && !!editModel && <EditApplicationClient refreshData={refreshTable} editModel={editModel} credentialList={credentialList} />
            }
          </AuthHandler>

          <AuthHandler requiredPermissions={[AppPermissions.CanDeleteApplicationClient]}>
            {selectionModel.length === 1 && <DeleteApplicationClient refreshData={refreshTable} selectionModel={selectionModel} />
            }
          </AuthHandler>
        </ButtonGroup>
      </LayoutGroup>

      <ApplicationClientsTable selectionModel={selectionModel} setSelectionModel={setSelectionModel} rows={rows} secretCredentials={credentialList} />
    </>
  );

}

export default ApplicationClientsTab;
