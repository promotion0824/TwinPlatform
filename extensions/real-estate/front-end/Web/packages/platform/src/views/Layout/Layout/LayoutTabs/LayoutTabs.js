import cx from 'classnames'
import styles from './LayoutTabs.css'

export default function LayoutTabs({ type, children }) {
  const cxClassName = cx(styles.layoutTabs, {
    [styles.typeOffCenter]: type === 'offCenter',
  })

  return (
    <div className={cxClassName}>
      <div className={styles.content}>{children}</div>
    </div>
  )
}
