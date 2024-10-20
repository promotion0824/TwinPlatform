import cx from 'classnames'
import Input from 'components/Input/Input'
import styles from './AccountInput.css'

export default function AccountInput({ className, ...rest }) {
  const cxClassName = cx(styles.accountInput, className)

  return (
    <Input
      {...rest}
      className={cxClassName}
      iconClassName={styles.icon}
      inputClassName={styles.input}
    />
  )
}
