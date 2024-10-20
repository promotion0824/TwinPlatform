import { ButtonGroup, GridRowSelectionModel, Group, PageTitle, PageTitleItem } from '@willowinc/ui';
import { useCallback, useEffect, useState } from 'react';
import { useQuery } from 'react-query';
import { Link } from 'react-router-dom';
import { AppPermissions } from '../../AppPermissions';
import { AuthHandler } from '../../Components/AuthHandler';
import { useCustomSnackbar } from '../../Hooks/useCustomSnackbar';
import { useLoading } from '../../Hooks/useLoading';
import { AssignmentClient, TwinsClient } from '../../Services/AuthClient';
import { AssignmentModel } from '../../types/AssignmentModel';
import { BatchRequestDto, FilterSpecificationDto } from '../../types/BatchRequestDto';
import { ILocationTwinModel } from '../../types/SelectTreeModel';
import AssignmentAdd from './AssignmentAdd';
import AssignmentDelete from './AssignmentDelete';
import AssignmentEdit from './AssignmentEdit';
import AssignmentTable from './AssignmentTable';
import { BatchDto } from '../../types/BatchDto';
import GridQuickFilter from '../../Components/GridQuickFilter';

function AssignmentPage() {
  const [selectionModel, setSelectionModel] = useState<GridRowSelectionModel>([]);
  const loader = useLoading();
  const { enqueueSnackbar } = useCustomSnackbar();
  const [batchRequest, setBatchRequest] = useState<BatchRequestDto>(new BatchRequestDto());
  const [quickFilters, setQuickFilters] = useState<FilterSpecificationDto[]>([]);
  const { data, isFetching, refetch } = useQuery(["assignments", batchRequest, quickFilters], fetchAssignments, { initialData: new BatchDto<AssignmentModel>() });

  const [locationResponse, setLocationResponse] = useState<{ locations: ILocationTwinModel[], loaded: boolean }>({ locations: [], loaded: false });
  useEffect(() => {
    async function getLocations() {
      try {
        var resp = await TwinsClient.GetLocationTwins();
        setLocationResponse({ locations: resp, loaded: true });
      } catch (e) {
        setLocationResponse({ locations: [], loaded: true });
      }
    }
    getLocations();
  }, []);


  const getEditModel = useCallback(
    () => {
      if (!data) {
        return new AssignmentModel();
      }
      var selectedRecord = data.items.find((x) => x.id === selectionModel.at(0)?.toString());
      return !!selectedRecord ? { ...selectedRecord } : new AssignmentModel();
    },
    [selectionModel, data]);

  async function fetchAssignments() {
    try {
      // Merge Table Filters and Quick Filters
      const request = { ...batchRequest };
      request.filterSpecifications = request.filterSpecifications.concat(quickFilters);

      const userFilter = request.filterSpecifications.map((v, i) => {
        if (v.field.indexOf("UserOrGroupName") > -1) {
          return { ...v, field: v.field.replace('UserOrGroupName','User.FirstName') };
        }
        return v;
      });

      const groupFilter = request.filterSpecifications.map((v, i) => {
        if (v.field.indexOf("UserOrGroupName") > -1) {
          return { ...v, field: v.field.replace('UserOrGroupName', 'Group.Name') };
        }
        return v;
      });

      return await AssignmentClient.GetAllAssignments({ ...request, filterSpecifications: userFilter }, { ...request, filterSpecifications: groupFilter });
    } catch (e: any) {
      enqueueSnackbar("Error while fetching assignments", { variant: 'error' }, e);
    }
  }

  return (
    <AuthHandler requiredPermissions={[AppPermissions.CanReadAssignment]}>
      <Group justify="space-between">
        <PageTitle>
          <PageTitleItem><Link to="/assignments">Assignments</Link></PageTitleItem>
        </PageTitle>
        <div className="marginLeftAuto">
          <GridQuickFilter filterFieldNames={["UserOrGroupName", "Role.Name", "expression"]} setQuickFilters={setQuickFilters} />
        </div>
        <ButtonGroup>
          <AuthHandler requiredPermissions={[AppPermissions.CanCreateAssignment]}>
            {selectionModel.length === 0 && <AssignmentAdd refreshData={refetch} locations={locationResponse.locations} />}
          </AuthHandler>

          <AuthHandler requiredPermissions={[AppPermissions.CanEditAssignment]}>
            {selectionModel.length === 1 && <AssignmentEdit refreshData={refetch} getEditModel={getEditModel} locations={locationResponse.locations} />}
          </AuthHandler>
          <AuthHandler requiredPermissions={[AppPermissions.CanDeleteAssignment]}>
            {selectionModel.length === 1 && <AssignmentDelete refreshData={refetch} selectionModel={selectionModel} rows={data?.items ?? []} />}
          </AuthHandler>
        </ButtonGroup>
      </Group>
      <AssignmentTable selectionModel={selectionModel} setSelectionModel={setSelectionModel}
        batch={data}
        isFetching={isFetching}
        batchRequest={batchRequest}
        setBatchRequest={setBatchRequest}
        locationResponse={locationResponse}
      ></AssignmentTable>
    </AuthHandler>);
}
export default AssignmentPage;  
