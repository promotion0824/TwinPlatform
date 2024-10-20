import { useParams } from 'react-router'
import cx from 'classnames'
import { useModal, Button } from '@willow/ui'
import { useDashboard } from '../DashboardContext'
import styles from './FloorSelectButton.css'

export default function FloorSelectButton({ floor }) {
  const dashboard = useDashboard()
  const modal = useModal()
  const params = useParams()

  const cxClassName = cx(styles.floor, {
    [styles.colorRed]: floor.insightsHighestPriority === 1,
    [styles.colorOrange]: floor.insightsHighestPriority === 2,
    [styles.colorYellow]: floor.insightsHighestPriority === 3,
  })

  return (
    <Button
      key={floor.id}
      to={`/sites/${params.siteId}/floors/${floor.id}${
        !dashboard.isReadOnly ? '?admin=true' : ''
      }`}
      className={cxClassName}
      onClick={() => modal.close()}
    >
      {floor.code}
    </Button>
  )
}
