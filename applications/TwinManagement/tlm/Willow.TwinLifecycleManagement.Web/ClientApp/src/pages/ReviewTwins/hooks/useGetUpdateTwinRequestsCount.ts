import { useQuery, UseQueryOptions, UseQueryResult } from 'react-query';
import { ApiException } from '../../../services/Clients';
import useApi from '../../../hooks/useApi';

export default function useGetUpdateTwinRequestsCount(
  options?: UseQueryOptions<number, ApiException>
): UseQueryResult<number, ApiException> {
  const api = useApi();

  return useQuery<number, ApiException>(
    ['update-twin-requests-count'],
    () => api.getUpdateTwinRequestsCount(),
    options
  );
}
