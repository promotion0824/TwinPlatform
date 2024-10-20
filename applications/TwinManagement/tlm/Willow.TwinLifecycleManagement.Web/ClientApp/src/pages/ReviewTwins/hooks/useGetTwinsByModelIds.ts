import { useState, useEffect, Dispatch, SetStateAction } from 'react';
import { useQuery, UseQueryOptions, UseQueryResult } from 'react-query';
import {
  ApiException,
  SourceType,
  ITwinWithRelationshipsPage,
  GetTwinsInfoRequestBFF,
  QueryFilter,
} from '../../../services/Clients';
import useApi from '../../../hooks/useApi';

export interface IGetTwins {
  query: UseQueryResult<ITwinWithRelationshipsPage, ApiException>;
  continuationToken: string;
  setContinuationToken: Dispatch<SetStateAction<string>>;
  pageState: [number, Dispatch<SetStateAction<number>>];
  setContinuationTokenCache: (map: Map<number, string>) => void;
  totalRecordsCount: number;
  pageSize: number;
}

export default function useGetTwinsByModelIds(
  modelIds: string[],
  locationId: string,
  options?: UseQueryOptions<ITwinWithRelationshipsPage, ApiException>
): IGetTwins {
  const api = useApi();
  const pageState = useState<number>(0);
  const [continuationToken, setContinuationToken] = useState<string>('');
  const [continuationTokenCache, setContinuationTokenCache] = useState(new Map<number, string>());

  const params = {
    exactModelMatch: false,
    includeRelationships: false,
    includeModelDefinition: false,
    includeIncomingRelationships: false,

    includeTotalCount: true,
    sourceType: SourceType.AdtQuery,
    pageSize: 100,
  };

  const {
    pageSize,
    exactModelMatch,
    includeRelationships,
    includeIncomingRelationships,
    sourceType,
    includeTotalCount,
  } = params;

  let twinsRequest = new GetTwinsInfoRequestBFF();
  twinsRequest.modelId = modelIds;
  twinsRequest.exactModelMatch = exactModelMatch;
  twinsRequest.includeRelationships = includeRelationships;
  twinsRequest.includeIncomingRelationships = includeIncomingRelationships;
  twinsRequest.sourceType = sourceType;
  twinsRequest.queryFilter = new QueryFilter({
    filter: "(NOT IS_DEFINED(twins.externalID) or twins.externalID = '')",
  });
  twinsRequest.locationId = locationId;

  const { QueryId = '', NextPage = 0 } = continuationToken !== '' ? JSON.parse(continuationToken) : {};

  let continuationTokenParam = continuationToken;

  switch (sourceType) {
    case SourceType.AdtQuery:
      if (!continuationTokenCache.has(NextPage)) {
        continuationTokenCache.set(NextPage, continuationToken);
      } else {
        continuationTokenParam = continuationTokenCache.get(NextPage) || '';
      }
      break;
  }

  const query = useQuery<ITwinWithRelationshipsPage, ApiException>(
    ['twins-by-modelIds', QueryId, NextPage, modelIds, continuationTokenParam, locationId],
    () => api.queryTwins(pageSize, continuationTokenParam, includeTotalCount, twinsRequest),
    options
  );

  const { data, isSuccess } = query;

  let parsedContinuationToken = data?.continuationToken && JSON.parse(data.continuationToken);
  // Modify twins query's continuation token when page is changed
  useEffect(() => {
    if (!!parsedContinuationToken) {
      parsedContinuationToken.NextPage = pageState[0];
      setContinuationToken(JSON.stringify(parsedContinuationToken));
    } else if (isSuccess) {
      // case when we're on the last page, endpoint does not return continuationToken, so use previous state to get the previous page
      setContinuationToken((prevState: string) => {
        let parsedContinuationToken = prevState && JSON.parse(prevState);
        // there are times when there is no twins, parsedContinuationToken is empty string - it will passed as null to the endpoint
        if (typeof parsedContinuationToken === 'object') parsedContinuationToken.NextPage = pageState[0];
        return JSON.stringify(parsedContinuationToken);
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pageState[0]]);

  const { Total = 0 } = parsedContinuationToken || {};

  return {
    query,
    continuationToken,
    setContinuationToken,
    pageState,
    setContinuationTokenCache,
    totalRecordsCount: Total,
    pageSize,
  };
}
