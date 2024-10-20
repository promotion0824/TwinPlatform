import cx from 'classnames'
import Flex from 'components/Flex/Flex'
import styles from './Footer.css'

export default function Footer({ className, children, ...rest }) {
  const cxClassName = cx(styles.footer, className)

  return (
    <Flex
      horizontal
      fill="equal"
      align="center"
      padding="large"
      size="medium"
      {...rest}
      className={cxClassName}
    >
      {children}
    </Flex>
  )
}
