import { ButtonGroup, GridRowSelectionModel, Group as LayoutGroup } from "@willowinc/ui";
import { useEffect, useState } from "react";
import { useCustomSnackbar } from "../../../Hooks/useCustomSnackbar";
import { useLoading } from "../../../Hooks/useLoading";
import { ApplicationModel } from "../../../types/ApplicationModel";
import { ClientAssignmentModel } from "../../../types/ClientAssignmentModel";
import { AssignmentClient } from "../../../Services/AuthClient";
import ClientAssignmentsTable from "./ClientAssignmentsTable";
import { AuthHandler } from "../../../Components/AuthHandler";
import { AppPermissions } from "../../../AppPermissions";
import ClientAssignmentAdd from "./ClientAssignmentAdd";
import { PermissionModel } from "../../../types/PermissionModel";
import ClientAssignmentEdit from "./ClientAssignmentEdit";
import ClientAssignmentDelete from "./ClientAssignmentDelete";

function ClientAssignmentsTab({ application, appPermissions }: { application: ApplicationModel, appPermissions: PermissionModel[] }) {
  const [selectionModel, setSelectionModel] = useState<GridRowSelectionModel>([]);
  const [rows, setRows] = useState<ClientAssignmentModel[]>([]);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const [dataModified, setDataModified] = useState<{ modified: Date }>({ modified: new Date() });
  const [editModel, setEditModel] = useState<ClientAssignmentModel>();

  const refreshTable = () => {
    setDataModified({ modified: new Date() });
  };

  useEffect(() => {
    var selectedRecord = rows.find((x) => x.id === selectionModel.at(0)?.toString());
    setEditModel(structuredClone(selectedRecord));
  }, [selectionModel, rows]);

  useEffect(() => {
    async function fetchClientAssignments() {
      try {
        loader(true, 'Fetching client assignments.');
        setRows(await AssignmentClient.GetClientAssignments(application.name));

      } catch (e: any) {
        enqueueSnackbar("Error while fetching client assignments.", { variant: 'error' }, e);
      }
      finally {
        loader(false);
      }
    }
    fetchClientAssignments();
  }, [dataModified]);

  return (
    <>
      <LayoutGroup justify="space-between">
        <p>The table displays the list of client permission assignments for the application: {application.name}.</p>

        <ButtonGroup>
          <AuthHandler requiredPermissions={[AppPermissions.CanCreateClientAssignment]}>
            {selectionModel.length === 0 && <ClientAssignmentAdd refreshData={refreshTable} application={application} applicationPermissions={appPermissions} />
            }
          </AuthHandler>
          <AuthHandler requiredPermissions={[AppPermissions.CanEditClientAssignment]}>
            {selectionModel.length === 1 && !!editModel && <ClientAssignmentEdit editModel={editModel} refreshData={refreshTable} application={application} applicationPermissions={appPermissions} />
            }
          </AuthHandler>

          <AuthHandler requiredPermissions={[AppPermissions.CanDeleteClientAssignment]}>
            {selectionModel.length === 1 && <ClientAssignmentDelete selectionModel={selectionModel} refreshData={refreshTable} />
            }
          </AuthHandler>
        </ButtonGroup>

      </LayoutGroup>

      <ClientAssignmentsTable selectionModel={selectionModel} setSelectionModel={setSelectionModel} rows={rows} />
    </>
  );

}

export default ClientAssignmentsTab;
