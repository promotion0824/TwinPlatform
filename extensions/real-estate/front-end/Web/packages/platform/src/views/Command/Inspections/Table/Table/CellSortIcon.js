import cx from 'classnames'
import { Icon } from '@willow/ui'
import { useTable } from './TableContext'
import styles from './CellSortIcon.css'

export default function CellSortIcon({ sort }) {
  const table = useTable()

  const isClickable = sort != null

  if (!isClickable || sort !== table.sortState.sort) {
    return null
  }

  const cxClassName = cx(styles.cellSortIcon, {
    [styles.asc]: table.sortState.order === 'asc',
  })

  return <Icon icon="chevron" className={cxClassName} />
}
