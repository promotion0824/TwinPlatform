import { useQuery, UseQueryOptions } from 'react-query';
import { ApiException, SourceType } from '../../../services/Clients';
import useApi from '../../../hooks/useApi';

export default function useGetTwinsCount(
  params: {
    modelIds: string[];
    locationId: string;
    exactModelMatch?: boolean;
    sourceType?: SourceType;
    searchString?: string;
    isIncrementalScan?: boolean;
  },
  options?: UseQueryOptions<number, ApiException>
) {
  const api = useApi();

  const { modelIds, locationId, exactModelMatch = false, sourceType, searchString, isIncrementalScan = false } = params;

  return useQuery<number, ApiException>(
    ['getTwinCount', modelIds, locationId, isIncrementalScan],
    () => api.getTwinsCount(modelIds, locationId, exactModelMatch, sourceType, searchString, isIncrementalScan),
    options
  );
}
