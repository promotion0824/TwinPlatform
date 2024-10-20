import { IconNew, Text } from '@willow/ui'
import { styled } from 'twin.macro'

const Item = styled.div({
  backgroundColor: 'var(--theme-color-neutral-bg-accent-default)',
  display: 'inline-flex',
  alignItems: 'center',
  padding: 'var(--padding)',
  fontWeight: 'var(--font-weight-500)',
  maxWidth: 'calc(50% - 24px)',
  '> span': {
    margin: '0 var(--padding)',
  },
  '+ svg': {
    margin: '0 var(--padding)',
  },
  '&:only-child': {
    maxWidth: '100%',
  },
})

const HostedBy = styled(Text)({
  fontSize: 'var(--font-h2)',
  color: '#FAFAFA',
  whiteSpace: 'nowrap',
})

const Connector = styled(Text)({
  fontSize: 'var(--font-small)',
  color: 'var(--light)',
  whiteSpace: 'nowrap',
})

const StyledIcon = styled(IconNew)(({ theme }) => ({
  color: theme.color.neutral.fg.subtle,
}))

type CapabilityArea = {
  id: string
  name: string
}

export type GroupHeaderProps = {
  hostedBy?: CapabilityArea
  connector: CapabilityArea
}

const GroupArea = ({
  type,
  name,
}: {
  type: 'connector' | 'device'
  name: string
}) => (
  <Item title={name}>
    <StyledIcon icon={type} />
    {type === 'connector' ? (
      <Connector>{name}</Connector>
    ) : (
      <HostedBy>{name}</HostedBy>
    )}
    <IconNew icon="status" size="tiny" />
  </Item>
)

const GroupHeader = ({ hostedBy, connector }: GroupHeaderProps) => (
  <div data-testid="groupHeader">
    {hostedBy ? (
      <>
        <GroupArea type="device" name={hostedBy.name} />
        <StyledIcon icon="dashedLine" />
      </>
    ) : null}
    <GroupArea type="connector" name={connector.name} />
  </div>
)

export default GroupHeader
