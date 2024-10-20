import cx from 'classnames'
import { Flex, Icon, Number, Text } from '@willow/ui'
import styles from './SiteIcon.css'

export default function SiteIcon({
  size = 'medium',
  value,
  color,
  selected,
  isLoading = false,
  icon = 'building',
  ...rest
}) {
  const cxClassName = cx(styles.siteIcon, {
    [styles.sizeMedium]: size === 'medium',
    [styles.sizeLarge]: size === 'large',
    [styles.colorGreen]: color === 'green',
    [styles.colorYellow]: color === 'yellow',
    [styles.colorOrange]: color === 'orange',
    [styles.colorRed]: color === 'red',
    [styles.colorPrimary]: color === 'primary',
    [styles.selected]: selected,
    [styles.isLoading]: isLoading,
  })

  return (
    <Flex align="center middle" className={cxClassName} {...rest}>
      <svg
        viewBox="0 0 36 36"
        className={cx(styles.svg, { [styles.isLoading]: isLoading })}
      >
        {color === 'primary' && (
          <circle cx={18} cy={18} r={16} className={styles.trackPrimary} />
        )}
        {value != null && color != null && (
          <circle
            cx={18}
            cy={18}
            r={16}
            className={isLoading ? styles.animated : styles.value}
            style={{
              strokeDashoffset:
                100 - (value ?? 0) * 100 * (color === 'primary' ? 0 : 1),
            }}
          />
        )}
      </svg>
      <Flex align="center middle" className={styles.content}>
        {color === 'primary' ? (
          <Text size="large" className={styles.text}>
            <Number value={value} />
          </Text>
        ) : (
          <Icon icon={icon} size="large" color={`${color}Graph`} />
        )}
      </Flex>
    </Flex>
  )
}
