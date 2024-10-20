import styles from './TicketLabel.css'

export default function TicketLabel({ label, value }) {
  return (
    <div className={styles.ticketLabel}>
      <p className={styles.label}>{label}</p>
      <div className={styles.content}>{value}</div>
    </div>
  )
}
