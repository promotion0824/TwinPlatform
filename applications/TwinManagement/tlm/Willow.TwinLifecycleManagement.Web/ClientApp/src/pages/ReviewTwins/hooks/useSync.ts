import { useMutation, UseMutationOptions } from 'react-query';
import useApi from '../../../hooks/useApi';

export default function useSync(options?: UseMutationOptions<any, any, any>) {
  const api = useApi();

  const syncOrganizationMutation = useMutation(
    ({ autoApprove }: { autoApprove: boolean }) => api.syncOrganization(autoApprove),
    options
  );
  const syncSpatialResourcesMutation = useMutation(
    ({ autoApprove, buildingIds }: { autoApprove: boolean; buildingIds: string[] }) =>
      api.syncSpatial(autoApprove, buildingIds),
    options
  );

  const syncAssetsMutation = useMutation(
    ({ autoApprove, buildingIds, connectorId }) => api.syncAssets(connectorId, autoApprove, buildingIds),
    options
  );
  const syncCapabilitiesMutation = useMutation(
    ({ autoApprove, matchStdPntList, buildingIds, connectorId }) =>
      api.syncCapabilities(connectorId, autoApprove, matchStdPntList, buildingIds),
    options
  );

  return {
    syncOrganizationMutation,
    syncSpatialResourcesMutation,
    syncAssetsMutation,
    syncCapabilitiesMutation,
  };
}
