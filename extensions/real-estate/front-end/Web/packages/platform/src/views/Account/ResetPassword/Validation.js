import { Flex, Icon } from '@willow/ui'

export default function Validation({ isValid, children, ...rest }) {
  return (
    <Flex {...rest} horizontal align="middle" size="small">
      <Icon icon={isValid ? 'ok' : 'error'} />
      <span>{children}</span>
    </Flex>
  )
}
