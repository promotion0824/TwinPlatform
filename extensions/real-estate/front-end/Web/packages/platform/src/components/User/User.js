import styles from './User.css'

export default function User({ user }) {
  const firstInitial = user.firstName?.[0] ?? ''
  const lastInitial = user.lastName?.[0] ?? ''

  return (
    <div className={styles.user}>
      {firstInitial}
      {lastInitial}
    </div>
  )
}
