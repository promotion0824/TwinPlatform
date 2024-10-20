import { ButtonGroup, GridRowSelectionModel, Group as LayoutGroup, PageTitle, PageTitleItem } from '@willowinc/ui';
import { useCallback, useState } from 'react';
import { useQuery } from 'react-query';
import { Link } from 'react-router-dom';
import { AppPermissions } from '../../AppPermissions';
import { AuthHandler } from '../../Components/AuthHandler';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { GroupClient } from '../../Services/AuthClient';
import { BatchDto } from '../../types/BatchDto';
import { BatchRequestDto, FilterSpecificationDto } from '../../types/BatchRequestDto';
import { GroupModel } from '../../types/GroupModel';
import GroupAdd from './GroupAdd';
import GroupDelete from './GroupDelete';
import GroupEdit from './GroupEdit';
import GroupTable from './GroupTable';
import GridQuickFilter from '../../Components/GridQuickFilter';

function GroupPage() {
  const [selectionModel, setSelectionModel] = useState<GridRowSelectionModel>([]);
  const { enqueueSnackbar } = useCustomSnackbar();
  const [batchRequest, setBatchRequest] = useState<BatchRequestDto>(new BatchRequestDto());
  const [quickFilters, setQuickFilters] = useState<FilterSpecificationDto[]>([]);
  const { data, isFetching, refetch } = useQuery(["groups", batchRequest, quickFilters], fetchGroups, { initialData: new BatchDto<GroupModel>() });

  const getEditModel = useCallback(
    () => {
      var selectedRecord = data!.items.find((x) => x.id === selectionModel.at(0)?.toString());
      return structuredClone(selectedRecord) ?? new GroupModel();
    },
    [selectionModel, data]);

  async function fetchGroups() {
    try {
      // Merge Table Filters and Quick Filters
      const request = { ...batchRequest };
      request.filterSpecifications = request.filterSpecifications!.concat(quickFilters);
      return await GroupClient.GetAllGroups(request);
    } catch (e: any) {
      enqueueSnackbar("Error while fetching groups.", { variant: 'error' }, e);
    }
  }


  return (
    <AuthHandler requiredPermissions={[AppPermissions.CanReadGroup]}>
      <LayoutGroup justify="space-between">
        <PageTitle>
          <PageTitleItem><Link to="/groups">Groups</Link></PageTitleItem>
        </PageTitle>
        <div className="marginLeftAuto">
          <GridQuickFilter filterFieldNames={["name"]} setQuickFilters={setQuickFilters} />
        </div>
        <ButtonGroup>
          <AuthHandler requiredPermissions={[AppPermissions.CanCreateGroup]}>
            {selectionModel.length === 0 && <GroupAdd refreshData={refetch} />}
          </AuthHandler>

          <AuthHandler requiredPermissions={[AppPermissions.CanEditGroup]}>
            {selectionModel.length === 1 && <GroupEdit refreshData={refetch} getEditModel={getEditModel} />
            }
          </AuthHandler>

          <AuthHandler requiredPermissions={[AppPermissions.CanDeleteGroup]}>
            {selectionModel.length === 1 && <GroupDelete refreshData={refetch} selectionModel={selectionModel} />
            }
          </AuthHandler>
        </ButtonGroup>
      </LayoutGroup>
      <GroupTable selectionModel={selectionModel} setSelectionModel={setSelectionModel} batch={data} isFetching={isFetching} batchRequest={batchRequest} setBatchRequest={setBatchRequest} />
    </AuthHandler>);
}
export default GroupPage;  
