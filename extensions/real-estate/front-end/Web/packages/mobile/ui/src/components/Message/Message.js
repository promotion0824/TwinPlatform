import Icon from 'components/Icon/Icon'
import Spacing from 'components/Spacing/Spacing'
import Text from 'components/Text/Text'
import styles from './Message.css'

export default function Message({ icon, children, whiteSpace, ...rest }) {
  return (
    <Spacing inline>
      <Spacing align="center middle" size="medium" horizontal {...rest}>
        {icon != null && <Icon icon={icon} size="large" />}
        <Text type="h3" whiteSpace={whiteSpace} className={styles.text}>
          {children}
        </Text>
      </Spacing>
    </Spacing>
  )
}
