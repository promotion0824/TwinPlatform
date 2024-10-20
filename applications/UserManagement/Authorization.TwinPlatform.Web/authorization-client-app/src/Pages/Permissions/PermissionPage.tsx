import { ButtonGroup, GridRowSelectionModel, Group, PageTitle, PageTitleItem } from '@willowinc/ui';
import { useCallback, useState } from 'react';
import { useQuery } from 'react-query';
import { Link } from 'react-router-dom';
import { AppPermissions } from '../../AppPermissions';
import { AuthHandler } from '../../Components/AuthHandler';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { PermissionClient } from '../../Services/AuthClient';
import { BatchDto } from '../../types/BatchDto';
import { BatchRequestDto, FilterSpecificationDto } from '../../types/BatchRequestDto';
import { PermissionFieldNames, PermissionModel } from '../../types/PermissionModel';
import PermissionAdd from './PermissionAdd';
import PermissionDelete from './PermissionDelete';
import PermissionEdit from './PermissionEdit';
import PermissionTable from './PermissionTable';
import GridQuickFilter from '../../Components/GridQuickFilter';

function PermissionPage() {
  const [selectionModel, setSelectionModel] = useState<GridRowSelectionModel>([]);
  const { enqueueSnackbar } = useCustomSnackbar();
  const [batchRequest, setBatchRequest] = useState<BatchRequestDto>(new BatchRequestDto());
  const [quickFilters, setQuickFilters] = useState<FilterSpecificationDto[]>([]);
  const { data, isFetching, refetch } = useQuery(["permissions", batchRequest, quickFilters], fetchPermissions, { initialData: new BatchDto<PermissionModel>() });

  const getEditModel = useCallback(
    () => {
      var selectedRecord = data?.items.find((x) => x.id === selectionModel.at(0)?.toString());
      return structuredClone(selectedRecord) ?? new PermissionModel();
    },
    [selectionModel, data]);

  async function fetchPermissions() {
    try {
      // Merge Table Filters and Quick Filters
      const request = { ...batchRequest };
      request.filterSpecifications = request.filterSpecifications.concat(quickFilters);

      return await PermissionClient.GetAllPermissions(request);
    } catch (e: any) {
      enqueueSnackbar("Error while fetching permissions", { variant: 'error' }, e);
    }
  }


  return (
    <AuthHandler requiredPermissions={[AppPermissions.CanReadPermission]}>
      <Group justify="space-between">
        <PageTitle>
          <PageTitleItem><Link to="/permissions">Permissions</Link></PageTitleItem>
        </PageTitle>
        <div className="marginLeftAuto">
          <GridQuickFilter filterFieldNames={[PermissionFieldNames.name.field]} setQuickFilters={setQuickFilters} />
        </div>
        <ButtonGroup>
          <AuthHandler requiredPermissions={[AppPermissions.CanCreatePermission]}>
            {selectionModel.length === 0 && <PermissionAdd refreshData={refetch} />}
          </AuthHandler>

          <AuthHandler requiredPermissions={[AppPermissions.CanEditPermission]}>
            {selectionModel.length === 1 && <PermissionEdit refreshData={refetch} getEditModel={getEditModel} />
            }
          </AuthHandler>

          <AuthHandler requiredPermissions={[AppPermissions.CanDeletePermission]}>
            {selectionModel.length === 1 && <PermissionDelete refreshData={refetch} selectionModel={selectionModel} />
            }
          </AuthHandler>
        </ButtonGroup>
      </Group>
      <PermissionTable selectionModel={selectionModel} setSelectionModel={setSelectionModel} batch={data} isFetching={isFetching} batchRequest={batchRequest} setBatchRequest={setBatchRequest} />
    </AuthHandler>);
}
export default PermissionPage;  
