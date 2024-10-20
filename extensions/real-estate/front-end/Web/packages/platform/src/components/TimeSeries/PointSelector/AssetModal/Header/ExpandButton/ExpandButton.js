import cx from 'classnames'
import { Button, Flex, Icon } from '@willow/ui'
import styles from './ExpandButton.css'

export default function ExpandButton({ isOpen, ...rest }) {
  const cxClassName = cx(styles.expandButton, {
    [styles.isOpen]: isOpen,
  })

  return (
    <Button className={cxClassName} ripple {...rest}>
      <Flex>
        <Flex align="center middle" className={styles.expandIconContainer}>
          <Icon icon="chevron" className={styles.expandIcon} />
        </Flex>
      </Flex>
    </Button>
  )
}
