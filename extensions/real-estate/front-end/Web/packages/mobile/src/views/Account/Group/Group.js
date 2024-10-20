import styles from './Group.css'

export default function Group(props) {
  const { children } = props

  return <div className={styles.group}>{children}</div>
}
