import { useState, useEffect } from 'react';
import { useQuery, UseQueryOptions } from 'react-query';
import { ValidationResultsPage, ApiException } from '../../../services/Clients';
import useApi from '../../../hooks/useApi';

export default function useGetDQResults(
  params: { errorsOnly: boolean; searchString: string; locationId: string; modelIds: string[] },
  options?: UseQueryOptions<ValidationResultsPage, ApiException>
) {
  const api = useApi();
  const [pageSize, setPageSize] = useState<number>(250);
  const [continuationToken, setContinuationToken] = useState<string>('');

  const { QueryId = '', NextPage = 0 } = continuationToken !== '' ? JSON.parse(continuationToken) : {};

  const { errorsOnly, searchString, locationId, modelIds } = params;

  const modelsIdsString = JSON.stringify(modelIds);
  const locationIdString = JSON.stringify(locationId);
  // Reset the continuation token when new filters are applied
  useEffect(() => {
    setContinuationToken('');
  }, [errorsOnly, pageSize, searchString, modelsIdsString, locationIdString]);

  const getDQResultsQuery = useQuery<ValidationResultsPage, ApiException>(
    ['getDQResults', QueryId, NextPage, errorsOnly, pageSize, searchString, locationIdString, modelsIdsString],
    () =>
      api.getDQResults(
        errorsOnly,
        modelIds,
        ['StaticDataQuality'],
        pageSize,
        continuationToken,
        searchString,
        locationId
      ),
    options
  );

  return { getDQResultsQuery, continuationToken, setContinuationToken, pageSize, setPageSize };
}
