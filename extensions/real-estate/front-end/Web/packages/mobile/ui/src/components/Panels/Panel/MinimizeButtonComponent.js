import Button from 'components/Button/Button'
import { usePanels } from '../PanelsContext'
import styles from './MinimizeButtonComponent.css'

export default function MinimizeButtonComponent({ name }) {
  const panels = usePanels()

  return (
    <Button
      icon="menu"
      iconClassName={styles.minimize}
      onClick={() => panels?.minimizePanel(name)}
    />
  )
}
