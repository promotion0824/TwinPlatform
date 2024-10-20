import Button from 'components/Button/Button'
import styles from './AccountLinkButton.css'

export default function AccountLinkButton({ children, ...rest }) {
  return (
    <Button {...rest} ripple className={styles.accountLinkButton}>
      {children}
    </Button>
  )
}
