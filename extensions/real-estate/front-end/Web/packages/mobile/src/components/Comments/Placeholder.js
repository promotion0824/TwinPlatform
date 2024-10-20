import styles from './Placeholder.css'

export default function Placeholder({ placeholder = 'There are no comments' }) {
  return <div className={styles.placeholder}>{placeholder}</div>
}
