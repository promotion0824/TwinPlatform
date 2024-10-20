import cx from 'classnames'
import Button from 'components/Button/Button'
import { usePanels } from '../PanelsContext'
import styles from './MinimizeButton.css'

export default function MinimizeButton(props) {
  const { name } = props

  const panels = usePanels()

  const cxClassName = cx(styles.minimizeButton, {
    [styles.vertical]: !panels.horizontal,
    [styles.horizontal]: panels.horizontal,
  })

  return (
    <Button
      icon="menu"
      className={cxClassName}
      onClick={() => panels.minimizePanel(name, false)}
    />
  )
}
