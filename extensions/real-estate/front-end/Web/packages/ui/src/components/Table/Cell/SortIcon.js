import cx from 'classnames'
import Flex from 'components/Flex/Flex'
import Icon from 'components/Icon/Icon'
import { useTable } from '../TableContext'
import getSort from '../useSort/getSort'
import styles from './SortIcon.css'

export default function SortIcon({ sort }) {
  const table = useTable()

  const sortOrder =
    table.sort[0]?.length === 1 && table.sort[0]?.[0] === getSort(sort)[0]?.[0]
      ? table.sort[1]?.[0]
      : undefined

  const isAscending = sortOrder === 'asc'
  const isDescending = sortOrder === 'desc'

  const cxClassName = cx({
    [styles.ascending]: isAscending,
    [styles.descending]: isDescending,
    [styles.hidden]: (!isAscending && !isDescending) || !table.hasSorted,
  })

  return (
    <Flex
      position="absolute"
      align={isAscending ? 'center top' : 'center bottom'}
      className={cxClassName}
    >
      <Icon icon="chevron" className={styles.chevron} />
    </Flex>
  )
}
