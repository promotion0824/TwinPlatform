import { useState, Dispatch, SetStateAction } from 'react';
import { useQuery, UseQueryOptions, UseQueryResult } from 'react-query';
import { ApiException, UpdateMappedTwinRequestResponse } from '../../../services/Clients';
import useApi from '../../../hooks/useApi';

export interface IGetUpdateTwinRequests {
  query: UseQueryResult<UpdateMappedTwinRequestResponse[], ApiException>;
  pageSizeState: [number, Dispatch<SetStateAction<number>>];
  offsetState: [number, Dispatch<SetStateAction<number>>];
}

export default function useGetUpdateTwinRequests(
  options?: UseQueryOptions<UpdateMappedTwinRequestResponse[], ApiException>
): IGetUpdateTwinRequests {
  const api = useApi();

  const pageSizeState = useState<number>(100);
  const offsetState = useState<number>(0);

  const query = useQuery<UpdateMappedTwinRequestResponse[], ApiException>(
    ['update-twin-requests', offsetState[0], pageSizeState[0]],
    () => api.getUpdateTwinRequests(offsetState[0], pageSizeState[0]),
    { ...options, retry: 5 }
  );

  return {
    query,
    pageSizeState,
    offsetState,
  };
}
