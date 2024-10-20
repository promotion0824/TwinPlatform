import { Icon, Spacing } from '@willow/mobile-ui'

export default function Validation({ isValid, children, ...rest }) {
  return (
    <Spacing {...rest} horizontal size="tiny">
      <Icon icon={isValid ? 'check' : 'error'} />
      <span>{children}</span>
    </Spacing>
  )
}
