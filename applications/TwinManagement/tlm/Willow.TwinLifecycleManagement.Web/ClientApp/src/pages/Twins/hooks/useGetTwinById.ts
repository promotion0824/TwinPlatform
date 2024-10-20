import { useQuery, UseQueryOptions } from 'react-query';
import { ApiException, SourceType, ITwinWithRelationships } from '../../../services/Clients';
import useApi from '../../../hooks/useApi';
import useMultipleSearchParams from '../../../hooks/useMultipleSearchParams';

export default function useGetTwinById(
  twinId: string,
  options?: UseQueryOptions<ITwinWithRelationships, ApiException>,
  includeRelationshipsParam?: boolean,
  source?: SourceType
) {
  const api = useApi();
  const [urlParams] = useMultipleSearchParams([{ name: 'source', type: 'string' }]);

  const sourceUrlParam = (urlParams?.source || '') as string;

  const isAdtSource = sourceUrlParam.toLowerCase() === 'adt';

  const isAdxSource = sourceUrlParam.toLowerCase() === 'adx';

  const params = {
    includeRelationships: includeRelationshipsParam ?? true,
    sourceType: source ? source : isAdtSource ? SourceType.AdtQuery : isAdxSource ? SourceType.Adx : SourceType.Adx,
  };

  const { includeRelationships, sourceType } = params;

  return useQuery<ITwinWithRelationships, ApiException>(
    ['twinById', twinId, includeRelationships, sourceType],
    () => api.getTwinById(twinId, sourceType, includeRelationships),
    options
  );
}
