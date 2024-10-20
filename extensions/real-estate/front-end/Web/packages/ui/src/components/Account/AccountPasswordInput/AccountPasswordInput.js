import cx from 'classnames'
import PasswordInput from 'components/PasswordInput/PasswordInput'
import styles from './AccountPasswordInput.css'

export default function AccountPasswordInput({
  className,
  passwordInputClassName,
  ...rest
}) {
  const cxClassName = cx(styles.accountPasswordInput, className)
  const cxPasswordInputClassName = cx(
    styles.passwordInput,
    passwordInputClassName
  )

  return (
    <PasswordInput
      {...rest}
      className={cxClassName}
      passwordInputClassName={cxPasswordInputClassName}
      iconClassName={styles.icon}
      inputClassName={styles.input}
      buttonClassName={styles.button}
    />
  )
}
