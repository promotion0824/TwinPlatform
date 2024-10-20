import cx from 'classnames'
import Number from 'components/Number/Number'
import Panel from 'components/Panel/Panel'
import Text from 'components/Text/Text'
import styles from './NumberPanel.css'

export default function NumberPanel({
  size = 'medium',
  value,
  label,
  format = ',',
  color,
  background,
  border = true,
  ...rest
}) {
  const cxClassName = cx(styles.numberPanel, {
    [styles.sizeSmall]: size === 'small',
    [styles.sizeMedium]: size === 'medium',
    [styles.colorGreen]: color === 'green',
    [styles.colorYellow]: color === 'yellow',
    [styles.colorRed]: color === 'red',
    [styles.border]: border,
  })

  return (
    <Panel
      align="center middle"
      position="relative"
      color={background}
      {...rest}
      className={cxClassName}
    >
      <Text size={size === 'medium' ? 'huge' : undefined} align="center">
        <Number value={value} format={format} />
      </Text>
      {label != null && (
        <Text align="center" color="text" className={styles.label}>
          {label}
        </Text>
      )}
    </Panel>
  )
}
