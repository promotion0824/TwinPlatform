import { useState } from "react";
import { PermissionFieldNames, PermissionModel } from "../../../types/PermissionModel";
import { GridRowSelectionModel } from "@willowinc/ui";
import PermissionTable from "../../Permissions/PermissionTable";
import { BatchRequestDto, FilterSpecificationDto } from "../../../types/BatchRequestDto";
import { useQuery } from "react-query";
import { BatchDto } from "../../../types/BatchDto";
import { PermissionClient } from "../../../Services/AuthClient";
import { useCustomSnackbar } from "../../../Hooks/useCustomSnackbar";
import { ApplicationModel } from "../../../types/ApplicationModel";

export default function ApplicationPermissionsTab({ application }: { application: ApplicationModel }) {
  const [selectionModel, setSelectionModel] = useState<GridRowSelectionModel>([]);
  const [batchRequest, setBatchRequest] = useState<BatchRequestDto>(new BatchRequestDto());
  const { data, isFetching } = useQuery(["appPermissions", application, batchRequest], fetchPermissions, { initialData: new BatchDto<PermissionModel>() });
  const { enqueueSnackbar } = useCustomSnackbar();

  async function fetchPermissions() {
    try {
      if (!application) {
        return; // Return if role is not loaded
      }

      // Merge Table Filters and Quick Filters
      const request = { ...batchRequest };
      const sysFilter = new FilterSpecificationDto(PermissionFieldNames.applicationId.field, "=", application.id, "AND");
      request.filterSpecifications =
        request.filterSpecifications!.concat(sysFilter);
      
      const res = await PermissionClient.GetAllPermissions(request);
      return res;
    } catch (e: any) {
      enqueueSnackbar("Error while fetching Permissions", { variant: 'error' }, e);
    }
  }

  return (
    <>
      <p>The table displays the list of Permissions associated with the application: {application.name}.</p>
      <PermissionTable selectionModel={selectionModel} setSelectionModel={setSelectionModel} batch={data!} isFetching={isFetching} batchRequest={batchRequest} setBatchRequest={setBatchRequest} />
    </>
  );

}
