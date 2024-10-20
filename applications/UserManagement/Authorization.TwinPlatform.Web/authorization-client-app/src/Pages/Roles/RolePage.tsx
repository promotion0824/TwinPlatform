import { ButtonGroup, GridRowSelectionModel, Group, PageTitle, PageTitleItem } from '@willowinc/ui';
import { useCallback, useState } from 'react';
import { useQuery } from 'react-query';
import { Link } from 'react-router-dom';
import { AppPermissions } from '../../AppPermissions';
import { AuthHandler } from '../../Components/AuthHandler';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { RoleClient } from '../../Services/AuthClient';
import { BatchDto } from '../../types/BatchDto';
import { BatchRequestDto, FilterSpecificationDto } from '../../types/BatchRequestDto';
import { RoleFieldNames, RoleModel } from '../../types/RoleModel';
import RoleAdd from './RoleAdd';
import RoleDelete from './RoleDelete';
import RoleEdit from './RoleEdit';
import RoleTable from './RoleTable';
import GridQuickFilter from '../../Components/GridQuickFilter';

function RolePage() {
  const [selectionModel, setSelectionModel] = useState<GridRowSelectionModel>([]);
  const { enqueueSnackbar } = useCustomSnackbar();
  const [batchRequest, setBatchRequest] = useState<BatchRequestDto>(new BatchRequestDto());
  const [quickFilters, setQuickFilters] = useState<FilterSpecificationDto[]>([]);
  const { data, isFetching, refetch } = useQuery(["roles", batchRequest, quickFilters], fetchRoles, { initialData: new BatchDto<RoleModel>() });

  const getEditModel = useCallback(
    () => {
      var selectedRecord = data?.items.find((x) => x.id === selectionModel.at(0)?.toString());
      return structuredClone(selectedRecord) ?? new RoleModel();
    },
    [selectionModel, data]);

  async function fetchRoles() {
    try {
      // Merge Table Filters and Quick Filters
      const request = { ...batchRequest };
      request.filterSpecifications = request.filterSpecifications.concat(quickFilters);
      return await RoleClient.GetAllRoles(request);
    } catch (e: any) {
      enqueueSnackbar('Error fetching roles', { variant: 'error' }, e);
    }
  }


  return (
    <AuthHandler requiredPermissions={[AppPermissions.CanReadRole]}>
      <Group justify="space-between">
        <PageTitle>
          <PageTitleItem><Link to="/roles">Roles</Link></PageTitleItem>
        </PageTitle>
        <div className="marginLeftAuto">
          <GridQuickFilter filterFieldNames={[RoleFieldNames.name.field, RoleFieldNames.description.field]} setQuickFilters={setQuickFilters} />
        </div>
        <ButtonGroup>
          <AuthHandler requiredPermissions={[AppPermissions.CanCreateRole]}>

            <RoleAdd refreshData={refetch} />
          </AuthHandler>
          <AuthHandler requiredPermissions={[AppPermissions.CanEditRole]}>

            {selectionModel.length == 1 && <RoleEdit refreshData={refetch} getEditModel={getEditModel} />
            }
          </AuthHandler>
          <AuthHandler requiredPermissions={[AppPermissions.CanDeleteRole]}>

            {selectionModel.length == 1 && <RoleDelete refreshData={refetch} selectionModel={selectionModel} />
            }
          </AuthHandler>
        </ButtonGroup>
      </Group>
      <RoleTable selectionModel={selectionModel} setSelectionModel={setSelectionModel} batch={data} isFetching={isFetching} batchRequest={batchRequest} setBatchRequest={setBatchRequest} />
    </AuthHandler>);
}
export default RolePage;  
