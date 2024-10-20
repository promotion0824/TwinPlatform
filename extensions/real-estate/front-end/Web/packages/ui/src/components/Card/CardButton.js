import Button from 'components/Button/Button'
import styles from './Card.css'

export default function CardButton({ ...rest }) {
  return <Button {...rest} tabIndex={-1} className={styles.cardButton} />
}
