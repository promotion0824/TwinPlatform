import { useQuery, UseQueryOptions, UseQueryResult } from 'react-query';
import { ApiException, Status } from '../../../services/Clients';
import useApi from '../../../hooks/useApi';

export default function useGetMappedEntriesCount(
  statuses: Status[] | undefined = undefined,
  prefixes: string[] | undefined = undefined,
  excludePrefixes: boolean | undefined = false,
  options?: UseQueryOptions<number, ApiException>
): UseQueryResult<number, ApiException> {
  const api = useApi();

  return useQuery<number, ApiException>(
    ['mapped-entities-count', statuses, prefixes, excludePrefixes],
    () => api.getMappedEntriesCount(statuses, prefixes, excludePrefixes),
    options
  );
}

export type IGetMappedEntriesCount = UseQueryResult<number, ApiException>;
