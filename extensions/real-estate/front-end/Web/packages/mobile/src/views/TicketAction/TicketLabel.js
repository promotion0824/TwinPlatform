import styles from './TicketLabel.css'

export default function TicketLabel({ label, children }) {
  return (
    <div className={styles.ticketLabel}>
      <p className={styles.label}>{label}</p>
      <div className={styles.content}>{children}</div>
    </div>
  )
}
