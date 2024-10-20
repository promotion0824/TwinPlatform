import cx from 'classnames'
import Spacing from 'components/Spacing/Spacing'
import styles from './Header.css'

export default function Header({ className, children, ...rest }) {
  const cxClassName = cx(styles.header, className)

  return (
    <Spacing
      horizontal
      size="medium"
      padding="large"
      {...rest}
      className={cxClassName}
    >
      {children}
    </Spacing>
  )
}
