import Flex from 'components/Flex/Flex'
import Icon from 'components/Icon/Icon'

export default function Progress({ progressClassName = undefined, ...rest }) {
  return (
    <Flex height="100%" {...rest}>
      <Flex align="center middle" padding="large" className={progressClassName}>
        <Icon icon="progress" aria-label="loading" role="img" />
      </Flex>
    </Flex>
  )
}
