import { createContext, useContext, useState, Dispatch, SetStateAction } from 'react';
import { UseQueryResult, UseMutationResult } from 'react-query';
import useGetLatestDQValidationJob from './hooks/useGetLatestDQValidationJob';
import useGetDQResults from './hooks/useGetDQResults';
import useMutateDQTwinValidation, { ValidateTwinsMutateParams } from './hooks/useMutateDQTwinValidation';
import {
  ApiException,
  ErrorResponse,
  ITwinsValidationJob,
  IInterfaceTwinsInfo,
  ValidationResultsPage,
  NestedTwin,
} from '../../services/Clients';
import { PopUpExceptionTemplate } from '../../components/PopUps/PopUpExceptionTemplate';
import useDebounce from '../../utils/useDebounce';

type DataQaulityContextType = {
  latestValidationJobQuery: UseQueryResult<ITwinsValidationJob, ApiException>;
  mutateDQTwinValidation: UseMutationResult<ITwinsValidationJob, any, ValidateTwinsMutateParams, unknown>;
  getDQResultsQuery: UseQueryResult<ValidationResultsPage, ApiException>;
  setContinuationToken: Dispatch<SetStateAction<string>>;
  continuationToken: string;
  errorsOnly: boolean;
  setErrorsOnly: (errorsOnly: boolean) => void;
  DQResultsPageSize: number;
  setDQResultsPageSize: (pageSize: number) => void;
  searchString: string;
  setSearchString: (searchString: string) => void;
  selectedFilterLocation: NestedTwin | null;
  setSelectedFilterLocation: (location: NestedTwin | null) => void;
  selectedFilterModels: IInterfaceTwinsInfo[];
  setSelectedFilterModels: (modelIds: IInterfaceTwinsInfo[]) => void;
};

const DataQaulityContext = createContext<DataQaulityContextType | undefined>(undefined);

export function useDataQaulity() {
  const context = useContext(DataQaulityContext);
  if (context == null) {
    throw new Error('useDataQaulity must be used within a DataQaulityProvider');
  }

  return context;
}

/**
 * DataQaulityProvider is a wrapper component that handles fetching data quality related data.
 */
export default function DataQaulityProvider({
  shouldFetchDQResults = false,
  children,
}: {
  shouldFetchDQResults?: boolean;
  children: JSX.Element;
}) {
  // state used for error handling
  const [openPopUp, setOpenPopUp] = useState(true);
  const [showPopUp, setShowPopUp] = useState(false);
  const [errorMessage, setErrorMessage] = useState<ErrorResponse | ApiException>();

  const latestValidationJobQuery = useGetLatestDQValidationJob(
    { status: undefined },
    {
      onError: (error) => {
        setErrorMessage(error);
        setShowPopUp(true);
        setOpenPopUp(true);
      },
      cacheTime: 0, // We don't want to cache this query, because we want to always fetch the latest validation job.
    }
  );

  const mutateDQTwinValidation = useMutateDQTwinValidation({
    onError: (error) => {
      setErrorMessage(error);
      setShowPopUp(true);
      setOpenPopUp(true);
    },
  });

  // state used for filtering DQ validation results
  const [errorsOnly, setErrorsOnly] = useState<boolean>(true);

  const [searchString, setSearchString] = useState<string>('');
  const searchStringDebounced = useDebounce(searchString, 500);

  const [selectedFilterLocation, setSelectedFilterLocation] = useState<NestedTwin | null>(null);
  const [selectedFilterModels, setSelectedFilterModels] = useState<IInterfaceTwinsInfo[]>([]);

  const {
    getDQResultsQuery,
    continuationToken,
    setContinuationToken,
    setPageSize: setDQResultsPageSize,
    pageSize: DQResultsPageSize,
  } = useGetDQResults(
    {
      errorsOnly,
      searchString: searchStringDebounced,
      locationId: selectedFilterLocation?.twin?.siteID || '',
      modelIds: selectedFilterModels.map((m) => m.id).filter((m) => m) as string[],
    },
    {
      enabled: shouldFetchDQResults, // This provider is used in two locations: DQ Results page, and DQ Trigger-scan page. We only want to fetch DQ results when we are on the DQ Results page.
      onError: (error) => {
        setErrorMessage(error);
        setShowPopUp(true);
        setOpenPopUp(true);
      },
      select: ({ content = [], continuationToken }: ValidationResultsPage): any => {
        const newContent = content.map((item, i) => ({ ...item, id: `${item.twinDtId}${i}` })); // MUI grid requires unique id for each row
        return {
          content: newContent,
          continuationToken,
        };
      },
    }
  );

  return (
    <DataQaulityContext.Provider
      value={{
        latestValidationJobQuery,
        mutateDQTwinValidation,
        getDQResultsQuery,
        setContinuationToken,
        continuationToken,
        errorsOnly,
        setErrorsOnly,
        setDQResultsPageSize,
        DQResultsPageSize,
        searchString,
        setSearchString,
        selectedFilterLocation,
        setSelectedFilterLocation,
        selectedFilterModels,
        setSelectedFilterModels,
      }}
    >
      {children}

      {
        // todo: remove when global error handling is implemented
        showPopUp && (
          <PopUpExceptionTemplate isCurrentlyOpen={openPopUp} onOpenChanged={setOpenPopUp} errorObj={errorMessage} />
        )
      }
    </DataQaulityContext.Provider>
  );
}
