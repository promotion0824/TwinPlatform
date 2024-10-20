import Icon from 'components/Icon/Icon'
import Spacing from 'components/Spacing/Spacing'

export default function Loader({ padding = 'large', ...rest }) {
  return (
    <Spacing height="100%" {...rest}>
      <Spacing align="center middle" padding={padding}>
        <Icon icon="progress" aria-label="loading" />
      </Spacing>
    </Spacing>
  )
}
