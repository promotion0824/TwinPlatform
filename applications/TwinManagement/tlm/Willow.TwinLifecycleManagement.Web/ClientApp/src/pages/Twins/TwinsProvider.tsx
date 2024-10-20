/* eslint-disable @typescript-eslint/no-unused-vars */
import { createContext, Dispatch, SetStateAction, useContext, useState, useEffect } from 'react';
import { ApiException, ErrorResponse, MultipleEntityResponse } from '../../services/Clients';
import { PopUpExceptionTemplate } from '../../components/PopUps/PopUpExceptionTemplate';
import useGetTwins, { IGetTwins } from './hooks/useGetTwins';
import { GridRowId, useGridApiRef } from '@mui/x-data-grid-pro';
import useDeleteTwins from './hooks/useDeleteTwins';
import PopUpDeleteWarning from '../../components/PopUps/PopUpDeleteWarning';
import { Alert, AlertProps, Snackbar } from '@mui/material';
import useMultipleSearchParams from '../../hooks/useMultipleSearchParams';
import { UseMutationResult } from 'react-query';

type TwinsContextType = {
  apiRef: ReturnType<typeof useGridApiRef>;
  getTwinsQuery: IGetTwins;
  selectedRows: GridRowId[];
  setSelectedRows: Dispatch<SetStateAction<GridRowId[]>>;
  deleteTwinsMutation: {
    mutateDeleteTwins: UseMutationResult<MultipleEntityResponse, any, { twinIds: string[] }, unknown>;
    includeRelationships: boolean;
    setIncludeRelationships: Dispatch<SetStateAction<boolean>>;
  };
  showAdxSync: boolean;
};

const TwinsContext = createContext<TwinsContextType | undefined>(undefined);

export function useTwins() {
  const context = useContext(TwinsContext);
  if (context == null) {
    throw new Error('useTwins must be used within a TwinsProvider');
  }

  return context;
}

/**
 * TwinsProvider is a wrapper component that handles fetching twins related data.
 */
export default function TwinsProvider({ children }: { children: JSX.Element }) {
  const apiRef = useGridApiRef();
  // states used for error handling
  const [openPopUp, setOpenPopUp] = useState(true);
  const [errorMessage, setErrorMessage] = useState<ErrorResponse | ApiException>();

  // Show AdxSync button when showAdxSync url param is true
  const [urlParams] = useMultipleSearchParams([{ name: 'showAdxSync', type: 'string' }]);
  const showAdxSyncUrlParam = (urlParams?.showAdxSync || '') as string;
  const [showAdxSync] = useState<boolean>(showAdxSyncUrlParam.toLowerCase() === 'true');

  const [snackbar, setSnackbar] = useState<Pick<AlertProps, 'children' | 'severity'> | null>(null);

  const handleCloseSnackbar = () => setSnackbar(null);

  const [selectedRows, setSelectedRows] = useState<GridRowId[]>([]);

  const [showPopUpDeleteWarning, setShowPopUpDeleteWarning] = useState(false);
  const { deletedTwinIds, deleteTwinsMutation, deleteWarning } = useDeleteTwins({
    selectedRowsState: [selectedRows, setSelectedRows],
    setSnackbar,
    setOpenPopUp,
    setErrorMessage,
    apiRef,
  });

  useEffect(() => {
    setShowPopUpDeleteWarning(!!deleteWarning);
  }, [deleteWarning]);

  const getTwinsQuery = useGetTwins({
    onError: (error) => {
      setErrorMessage(error);
      setOpenPopUp(true);
    },
    select: (data) => {
      // filter out twins that were deleted, due to limitation of server-side pagination queries
      return {
        ...data,
        content: data?.content?.filter((x) => !deletedTwinIds.includes(x.twin?.$dtId!)),
      };
    },
  });

  return (
    <TwinsContext.Provider
      value={{
        apiRef,
        getTwinsQuery,
        selectedRows,
        setSelectedRows,
        deleteTwinsMutation,
        showAdxSync,
      }}
    >
      {children}

      {
        // todo: remove when global error handling is implemented
        <PopUpExceptionTemplate isCurrentlyOpen={openPopUp} onOpenChanged={setOpenPopUp} errorObj={errorMessage} />
      }

      {!!snackbar && (
        <Snackbar
          sx={{ top: '90px !important' }}
          open
          anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
          onClose={handleCloseSnackbar}
          autoHideDuration={6000}
        >
          <Alert {...snackbar} onClose={handleCloseSnackbar} variant="filled" />
        </Snackbar>
      )}

      <PopUpDeleteWarning isOpen={showPopUpDeleteWarning} onOpen={setShowPopUpDeleteWarning} errors={deleteWarning} />
    </TwinsContext.Provider>
  );
}
