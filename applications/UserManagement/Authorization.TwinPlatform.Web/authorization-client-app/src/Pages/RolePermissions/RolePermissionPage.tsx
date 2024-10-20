import { ButtonGroup, GridRowSelectionModel, Group, PageTitle, PageTitleItem } from '@willowinc/ui';
import { useEffect, useState } from 'react';
import { useQuery } from 'react-query';
import { useParams } from 'react-router';
import { Link } from 'react-router-dom';
import { AppPermissions } from '../../AppPermissions';
import { AuthHandler } from '../../Components/AuthHandler';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { useLoading } from '../../Hooks/useLoading';
import { PermissionClient, RoleClient } from '../../Services/AuthClient';
import { BatchDto } from '../../types/BatchDto';
import { BatchRequestDto, FilterSpecificationDto } from '../../types/BatchRequestDto';
import { PermissionFieldNames, PermissionModel } from '../../types/PermissionModel';
import { RoleModel, RoleType } from '../../types/RoleModel';
import PermissionTable from '../Permissions/PermissionTable';
import AssignPermissionToRole from './AssignPermissionToRole';
import RemovePermissionFromRole from './RemovePermissionFromRole';
import GridQuickFilter from '../../Components/GridQuickFilter';

function RolePermissionPage() {
  const [selectionModel, setSelectionModel] = useState<GridRowSelectionModel>([]);
  const [row, setRow] = useState<RoleType>(new RoleModel());
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  let { name } = useParams();
  const [batchRequest, setBatchRequest] = useState<BatchRequestDto>(new BatchRequestDto());
  const [quickFilters, setQuickFilters] = useState<FilterSpecificationDto[]>([]);
  const { data, isFetching, refetch } = useQuery(["rolePermissions", row, batchRequest, quickFilters], fetchPermissions, { initialData: new BatchDto<PermissionModel>() });

  useEffect(() => {
    async function fetchRole() {
      try {
        loader(true, 'Fetching role.');

        setRow(await RoleClient.GetRoleByName(name as string));

      } catch (e: any) {
        enqueueSnackbar("Error while fetching role.", { variant: 'error' }, e);
      }
      finally {
        loader(false);
      }
    }
    fetchRole();
  }, []);

  async function fetchPermissions() {
    try {
      if (!row) {
        return; // Return if role is not loaded
      }

      // Merge Table Filters and Quick Filters
      const request = { ...batchRequest };
      request.filterSpecifications = request.filterSpecifications.concat(quickFilters);

      const res = await PermissionClient.GetPermissionsByRole(row.id, request, false);
      return res;
    } catch (e: any) {
      enqueueSnackbar("Error while fetching Permissions", { variant: 'error' }, e);
    }
  }

  return (
    <>
      <Group justify='space-between'>
        <PageTitle>
          <PageTitleItem><Link to="/roles">Roles</Link></PageTitleItem>
          <PageTitleItem><Link to={`/roles/${row.name}`}>{row.name}</Link></PageTitleItem>
        </PageTitle>
        <div className="marginLeftAuto">
          <GridQuickFilter filterFieldNames={[PermissionFieldNames.name.field]} setQuickFilters={setQuickFilters} />
        </div>
        <ButtonGroup>
          <AuthHandler requiredPermissions={[AppPermissions.CanAssignPermission]}>
            {selectionModel.length === 0 && <AssignPermissionToRole roleModel={row} refreshData={refetch} />}
          </AuthHandler>
          <AuthHandler requiredPermissions={[AppPermissions.CanRemovePermission]}>
            {selectionModel.length === 1 && <RemovePermissionFromRole roleModel={row} refreshData={refetch} selectionModel={selectionModel} />
            }
          </AuthHandler>
        </ButtonGroup>
      </Group>
      <p>
        Role <b>{row.name} </b>has the following {row.permissions.length} permissions assigned to it.
        Click assign to add new permission or select an existing row and click remove to delete an assigned permission.
      </p>
      <PermissionTable selectionModel={selectionModel} setSelectionModel={setSelectionModel} batch={data} isFetching={isFetching} batchRequest={batchRequest} setBatchRequest={setBatchRequest} />
    </>
  );
}
export default RolePermissionPage;  
