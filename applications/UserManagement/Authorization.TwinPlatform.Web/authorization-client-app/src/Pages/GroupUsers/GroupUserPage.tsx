import { Alert } from '@mui/material';
import { ButtonGroup, GridRowSelectionModel, Group as LayoutGroup, PageTitle, PageTitleItem } from '@willowinc/ui';
import { useEffect, useState } from 'react';
import { useQuery } from 'react-query';
import { useParams } from 'react-router';
import { Link } from 'react-router-dom';
import { AppPermissions } from '../../AppPermissions';
import { AuthHandler } from '../../Components/AuthHandler';
import GridQuickFilter from '../../Components/GridQuickFilter';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { useLoading } from '../../Hooks/useLoading';
import { GroupClient, UserClient } from '../../Services/AuthClient';
import { BatchDto } from '../../types/BatchDto';
import { BatchRequestDto, FilterSpecificationDto } from '../../types/BatchRequestDto';
import { Group } from '../../types/GroupModel';
import { UserModel } from '../../types/UserModel';
import UserTable from '../Users/UserTable';
import AssignUserToGroup from './AssignUserToGroup';
import RemoveUserFromGroup from './RemoveUserFromGroup';

function GroupUserPage() {
  const [selectionModel, setSelectionModel] = useState<GridRowSelectionModel>([]);
  const [row, setRow] = useState<Group | null>(null);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  let { name } = useParams();
  let isAppGroup = row?.groupType?.name?.toLocaleLowerCase() == 'application';
  const [batchRequest, setBatchRequest] = useState<BatchRequestDto>(new BatchRequestDto());
  const [quickFilters, setQuickFilters] = useState<FilterSpecificationDto[]>([]);
  const { data, isFetching, refetch } = useQuery(["groupUsers", row, batchRequest, quickFilters], fetchUsers, { initialData: new BatchDto<UserModel>() });

  useEffect(() => {
    async function fetchGroup() {
      try {
        loader(true, 'Fetching group users.');

        setRow(await GroupClient.GetGroupbyName(name as string));

      } catch (e: any) {
        enqueueSnackbar("Error while fetching groups.", { variant: 'error' }, e);
      }
      finally {
        loader(false);
      }
    }
    fetchGroup();
  }, []);

  async function fetchUsers() {
    try {

      if (!row || !isAppGroup) {
        return; // Return if group is not loaded
      }

      // Merge Table Filters and Quick Filters
      const request = { ...batchRequest };
      request.filterSpecifications = request.filterSpecifications.concat(quickFilters);

      const res = await UserClient.GetUsersByGroup(row.id, request, false);
      return res;
    } catch (e: any) {
      enqueueSnackbar("Error while fetching users", { variant: 'error' }, e);
    }
  }

  return (

    <>
      {row == null || <>
        <LayoutGroup justify="space-between">
          <PageTitle>
            <PageTitleItem><Link to="/groups">Groups</Link></PageTitleItem>
            <PageTitleItem><Link to={`/groups/${row?.name}`}>{row?.name}</Link></PageTitleItem>
          </PageTitle>
          {isAppGroup &&
            <>
              <div className="marginLeftAuto">
                <GridQuickFilter filterFieldNames={["email", "firstName", "lastName"]} setQuickFilters={setQuickFilters} />
              </div>
              <ButtonGroup>
                <AuthHandler requiredPermissions={[AppPermissions.CanAssignUser]}>
                  <AssignUserToGroup groupModel={row!} refreshData={refetch} />
                </AuthHandler>
                <AuthHandler requiredPermissions={[AppPermissions.CanRemoveUser]}>
                  {selectionModel.length == 1 && <RemoveUserFromGroup groupModel={row!} refreshData={refetch} selectionModel={selectionModel} />
                  }
                </AuthHandler>
              </ButtonGroup>
            </>
          }
        </LayoutGroup>
        <div>
          {
            isAppGroup ?
              <p>Group<b>&nbsp;{row?.name} </b> has the following {row?.users.length} user(s) assigned to it.
                Click assign to add new user or select an existing row and click remove to delete an existing user.
              </p>
              :
              <Alert icon={false} severity="warning">
                AD group modification is not allowed. Update group user membership via azure active directory.
              </Alert>
          }
        </div>
        <UserTable selectionModel={selectionModel} setSelectionModel={setSelectionModel} batch={data} isFetching={isFetching} batchRequest={batchRequest} setBatchRequest={setBatchRequest} />
      </>}
    </>
  );
}
export default GroupUserPage;  
