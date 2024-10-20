import { Button, Icon, Loader, Modal, ButtonGroup, useDisclosure, GridRowModelUpdate, Tooltip } from '@willowinc/ui';
import { useMappings } from '../MappingsProvider';
import styled from '@emotion/styled';
import { AsyncJobStatus, Status } from '../../../services/Clients';
import { Typography } from '@mui/material';
import { useState } from 'react';
import SyncButtons from './SyncButtons/SyncButtons';
import useGetLatestMtiAsyncJob from '../hooks/useGetLatestMtiAsyncJob';
import useMultipleSearchParams from '../../../hooks/useMultipleSearchParams';
import { useQueryClient } from 'react-query';
import useDeleteUpdateTwinRequests from '../hooks/useDeleteUpdateTwinRequests';
import { useSnackbar } from '../../../providers/SnackbarProvider/SnackbarProvider';
import { AuthHandler } from '../../../components/AuthHandler';
import { AppPermissions } from '../../../AppPermissions';
type ModalType = 'DeleteAll' | 'DeleteBulk' | 'DeleteConflicts';

export default function ApproveAcceptActionButtons() {
  const { tabState } = useMappings();

  return (
    <ActionsButtonsContainer>
      {/* flex direction is row-reverse so gap is removed from last button on right end side.  */}
      {tabState[0] !== 'conflicts' ? <MappingTableActionButtons /> : <ConflictsTableActionButtons />}
    </ActionsButtonsContainer>
  );
}

function MappingTableActionButtons() {
  const {
    selectedRowsState,
    handleStatusChange,
    handleDeleteAll,
    isLoadingState,
    changeMappedEntriesStatusMutate,

    handleDeleteBulk,
    tableApiRef,
    selectAllState,
  } = useMappings();
  const [selectedRows] = selectedRowsState;

  const { isLoading } = changeMappedEntriesStatusMutate;

  const { selectAll } = selectAllState[0];
  const isDisabled = selectedRows.length === 0 || isLoading || isLoadingState[0] !== null;

  const [opened, { open, close }] = useDisclosure(false);
  const modalTypeState = useState<ModalType | null>(null);

  const handleSubmit = async () => {
    switch (modalTypeState[0]) {
      case 'DeleteAll':
        handleDeleteAll();
        break;

      case 'DeleteBulk':
        handleDeleteBulk();
        break;
      default:
        break;
    }
    close();
  };

  function handleButtonClick(type: ModalType) {
    modalTypeState[1](type);
    open();
  }

  function handleModalClose() {
    close();
    modalTypeState[1](null);
  }

  const [urlParams] = useMultipleSearchParams([{ name: 'devMode', type: 'string' }]);
  const devModesUrlParam = (urlParams?.devMode || '') as string;
  const isDevMode = devModesUrlParam.toLowerCase() === 'true';

  const pendingRecordCount = selectedRows
    .map((id) => {
      return tableApiRef.current.getRow(id);
    })
    .filter((item) => item && (item.status === Status.Pending || isDevMode));

  const modalContentMap: Record<ModalType, string> = {
    DeleteAll: `You are about to delete all records. Are you sure you want to remove them?`,
    DeleteBulk: `You are about to delete ${selectAll ? 'all' : pendingRecordCount.length} ${
      isDevMode ? '' : 'pending'
    } twins. Are you sure you want to remove them?`,
    DeleteConflicts: '',
  };

  const ContentComponent = modalTypeState[0] ? modalContentMap[modalTypeState[0]] : '';

  // disable delete button if there no selected pending records
  const shouldDisableDeleteButton =
    (pendingRecordCount.length === 0 && !isDevMode && !selectAll) || isLoadingState[0] !== null;

  return (
    <>
      {isDevMode && (
        <Button
          onClick={() => {
            handleButtonClick('DeleteAll');
          }}
          kind="negative"
        >
          Delete All
        </Button>
      )}

      <Tooltip label="You can only delete pending records." withArrow>
        <Button
          onClick={() => {
            handleButtonClick('DeleteBulk');
          }}
          disabled={shouldDisableDeleteButton}
          prefix={isLoadingState[0] === 'deleting' ? <Loader /> : <StyledIcon icon="info" />}
          kind="negative"
        >
          Delete
        </Button>
      </Tooltip>

      <Button
        onClick={() => {
          handleStatusChange(Status.Ignore);
        }}
        disabled={isDisabled}
        suffix={isLoadingState[0] === Status.Ignore && <Loader />}
        kind="secondary"
      >
        Ignore
      </Button>
      <Button
        onClick={() => {
          handleStatusChange(Status.Approved);
        }}
        disabled={isDisabled}
        suffix={isLoadingState[0] === Status.Approved && <Loader />}
        kind="secondary"
      >
        Approve
      </Button>

      <SyncDropdownButtons />

      {/* confirmation deletion popup */}
      <ConfirmationModal opened={opened} close={handleModalClose} onSubmit={handleSubmit} modalType={modalTypeState[0]}>
        <Typography sx={{ paddingTop: '5px !important' }}>{ContentComponent}</Typography>
      </ConfirmationModal>
    </>
  );
}
const StyledIcon = styled(Icon)({ fontVariationSettings: `'FILL' 1,'wght' 400,'GRAD' 200,'opsz' 20 !important` });

function ConflictsTableActionButtons() {
  const { selectedRowsState, tableApiRef, syncRowsState, selectAllState } = useMappings();
  const [selectedRows] = selectedRowsState;
  const [opened, { open, close }] = useDisclosure(false);

  const { selectAll } = selectAllState[0];

  const { mutateAsync: deleteUpdateTwinRequests, isLoading } = useDeleteUpdateTwinRequests(selectAll);
  const snackbar = useSnackbar();
  const queryClient = useQueryClient();

  const handleSubmit = async () => {
    try {
      close();
      const numOfDeletedRecords = await deleteUpdateTwinRequests({ ids: selectedRows as string[] });

      const gridRowModelUpdate = selectedRows.map((id) => ({ id, _action: 'delete' } as GridRowModelUpdate));

      tableApiRef.current.updateRows(gridRowModelUpdate);
      syncRowsState[1]((prev) => ({ ...prev, conflicts: [...(prev?.['conflicts'] || []), ...gridRowModelUpdate] }));

      queryClient.invalidateQueries('update-twin-requests');
      queryClient.invalidateQueries('update-twin-requests-count');
      snackbar.show(`${numOfDeletedRecords} conflicts deleted successfully`);
    } catch (error) {
      snackbar.show(`An Error occurred while deleting. Please try again.`, { isError: true });
    }
  };

  const shouldDisableButton = selectedRows.length === 0 || isLoading;

  return (
    <>
      <Button
        onClick={open}
        kind="negative"
        disabled={shouldDisableButton}
        prefix={isLoading ? <Loader /> : <StyledIcon icon="info" />}
      >
        Delete
      </Button>

      <SyncDropdownButtons />

      {/* confirmation deletion popup */}
      <ConfirmationModal opened={opened} close={close} onSubmit={handleSubmit} modalType={'DeleteConflicts'}>
        <Typography sx={{ paddingTop: '5px !important' }}>
          {`You are about to delete ${
            selectAll ? 'all' : selectedRows.length
          } conflicts records. Are you sure you want to remove them?`}
        </Typography>
      </ConfirmationModal>
    </>
  );
}

function SyncDropdownButtons() {
  const { data: latestMtiJob, isLoading: isGetLatestMtiJobLoading } = useGetLatestMtiAsyncJob(undefined, {
    refetchInterval: 1000 * 5, // refetch every 5 seconds
    refetchIntervalInBackground: true,
  });

  const [urlParams] = useMultipleSearchParams([{ name: 'bypass', type: 'string' }]);
  // bypass flag for override disabling buttons when job is queued or processing
  // https://localhost:44423/review-twins?bypass=true
  const bypassUrlParam = (urlParams?.bypass || '') as string;
  const bypass = bypassUrlParam.toLowerCase() === 'true';

  const queryClient = useQueryClient();

  const isMutating = queryClient.isMutating();

  const shouldDisableMtiButtons =
    (latestMtiJob?.details?.status === AsyncJobStatus.Processing ||
      latestMtiJob?.details?.status === AsyncJobStatus.Queued ||
      isGetLatestMtiJobLoading ||
      !!isMutating) &&
    !bypass;

  return (
    <>
      {
        <AuthHandler requiredPermissions={[AppPermissions.CanSyncToMapped]}>
          <SyncButtons disabled={shouldDisableMtiButtons} />
        </AuthHandler>
      }
    </>
  );
}

const ActionsButtonsContainer = styled('div')({
  display: 'flex',
  flexDirection: 'row',
  gap: 8,
  flexFlow: 'row-reverse',
  flexWrap: 'wrap-reverse',
});

function ConfirmationModal({
  opened,
  close,
  children,
  onSubmit,
  modalType,
}: {
  opened: boolean;
  close: () => void;
  children: React.ReactNode;
  onSubmit: () => void;
  modalType: ModalType | null;
}) {
  const headerObj = {
    DeleteAll: 'Delete Twins',
    DeleteBulk: 'Delete Twins',
    Ingest: 'Ingest Twins',
    DeleteConflicts: 'Delete Conflicts',
  };
  const header = !!modalType ? headerObj[modalType] : '';

  return (
    <StyledModal opened={opened} onClose={close} size="md" centered header={header} transitionProps={{ duration: 0 }}>
      <ModalContent>{children}</ModalContent>
      <FooterContainer>
        <ButtonGroup>
          <Button kind="secondary" background="transparent" onClick={close}>
            Cancel
          </Button>
          <Button onClick={onSubmit}>Confirm</Button>
        </ButtonGroup>
      </FooterContainer>
    </StyledModal>
  );
}
const ModalContent = styled.div((props) => ({
  padding: '1rem',
}));

const StyledModal = styled(Modal)`
  .mantine-Modal-inner {
    bottom: 20%; // move modal closer to the top
  }
`;

const FooterContainer = styled('div')({
  height: 60,
  display: 'flex',
  gap: 12,
  flexDirection: 'row',
  justifyContent: 'flex-end',
  padding: 16,
});
