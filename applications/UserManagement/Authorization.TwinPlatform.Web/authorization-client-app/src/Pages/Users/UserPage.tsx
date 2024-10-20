import { ButtonGroup, GridRowSelectionModel, Group, PageTitle, PageTitleItem } from '@willowinc/ui';
import { useState, useCallback } from 'react';
import { UserFieldNames, UserModel } from '../../types/UserModel';
import UserAdd from './UserAdd';
import UserEdit from './UserEdit';
import UserDelete from './UserDelete';
import UserTable from './UserTable';
import { UserClient } from '../../Services/AuthClient';
import { AuthHandler } from '../../Components/AuthHandler';
import { AppPermissions } from '../../AppPermissions';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { Link } from 'react-router-dom';
import { BatchRequestDto, FilterSpecificationDto } from '../../types/BatchRequestDto';
import { useQuery } from 'react-query';
import { BatchDto } from '../../types/BatchDto';
import GridQuickFilter from '../../Components/GridQuickFilter';


function UserPage() {
  const [selectionModel, setSelectionModel] = useState<GridRowSelectionModel>([]);
  const { enqueueSnackbar } = useCustomSnackbar();
  const [batchRequest, setBatchRequest] = useState<BatchRequestDto>(new BatchRequestDto());
  const [quickFilters, setQuickFilters] = useState<FilterSpecificationDto[]>([]);
  const { data, isFetching, refetch } = useQuery(["users", batchRequest, quickFilters], fetchUsers, { initialData: new BatchDto<UserModel>() });

  const getEditModel = useCallback(
    () => {
      var selectedRecord = data?.items.find((x) => x.id === selectionModel.at(0)?.toString());
      return structuredClone(selectedRecord) ?? new UserModel();
    },
    [selectionModel, data]);

  async function fetchUsers() {
    try {

      // Merge Table Filters and Quick Filters
      const request = { ...batchRequest };
      request.filterSpecifications = request.filterSpecifications.concat(quickFilters);

      const res = await UserClient.GetAllUsers(request);
      return res;
    } catch (e: any) {
      enqueueSnackbar("Error while fetching users", { variant: 'error' }, e);
    }
  }

  return (
    <AuthHandler requiredPermissions={[AppPermissions.CanReadUser]}>
      <Group justify="right">
        <PageTitle>
          <PageTitleItem><Link to="/users">Users</Link></PageTitleItem>
        </PageTitle>
        <div className="marginLeftAuto">
          <GridQuickFilter filterFieldNames={[UserFieldNames.email.field, UserFieldNames.firstName.field, UserFieldNames.lastName.field]} setQuickFilters={setQuickFilters} />
        </div>
        <ButtonGroup>
          <AuthHandler requiredPermissions={[AppPermissions.CanCreateUser]}>
            {selectionModel.length === 0 && <UserAdd refreshData={refetch} />}
          </AuthHandler>

          <AuthHandler requiredPermissions={[AppPermissions.CanEditUser]}>
            {selectionModel.length === 1 && <UserEdit refreshData={refetch} getEditModel={getEditModel} />
            }
          </AuthHandler>

          <AuthHandler requiredPermissions={[AppPermissions.CanDeleteUser]}>
            {selectionModel.length === 1 && <UserDelete refreshData={refetch} selectionModel={selectionModel} />
            }
          </AuthHandler>
        </ButtonGroup>
      </Group>
      <UserTable selectionModel={selectionModel} setSelectionModel={setSelectionModel} batch={data} isFetching={isFetching} batchRequest={batchRequest} setBatchRequest={setBatchRequest} />
    </AuthHandler>);
}
export default UserPage;  
