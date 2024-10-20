import { Button, Badge, Icon } from '@willowinc/ui';
import useGetMappedEntriesCount from '../../../ReviewTwins/hooks/useGetMappedEntriesCount';
import './button.css';
import { useNavigate } from 'react-router-dom';
import { Status } from '../../../../services/Clients';
import styled from '@emotion/styled';

export default function ApproveAcceptNavButton() {
  const { data: count, isSuccess } = useGetMappedEntriesCount([Status.Pending], undefined, undefined, {});
  const navigate = useNavigate();
  return (
    <>
      {isSuccess ? (
        <Button
          kind="secondary"
          onClick={() => {
            navigate('/review-twins');
          }}
        >
          <Flex>
            Approve & Accept
            <Badge color="purple">{count}</Badge>
          </Flex>
        </Button>
      ) : (
        <Button
          kind="secondary"
          onClick={() => {
            navigate('/review-twins');
          }}
          prefix={<StyledIcon icon="info" />}
        >
          Review Pending Twins
        </Button>
      )}
    </>
  );
}

const StyledIcon = styled(Icon)({ fontVariationSettings: `'FILL' 1,'wght' 400,'GRAD' 200,'opsz' 20 !important` });
const Flex = styled('span')({
  display: 'flex',
  flexDirection: 'row',
  justifyContent: 'space-between',
  alignItems: 'center',
  gap: '0.25rem',
});
