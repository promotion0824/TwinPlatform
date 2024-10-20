import { Button, ButtonGroup, Icon, Menu, Modal, useDisclosure, Checkbox, Tooltip, Link } from '@willowinc/ui';
import { useEffect, useState, useMemo } from 'react';
import styled from '@emotion/styled';
import useSync from '../../hooks/useSync';
import MappedBuildingSelector, { SelectedBuildingType } from './MappedBuildingSelector';
import MappedConnectorSelector from './MappedConnectorSelector';
import { useSnackbar } from '../../../../providers/SnackbarProvider/SnackbarProvider';
import { useQueryClient } from 'react-query';
// import { configService } from '../../../../services/ConfigService';

enum SyncType {
  ORGANIZATION = 'Organization',
  SPATIAL_RESOURCES = 'Spatial Resources',
  ASSETS = 'Assets',
  CAPABILITIES = 'Capabilities',
}

type ModalType = SyncType | 'PushToMapped';

/**
 * Display different kinds of sync modals based on which button was clicked.
 */
export default function SyncButtons({ disabled }: { disabled: boolean }) {
  const menuRenderObj = {
    [SyncType.ORGANIZATION]: { onClick: () => handleButtonClick(SyncType.ORGANIZATION) },
    [SyncType.SPATIAL_RESOURCES]: { onClick: () => handleButtonClick(SyncType.SPATIAL_RESOURCES) },
    [SyncType.ASSETS]: { onClick: () => handleButtonClick(SyncType.ASSETS) },
    [SyncType.CAPABILITIES]: { onClick: () => handleButtonClick(SyncType.CAPABILITIES) },
  };

  const [opened, { open, close }] = useDisclosure(false);
  const modalTypeState = useState<ModalType | null>(null);

  // open modal based on type of button clicked
  function handleButtonClick(type: ModalType) {
    modalTypeState[1](type);
    open();
  }

  // close modal and reset modalTypeState
  function handleModalClose() {
    close();
    modalTypeState[1](null);
  }

  // // open modal for PushToMapped button
  // function handlePushToMappedButton() {
  //   handleButtonClick('PushToMapped');
  // }

  return (
    <>
      {/*parent container's flex direction is row-reverse to remove gap on last button on right end side of container.  */}
      {
        // hide push to mapped button until it is implemented
        /* <Button
        kind="secondary"
        onClick={handlePushToMappedButton}
        // enableSyncToMapped is set in TLM's appsettings
        disabled={disabled || !configService.config.mtiOptions.enableSyncToMapped}
      >
        Push to Mapped
      </Button> */
      }
      <StyledMenu isdisabled={disabled ? 'true' : undefined}>
        <Menu.Target>
          {/* @ts-expect-error */}
          <MaxWidthButton kind="secondary" suffix={<Icon icon="expand_more" />}>
            Pull From Mapped & Ingest
          </MaxWidthButton>
        </Menu.Target>
        <Menu.Dropdown>
          {Object.entries(menuRenderObj).map(([key, value]) => {
            return (
              <Menu.Item key={key} onClick={value.onClick}>
                {key}
              </Menu.Item>
            );
          })}
        </Menu.Dropdown>
      </StyledMenu>

      <SyncModal opened={opened} close={handleModalClose} modalType={modalTypeState[0]} />
    </>
  );
}

const MaxWidthButton = styled(Button)({ maxWidth: 196 });

interface MenuProps {
  isdisabled: string | undefined;
}

const StyledMenu = styled(Menu)<MenuProps>(({ isdisabled }) => ({
  ...(isdisabled && {
    color: '#474747 !important',
    fill: '#474747 !important',
    backgroundColor: '#242424 !important',

    borderColor: 'transparent !important',
    cursor: 'not-allowed !important',
    backgroundImage: 'none',
    pointerEvents: 'none',

    maxWidth: 196,
  }),
}));

function SyncModal({ opened, close, modalType }: { opened: boolean; close: () => void; modalType: ModalType | null }) {
  const selectedBuildingsState = useState<SelectedBuildingType[]>([]);
  const selectedConnectorState = useState<string | null>(null);
  const matchStdPntListState = useState<boolean>(true);
  const autoApproveState = useState<boolean>(true);

  // reset values when modal is closed
  useEffect(
    () => {
      if (!opened) {
        selectedBuildingsState[1]([]);
        selectedConnectorState[1](null);
        matchStdPntListState[1](true);
        autoApproveState[1](true);
      }
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [opened]
  );

  const ModalContentMap = useMemo(
    () => ({
      [SyncType.ORGANIZATION]: {
        contentText:
          'New twins in Mapped that do not yet exist and need to be approved will appear as Pending. All Approved twins will be ingested to ADT.',
        modalMinHeight: 139,
        showBuildingDropdown: false,
        showConnectorDropdown: false,
        showAutoApproveCheckBox: false,
        showMatchStdPntListCheckBox: false,
      },
      [SyncType.SPATIAL_RESOURCES]: {
        contentText:
          'New twins in Mapped that do not yet exist and need to be approved will appear as Pending. All Approved twins will be ingested to ADT.',
        modalMinHeight: 183,
        showBuildingDropdown: true,
        showConnectorDropdown: false,
        showAutoApproveCheckBox: false,
        showMatchStdPntListCheckBox: false,
      },
      [SyncType.ASSETS]: {
        contentText:
          'New twins in Mapped that do not yet exist and need to be approved will appear as Pending. All Approved twins will be ingested to ADT.',
        modalMinHeight: 227,
        showBuildingDropdown: true,
        showConnectorDropdown: true,
        showAutoApproveCheckBox: true,
        showMatchStdPntListCheckBox: false,
      },
      [SyncType.CAPABILITIES]: {
        contentText:
          'New twins in Mapped that do not yet exist and need to be approved will appear as Pending. All Approved twins will be ingested to ADT.',
        modalMinHeight: 227,
        showBuildingDropdown: true,
        showConnectorDropdown: true,
        showAutoApproveCheckBox: true,
        showMatchStdPntListCheckBox: false,
      },
      PushToMapped: {
        contentText:
          'New twins in Mapped that do not yet exist and need to be approved will appear as Pending. All Approved twins will be ingested to ADT.',
        modalMinHeight: 100,
        showBuildingDropdown: false,
        showConnectorDropdown: false,
        showAutoApproveCheckBox: false,
        showMatchStdPntListCheckBox: false,
      },
      default: undefined,
    }),
    []
  );

  const content = modalType ? ModalContentMap[modalType] : ModalContentMap.default;

  const { syncOrganizationMutation, syncSpatialResourcesMutation, syncAssetsMutation, syncCapabilitiesMutation } =
    useSync({
      onError: () => {
        snackbar.show('Error syncing', { isError: true });
      },
    });

  const submitMap = {
    [SyncType.ORGANIZATION]: syncOrganizationMutation,
    [SyncType.SPATIAL_RESOURCES]: syncSpatialResourcesMutation,
    [SyncType.ASSETS]: syncAssetsMutation,
    [SyncType.CAPABILITIES]: syncCapabilitiesMutation,
    PushToMapped: {
      mutate: () => {},
      isLoading: false,
    },
    default: {
      mutate: () => {},
      isLoading: false,
    },
  };

  const queryClient = useQueryClient();

  const snackbar = useSnackbar();
  const snackbarIdState = useState<string | null>(null);
  const flagState = useState<boolean>(false); // flag to conditionally hide snackbar, minimum time for snackbar to display

  const mutate = submitMap[modalType || 'default'];
  const { mutate: submit } = mutate;
  async function handleSubmit() {
    try {
      switch (modalType) {
        case SyncType.ORGANIZATION:
          submit({ autoApprove: false });
          break;

        case SyncType.SPATIAL_RESOURCES:
          submit({
            autoApprove: false,
            buildingIds: selectedBuildingsState[0].map(({ value }) => value),
          });
          break;

        case SyncType.ASSETS:
          submit({
            autoApprove: autoApproveState[0],
            buildingIds: selectedBuildingsState[0].map(({ value }) => value),
            connectorId: selectedConnectorState[0],
          });
          break;

        case SyncType.CAPABILITIES:
          submit({
            autoApprove: autoApproveState[0],
            buildingIds: selectedBuildingsState[0].map(({ value }) => value),
            connectorId: selectedConnectorState[0],
            matchStdPntList: matchStdPntListState[0],
          });
          break;

        case 'PushToMapped':
          break;

        default:
          break;
      }

      queryClient.invalidateQueries('GetLatestMtiAsyncJob');
      const snackbarId = snackbar.show('Queuing Sync job', { isAutoHide: false });
      snackbarIdState[1](snackbarId);

      // minimum time for snackbar to show
      setTimeout(() => {
        flagState[1](true);
      }, 5200);
    } catch (error) {
    } finally {
      close();
    }
  }

  // hide snackbar when sync job request is successful
  useEffect(() => {
    if (!queryClient.isMutating() && snackbarIdState[0] && flagState[0]) {
      snackbar.hide({ snackbarId: snackbarIdState[0] });
      snackbarIdState[1](null);
      flagState[1](false);
      queryClient.invalidateQueries('getMtiAsyncJobs');
    }
  }, [snackbar, snackbarIdState, queryClient, flagState]);

  const shouldDisableSubmitState = useState<boolean>(false);

  useEffect(
    () => {
      shouldDisableSubmitState[1](shouldDisableSubmit());
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [modalType, selectedBuildingsState[0], selectedConnectorState[0]]
  );

  function shouldDisableSubmit() {
    switch (modalType) {
      case SyncType.ORGANIZATION:
      case 'PushToMapped':
        return false;
      case SyncType.SPATIAL_RESOURCES:
        return selectedBuildingsState[0].length === 0;
      case SyncType.ASSETS:
      case SyncType.CAPABILITIES:
        return selectedBuildingsState[0].length === 0 || !selectedConnectorState[0];
      default:
        return true;
    }
  }

  return (
    <StyledModal
      opened={opened}
      onClose={close}
      header={modalType === 'PushToMapped' ? 'Push to Mapped' : 'Pull from Mapped & Ingest to ADT'}
      size="sm"
      centered
      transitionProps={{ duration: 0 }}
    >
      {content && (
        <>
          <ModalContent>
            {content.contentText}
            {content.showBuildingDropdown && <MappedBuildingSelector selectedBuildingsState={selectedBuildingsState} />}
            {content.showConnectorDropdown && (
              <MappedConnectorSelector selectedConnectorState={selectedConnectorState} />
            )}

            {content.showMatchStdPntListCheckBox && <MatchStdPntCheckbox matchStdPntListState={matchStdPntListState} />}

            {content.showAutoApproveCheckBox && (
              <Checkbox
                label={`Auto-approve all ${modalType} without conflicts`}
                checked={autoApproveState[0]}
                onChange={() => {
                  autoApproveState[1]((prev) => !prev);
                }}
              />
            )}
          </ModalContent>
        </>
      )}
      <FooterContainer>
        <ButtonGroup>
          <Button kind="secondary" background="transparent" onClick={close}>
            Cancel
          </Button>
          <Button onClick={handleSubmit} disabled={shouldDisableSubmitState[0]}>
            {`Pull & Ingest ${modalType}`}
          </Button>
        </ButtonGroup>
      </FooterContainer>
    </StyledModal>
  );
}

function MatchStdPntCheckbox({
  matchStdPntListState,
}: {
  matchStdPntListState: [boolean, React.Dispatch<React.SetStateAction<boolean>>];
}) {
  const STP_LINK =
    'https://ridleyco.sharepoint.com/:x:/r/sites/EXT_WillowMapped/Shared%20Documents/General/Standard%20Telemetry%20Points%20(STP).xlsx?d=w3f265636333d414abbb4cafa85619c2c&csf=1&web=1&e=2Mc9E6';
  return (
    <CheckBoxContainer>
      <Checkbox
        checked={matchStdPntListState[0]}
        onChange={() => {
          matchStdPntListState[1]((prev) => !prev);
        }}
      />
      Filter to match{' '}
      <Link href={STP_LINK} target="_blank">
        Standard Points List
      </Link>
      <Tooltip multiline withArrow width={139} label="Only applicable to Edge Connectors">
        <StyledIcon icon="info" size={16} />
      </Tooltip>
    </CheckBoxContainer>
  );
}

const StyledIcon = styled(Icon)({ fontVariationSettings: `'FILL' 1,'wght' 400,'GRAD' 200,'opsz' 20 !important` });
const CheckBoxContainer = styled('div')({
  display: 'flex',
  flexDirection: 'row',
  justifyContent: 'start',
  alignItems: 'center',
  gap: 8,
  '> a': { marginLeft: -3 },
});

const StyledModal = styled(Modal)`
  .mantine-Modal-inner {
    bottom: 20%; // move modal closer to the top
  }
`;

interface ModalContentProps {
  minHeight?: number;
}

const ModalContent = styled.div<ModalContentProps>((props) => ({
  padding: '1rem',
  display: 'flex',
  flexDirection: 'column',
  gap: 16,
  minHeight: `${props.minHeight || 100}px`,
}));

const FooterContainer = styled('div')({
  height: 60,
  display: 'flex',
  gap: 12,
  flexDirection: 'row',
  justifyContent: 'flex-end',
  padding: 16,
  borderTop: '1px solid #3B3B3B',
});
