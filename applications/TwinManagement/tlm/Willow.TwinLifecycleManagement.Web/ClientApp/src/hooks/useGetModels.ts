import { useQuery, UseQueryOptions } from 'react-query';
import { IInterfaceTwinsInfo, ApiException, SourceType } from '../services/Clients';
import useApi from './useApi';
import useMultipleSearchParams from './useMultipleSearchParams';

export default function useGetModels(
  options?: UseQueryOptions<IInterfaceTwinsInfo[], ApiException>,
  rootModel?: string
) {
  const api = useApi();
  const [urlParams] = useMultipleSearchParams([{ name: 'source', type: 'string' }]);

  // Bandaid fix for timeout error when source is ADT, always use ADX for now, until better implementation of ADT caching
  const sourceTypeUrlParam =
    ((urlParams?.source as string) || '').toLowerCase() === 'adt' ? SourceType.Adx : SourceType.Adx;

  return useQuery<IInterfaceTwinsInfo[], ApiException>(
    ['models', sourceTypeUrlParam, rootModel],
    () => api.getModels(rootModel, sourceTypeUrlParam),
    {
      ...options,
      staleTime: Infinity,
    }
  );
}
