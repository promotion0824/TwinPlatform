import { useQuery, UseQueryOptions, UseQueryResult } from '@tanstack/react-query';
import useApi from '../../../hooks/useApi';
import { ApiException, GetConflictingCommandPresentValuesRequestDto, GetConflictingCommandPresentValuesResponseDto } from '../../../services/Clients';

export default function usePostRequestedCommandPresentValue(
  getConflictingCommandPresentValuesRequestDto: GetConflictingCommandPresentValuesRequestDto,
  options?: UseQueryOptions<GetConflictingCommandPresentValuesResponseDto, ApiException>
) {
  const api = useApi();

  return useQuery<any, any>({
    queryKey: ['postRequestedCommandPresentValue', getConflictingCommandPresentValuesRequestDto],
    queryFn: () => api.getRequestedCommandsPresentValues(getConflictingCommandPresentValuesRequestDto),
    ...options,
    enabled:  getConflictingCommandPresentValuesRequestDto.externalIds && getConflictingCommandPresentValuesRequestDto.externalIds.length > 0
  });
}
