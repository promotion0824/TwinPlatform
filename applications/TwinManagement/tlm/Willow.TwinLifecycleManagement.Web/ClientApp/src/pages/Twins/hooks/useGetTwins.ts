import { useState, useEffect, Dispatch, SetStateAction, useMemo } from 'react';
import { useQuery, UseQueryOptions, UseQueryResult } from 'react-query';
import {
  ApiException,
  IInterfaceTwinsInfo,
  SourceType,
  ITwinWithRelationshipsPage,
  NestedTwin,
  GetTwinsInfoRequestBFF,
  FilterSpecificationDto,
} from '../../../services/Clients';
import useApi from '../../../hooks/useApi';
import useMultipleSearchParams from '../../../hooks/useMultipleSearchParams';
import useDebounce from '../../../utils/useDebounce';

export interface IGetTwins {
  query: UseQueryResult<ITwinWithRelationshipsPage, ApiException>;
  continuationToken: string;
  setContinuationToken: Dispatch<SetStateAction<string>>;
  pageSize: number;
  setPageSize: (size: number) => void;
  setContinuationTokenCache: (map: Map<number, string>) => void;
  filtersStates: any;
  sourceType: SourceType;
}

export default function useGetTwins(options?: UseQueryOptions<ITwinWithRelationshipsPage, ApiException>): IGetTwins {
  const api = useApi();
  const [pageSize, setPageSize] = useState<number>(100);
  const [continuationToken, setContinuationToken] = useState<string>('');
  const [continuationTokenCache, setContinuationTokenCache] = useState(new Map<number, string>());

  const selectedOrphanState = useState<boolean>(false);
  const selectedModelState = useState<IInterfaceTwinsInfo[]>([]);
  const selectedLocationState = useState<NestedTwin | null>(null);
  const searchTextState = useState('');
  const filterSpecificationsState = useState<FilterSpecificationDto[]>();

  const debouncedSearchText = useDebounce(searchTextState[0], 500);

  const [urlParams] = useMultipleSearchParams([{ name: 'source', type: 'string' }]);

  let sourceTypeUrlParam: SourceType;
  switch (((urlParams?.source as string) || '').toLowerCase()) {
    case 'adt':
      sourceTypeUrlParam = SourceType.AdtQuery;
      break;
    case 'acs':
      sourceTypeUrlParam = SourceType.Acs;
      break;
    default:
      sourceTypeUrlParam = SourceType.Adx;
  }

  const memoFilterSpecifications = useMemo(
    () => mapFilterSpecifications(filterSpecificationsState[0], sourceTypeUrlParam),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [filterSpecificationsState[0], sourceTypeUrlParam]
  );

  const params = {
    modelIds: selectedModelState[0].map((x) => x.id ?? ''),
    locationId: selectedLocationState[0]?.twin?.$dtId || '',
    exactModelMatch: false,
    includeRelationships: false,
    includeModelDefinition: false,
    includeIncomingRelationships: false,
    orphanOnly: selectedOrphanState[0],
    includeTotalCount: true,
    searchText: debouncedSearchText,
    sourceType: sourceTypeUrlParam,
    filterSpecifications: memoFilterSpecifications,
  };

  const {
    modelIds,
    locationId,
    exactModelMatch,
    includeRelationships,
    includeIncomingRelationships,
    orphanOnly,
    searchText,
    sourceType,
    includeTotalCount,
    filterSpecifications,
  } = params;

  let twinsRequest = new GetTwinsInfoRequestBFF();
  twinsRequest.modelId = modelIds;
  twinsRequest.locationId = locationId;
  twinsRequest.exactModelMatch = exactModelMatch;
  twinsRequest.includeRelationships = includeRelationships;
  twinsRequest.includeIncomingRelationships = includeIncomingRelationships;
  twinsRequest.orphanOnly = orphanOnly;
  twinsRequest.searchString = searchText;
  twinsRequest.sourceType = sourceType;
  twinsRequest.filterSpecifications = filterSpecifications;

  const { QueryId = '', NextPage = 0 } = continuationToken !== '' ? JSON.parse(continuationToken) : {};

  const modelsIdsString = JSON.stringify(modelIds);

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

  useEffect(() => {
    setContinuationToken('');
    setContinuationTokenCache(new Map<number, string>());
  }, [orphanOnly, modelsIdsString, locationId, pageSize, searchText, filterSpecifications]);

  const query = useQuery<ITwinWithRelationshipsPage, ApiException>(
    [
      'twins',
      QueryId,
      NextPage,
      orphanOnly,
      modelsIdsString,
      searchText,
      pageSize,
      continuationTokenParam,
      locationId,
      filterSpecifications,
    ],
    () => api.queryTwins(pageSize, continuationTokenParam, includeTotalCount, twinsRequest),
    options
  );

  return {
    query,
    continuationToken,
    setContinuationToken,
    pageSize,
    setPageSize,
    setContinuationTokenCache,
    filtersStates: {
      selectedLocationState,
      selectedModelState,
      searchTextState,
      selectedOrphanState,
      filterSpecificationsState,
    },
    sourceType: sourceTypeUrlParam,
  };
}

function mapFilterSpecifications(
  filterSpecifications: FilterSpecificationDto[] = [],
  sourceType: SourceType
): FilterSpecificationDto[] {
  const adtFieldMap: { [key: string]: string } = {
    id: '$dtId',
    siteId: 'siteID',
    uniqueID: 'uniqueID',
    name: 'name',
    externalID: 'externalID',
  };
  const adxFieldMap: { [key: string]: string } = {
    id: 'Id',
    siteId: 'SiteId',
    uniqueID: 'UniqueId',
    name: 'Name',
    externalID: 'ExternalId',
  };

  const fieldMap = sourceType === SourceType.AdtQuery ? adtFieldMap : adxFieldMap;

  return filterSpecifications.map((x) => {
    const result = new FilterSpecificationDto();
    result.field = fieldMap[x.field] ?? x.field;
    result.operator = x.operator;
    result.value = x.value;
    return result;
  });
}
