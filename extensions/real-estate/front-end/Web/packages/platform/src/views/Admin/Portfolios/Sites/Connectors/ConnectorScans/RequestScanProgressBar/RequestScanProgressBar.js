import styles from './RequestScanProgressBar.css'

export default function ScanProgressBar() {
  return (
    <div className={styles.outerContainer}>
      <div className={styles.labelContainer}>
        <span className={styles.label}>Requesting...</span>
      </div>
      <div className={styles.container}>
        <div className={styles.filler} />
      </div>
    </div>
  )
}
