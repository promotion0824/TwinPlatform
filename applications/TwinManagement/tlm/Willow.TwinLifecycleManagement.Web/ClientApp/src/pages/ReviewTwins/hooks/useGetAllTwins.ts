import { useQuery, UseQueryOptions } from 'react-query';
import { ApiException, SourceType, TwinWithRelationships } from '../../../services/Clients';
import useApi from '../../../hooks/useApi';

export default function useGetAllTwins(
  willowModelId: string[] | undefined,
  options?: UseQueryOptions<TwinWithRelationships[], ApiException, any>
) {
  const api = useApi();

  return useQuery<TwinWithRelationships[], ApiException, TwinWithRelationships[]>(
    ['get-all-twins', willowModelId],
    () => api.getAllTwins(willowModelId, SourceType.AdtQuery),
    { ...options }
  );
}
