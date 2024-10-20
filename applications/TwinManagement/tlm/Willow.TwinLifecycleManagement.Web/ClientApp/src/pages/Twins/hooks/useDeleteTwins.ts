import { useMutation, useQueryClient } from 'react-query';
import { useState } from 'react';
import useApi from '../../../hooks/useApi';
import {
  MultipleEntityResponse,
  HttpStatusCode,
  ErrorResponse,
  ApiException,
  DeleteTwinsRequest,
} from '../../../services/Clients';
import { DeleteWarning } from '../../../components/PopUps/PopUpDeleteWarning';
import { GridRowId, useGridApiRef } from '@mui/x-data-grid-pro';
import { AlertProps } from '@mui/material';

export default function useDeleteTwins({
  setSnackbar,
  selectedRowsState,
  setErrorMessage,
  setOpenPopUp,
  apiRef,
}: {
  setSnackbar: (snackbar: Pick<AlertProps, 'children' | 'severity'> | null) => void;
  selectedRowsState: [GridRowId[], (ids: GridRowId[]) => void];
  setErrorMessage: (error: ErrorResponse | ApiException) => void;
  setOpenPopUp: (isOpen: boolean) => void;
  apiRef: ReturnType<typeof useGridApiRef>;
}) {
  const api = useApi();
  const queryClient = useQueryClient();
  const [includeRelationships, setIncludeRelationships] = useState(false);
  const [deleteWarning, setDeleteWarning] = useState<DeleteWarning | null>(null);

  // ids used to filter out twins that were deleted
  const [deletedTwinIds, setDeletedTwinsIds] = useState<string[]>([]);

  const [selectedRows, setSelectedRows] = selectedRowsState;

  const mutation = useMutation(
    ({ twinIds }: { twinIds: string[] }) => {
      const request = new DeleteTwinsRequest();
      request.twinIDs = twinIds;
      request.externalIDs = twinIds
        .map((id) => apiRef.current.getRow(id))
        .filter(({ twin }) => twin.externalID)
        .map(({ twin }) => twin.externalID);

      return api.deleteTwinsIds(includeRelationships, request);
    },
    {
      onError: (error: any) => {
        setErrorMessage(error);
        setOpenPopUp(true);
      },
      onSuccess: async (data: MultipleEntityResponse) => {
        const { responses = [] } = data;

        const deletedIds = responses
          .filter((x) => x.statusCode === HttpStatusCode.OK)
          .map(({ entityId, subEntityId }) => {
            return subEntityId ? subEntityId : entityId!;
          });

        // twins does not exist in ADT, but exist in ADX. Not found twins are deleted from ADX on server-side.
        const notFoundIds = responses
          .filter((x) => x.statusCode === HttpStatusCode.NotFound)
          .map(({ entityId }) => entityId!);

        // deletedIds can contain deleted twins, not found twins, or deleted relationships, put in Set to get unique deleted ids.
        const deletedIdsSet = new Set([...deletedIds, ...notFoundIds]);

        const notDeleted = responses.filter(
          (x) =>
            x.statusCode !== HttpStatusCode.OK &&
            x.statusCode !== HttpStatusCode.NotFound &&
            !deletedIdsSet.has(x.subEntityId!) // Responses may have relationship not found due to it already being deleted during the delete process on server-side.
        );

        const notDeletedIds = notDeleted.map(({ entityId }) => entityId!);

        // notFoundIds can contain not found twins or relationships, put in Set to get unique not found ids.
        const notDeletedIdsSet = new Set(notDeletedIds);

        // keep track of all deleted twins ids, so we can filter them out from the table
        setDeletedTwinsIds((prev) => [...prev, ...deletedIds, ...notFoundIds]);

        // show success snackbar if all twins were deleted
        if (deletedIdsSet.size >= selectedRows.length && notDeletedIdsSet.size === 0) {
          setSnackbar({ children: 'Twins deleted successful', severity: 'success' });
        }
        // show warning snackbar if some twins were deleted
        else if (deletedIdsSet.size > 0 && notDeletedIdsSet.size > 0) {
          setSnackbar({ children: 'Some twins were not deleted successfully', severity: 'warning' });
        }

        // show popup warning that displays exceptions if some twins didn't get deleted
        if (notDeletedIds.length > 0) {
          // parse exceptions
          const exceptions = {} as Record<string, string>;
          for (let { entityId, subEntityId, message } of notDeleted) {
            if (subEntityId) {
              exceptions[subEntityId] = message!;
            } else {
              exceptions[entityId!] = message!;
            }
          }

          const deleteWarning = {
            exceptions,
            title: `${notDeletedIdsSet.size} of ${selectedRows.length} twins did not get deleted:`,
          };
          setDeleteWarning(deleteWarning);

          // persist selections for those twins that were not deleted
          setSelectedRows([...notDeletedIds]);
        }

        // refresh models caches with updated twin count
        queryClient.invalidateQueries(['models']);

        // refresh mapped building/connector's dropdown
        queryClient.invalidateQueries(['get-all-twins']);
      },
    }
  );

  const deleteTwinsMutation = { mutateDeleteTwins: mutation, includeRelationships, setIncludeRelationships };

  return { deleteTwinsMutation, deletedTwinIds, deleteWarning };
}
