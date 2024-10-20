import cx from 'classnames'
import { Button } from '@willow/ui'
import { useFloor } from '../../../FloorContext'
import styles from './AdminTabsButton.css'

export default function TabsButton({ mode, ...rest }) {
  const floor = useFloor()

  const selected = floor.mode === mode

  const cxClassName = cx(styles.tabsButton, {
    [styles.selected]: selected,
  })

  return (
    <Button
      {...rest}
      iconSize="small"
      selected={selected}
      className={cxClassName}
      onClick={() => floor.selectMode(mode)}
    />
  )
}
