import Message from 'components/Message/Message'
import Spacing from 'components/Spacing/Spacing'

export default function Error({ children = 'An error has occurred', ...rest }) {
  return (
    <Spacing height="100%">
      <Spacing align="center middle" padding="large" {...rest}>
        <Message icon="error">{children}</Message>
      </Spacing>
    </Spacing>
  )
}
