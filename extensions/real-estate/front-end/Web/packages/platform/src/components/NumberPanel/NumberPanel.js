import cx from 'classnames'
import { Icon, Number, Panel, Text } from '@willow/ui'
import styles from './NumberPanel.css'

export default function NumberPanel({
  icon,
  value,
  color = 'green',
  size,
  children,
}) {
  const cxClassName = cx(styles.numberPanel, {
    [styles.sizeSmall]: size === 'small',
    [styles.colorGreen]: color === 'green',
    [styles.colorRed]: color === 'red',
  })

  return (
    <Panel align="center middle" padding="medium" className={cxClassName}>
      <Icon icon={icon} size="large" className={styles.icon} />
      <Text size="huge" className={styles.value}>
        <Number value={value} format="," />
      </Text>
      {children}
    </Panel>
  )
}
