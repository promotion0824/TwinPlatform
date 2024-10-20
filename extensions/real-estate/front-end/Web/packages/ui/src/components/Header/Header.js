import cx from 'classnames'
import Flex from 'components/Flex/Flex'
import styles from './Header.css'

export default function Header({ type, className, children, ...rest }) {
  const cxClassName = cx(
    styles.header,
    {
      [styles.typeTab]: type === 'tab',
    },
    className
  )

  return (
    <Flex
      horizontal
      size="medium"
      align={type === 'tab' ? 'middle' : undefined}
      padding={type === 'tab' ? '0 medium' : 'large'}
      {...rest}
      className={cxClassName}
    >
      {children}
    </Flex>
  )
}
