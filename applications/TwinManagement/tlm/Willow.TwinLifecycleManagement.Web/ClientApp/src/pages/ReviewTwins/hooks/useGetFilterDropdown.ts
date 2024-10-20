import { useQuery, UseQueryOptions } from 'react-query';
import { ApiException, CombinedMappedEntriesGroupCount } from '../../../services/Clients';
import useApi from '../../../hooks/useApi';

export default function useGetFilterDropdown(
  options?: UseQueryOptions<CombinedMappedEntriesGroupCount, ApiException, any>
) {
  const api = useApi();

  return useQuery<CombinedMappedEntriesGroupCount, ApiException, CombinedMappedEntriesGroupCount>(
    ['get-ApproveAndAccept-filters-dropdown'],
    () => api.getFilterDropdown(),
    { ...options }
  );
}
