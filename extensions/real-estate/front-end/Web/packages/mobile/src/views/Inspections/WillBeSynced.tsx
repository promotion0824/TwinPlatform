import { Icon } from '@willow/mobile-ui'
import { styled } from 'twin.macro'

const Container = styled.div`
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  height: 75px;
  color: #d9d9d9;
  padding: 11px 0;
  background-color: #5340d6;
  display: flex;
`

const Left = styled.div`
  flex: 1;
`

const Middle = styled.div`
  flex: 2;
  display: flex;
  justify-content: center;
`

const Right = styled.div`
  flex: 1;
  text-align: right;
  padding-right: 22px;
`

export default function WillBeSynced({ onClick }: { onClick: () => void }) {
  return (
    <Container>
      <Left />
      <Middle>
        <Icon icon="cloudOff" style={{ marginRight: '1em' }} />
        Check data will be synced when you are back online
      </Middle>
      <Right>
        <Icon icon="close" onClick={onClick} />
      </Right>
    </Container>
  )
}
