import Message from 'components/Message/Message'
import Spacing from 'components/Spacing/Spacing'

export default function NotFound({ icon = 'notFound', children, ...rest }) {
  return (
    <Spacing height="100%">
      <Spacing align="center middle" padding="large" {...rest}>
        <Message icon={icon}>{children}</Message>
      </Spacing>
    </Spacing>
  )
}
