import { Button, Flex, Icon } from '@willow/ui'
import styles from './CategoryButton.css'

export default function CategoryButton({ children, ...rest }) {
  return (
    <Button
      color="purple"
      width="medium"
      className={styles.categoryButton}
      {...rest}
    >
      <Flex horizontal fill="header hidden" align="middle" width="100%">
        <span>{children}</span>
        <Icon icon="right" />
      </Flex>
    </Button>
  )
}
