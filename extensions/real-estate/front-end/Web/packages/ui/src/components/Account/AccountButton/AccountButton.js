import cx from 'classnames'
import { useForm } from 'components/Form/Form'
import Button from 'components/Button/Button'
import styles from './AccountButton.css'

export default function AccountButton({ disabled, children, ...rest }) {
  const form = useForm()

  const cxClassName = cx(styles.accountButton, {
    [styles.disabled]: disabled,
    [styles.loading]: form.isSubmitting,
  })

  return (
    <Button
      {...rest}
      color="purple"
      disabled={disabled}
      className={cxClassName}
    >
      {children}
    </Button>
  )
}
