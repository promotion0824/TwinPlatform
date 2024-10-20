import { useDisclosure, Modal, Button } from '@willowinc/ui';
import styled from '@emotion/styled';
import useGetMappedEntriesCount from '../hooks/useGetMappedEntriesCount';
import { useLocation, useNavigate } from 'react-router-dom';
import { AuthHandler } from '../../../components/AuthHandler';
import { AppPermissions } from '../../../AppPermissions';
import { Status } from '../../../services/Clients';

export default function ApproveAcceptNotification() {
  const location = useLocation();
  const navigate = useNavigate();
  const [opened, { close }] = useDisclosure(true);

  const { data: count = 0, isSuccess } = useGetMappedEntriesCount([Status.Pending], undefined, undefined, {
    enabled: location.pathname !== '/review-twins', // do not fetch if we are on the review twins page
  });

  const showNotification = isSuccess && count > 0 && location.pathname !== '/review-twins';

  return (
    <AuthHandler requiredPermissions={[AppPermissions.CanReadMappings]}>
      <Modal
        closeOnClickOutside={false}
        centered
        opened={showNotification && opened}
        onClose={close}
        header="Review Twins"
      >
        <ModalContent>
          <div>
            <span>
              There are <b>{count}</b> new twins from <b>Edge Connector</b> awaiting your approval
            </span>
            <ButtonContainer>
              <Button kind="secondary" onClick={close}>
                Remind me later
              </Button>
              <Button
                kind="primary"
                onClick={() => {
                  close();
                  navigate('/review-twins');
                }}
              >
                Review Twins
              </Button>
            </ButtonContainer>
          </div>
        </ModalContent>
      </Modal>
    </AuthHandler>
  );
}

const ModalContent = styled('div')({ padding: '1rem' });

const ButtonContainer = styled('div')({
  display: 'flex',
  gap: 10,
  flexDirection: 'row',
  justifyContent: 'flex-end',
  marginTop: 24,
});
