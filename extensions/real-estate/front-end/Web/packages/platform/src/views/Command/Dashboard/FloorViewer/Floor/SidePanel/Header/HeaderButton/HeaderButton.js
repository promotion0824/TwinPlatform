import cx from 'classnames'
import { Button, Flex, Icon, Text } from '@willow/ui'
import styles from './HeaderButton.css'

export default function HeaderButton({
  icon,
  children,
  onPanelToggle = () => {},
  name: nameProp,
  tab,
  onTabChange = () => {},
  ...rest
}) {
  const name = nameProp ?? icon
  const selected = tab === name

  const cxClassName = cx(styles.headerButton, {
    [styles.isSelected]: selected,
  })

  function handleClick() {
    onPanelToggle(!selected)
    onTabChange(name)
  }

  return (
    <Button
      ripple
      {...rest}
      selected={selected}
      className={cxClassName}
      onClick={handleClick}
    >
      <Flex
        horizontal
        fill="content hidden"
        align="middle"
        padding="0 medium 0 0"
        overflow="hidden"
      >
        <Flex align="center middle" className={styles.iconContainer}>
          <Icon icon={icon} />
        </Flex>
        <Text whiteSpace="nowrap">{children}</Text>
      </Flex>
    </Button>
  )
}
