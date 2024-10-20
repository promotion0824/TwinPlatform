import { useState, Dispatch, SetStateAction } from 'react';
import { useQuery, UseQueryOptions, UseQueryResult } from 'react-query';
import {
  ApiException,
  MappedEntryRequest,
  MappedEntryResponse,
  FilterSpecificationDto,
} from '../../../services/Clients';
import useApi from '../../../hooks/useApi';

export type SelectType = { value?: string; label?: string };
export interface IGetMappedEntries {
  query: UseQueryResult<MappedEntryResponse, ApiException>;
  pageSizeState: [number, Dispatch<SetStateAction<number>>];
  offsetState: [number, Dispatch<SetStateAction<number>>];
}

export default function useGetMappedEntries(
  prefixes: string[] | undefined = undefined,
  excludePrefixes: boolean | undefined = false,
  buildingIds: string[] = [],
  connectorId?: string | null,
  options?: UseQueryOptions<MappedEntryResponse, ApiException, any>
): IGetMappedEntries {
  const api = useApi();

  const pageSizeState = useState<number>(100);
  const offsetState = useState<number>(0);

  const request = new MappedEntryRequest();
  request.prefixToMatchId = prefixes;
  request.excludePrefixes = excludePrefixes;
  request.pageSize = pageSizeState[0];
  request.offset = offsetState[0];
  request.filterSpecifications = getFilterSpecificationsRequest(buildingIds, connectorId);

  // sort the building ids to ensure the query key is consistent
  const sortedBuildingIdsQueryKey = buildingIds.sort();
  const query = useQuery<MappedEntryResponse, ApiException, any>(
    [
      'mapped-entities',
      offsetState[0],
      pageSizeState[0],
      prefixes,
      excludePrefixes,
      sortedBuildingIdsQueryKey,
      connectorId,
    ],
    () => api.getMappedEntries(request),
    { ...options, retry: 5 }
  );

  return {
    query,
    pageSizeState,
    offsetState,
  };
}

function getFilterSpecificationsRequest(buildingIds: string[], connectorId?: string | null): FilterSpecificationDto[] {
  const filters = [];

  if (buildingIds?.length > 0) {
    filters.push(
      new FilterSpecificationDto({
        field: 'buildingId',
        value: buildingIds,
        //logicalOperator: 'AND',
        operator: 'in',
      })
    );
  }

  if (connectorId) {
    filters.push(
      new FilterSpecificationDto({
        field: 'connectorId',
        operator: 'equals',
        value: connectorId,
        //logicalOperator: 'AND',
      })
    );
  }

  return filters;
}
