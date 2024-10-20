import { useQuery, UseQueryOptions } from 'react-query';
import { ApiException, INestedTwin } from '../services/Clients';
import useApi from './useApi';

export default function useLocations(
  modelIds: string[] = ['dtmi:com:willowinc:Building;1', 'dtmi:com:willowinc:Substructure;1'],
  exactModelMatch: boolean = false,
  options?: UseQueryOptions<any, ApiException>
) {
  const api = useApi();
  const { outgoingRelationships, incomingRelationships } = {
    outgoingRelationships: ['isPartOf', 'locatedIn'],
    incomingRelationships: []
  };
  return useQuery<INestedTwin[], ApiException>(
    ['locations', modelIds],
    () => api.getTwinsTree(modelIds, outgoingRelationships, incomingRelationships, exactModelMatch),
    {
      ...options,
      staleTime: 5 * 60 * 1000, // 5 minutes
    }
  );
}
