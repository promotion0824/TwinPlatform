import { useEffect } from 'react'
import cx from 'classnames'
import { Flex } from '@willow/ui'
import { useTable } from './TableContext'
import { useHead } from './HeadContext'
import CellSortIcon from './CellSortIcon'
import styles from './Table.css'

export default function Cell({
  type = 'text',
  sort,
  className,
  children,
  ...rest
}) {
  const head = useHead()
  const table = useTable()

  const isClickable = sort != null

  useEffect(() => {
    if (head != null) {
      table.registerColumn()
    }

    return () => {
      if (head != null) {
        table.unregisterColumn()
      }
    }
  }, [])

  const cxClassName = cx(
    styles.cell,
    {
      [styles.headCell]: head != null,
      [styles.typeFill]: type === 'fill',
      [styles.typeNone]: type === 'none',
      [styles.typeWide]: type === 'wide',
      [styles.cellIsClickable]: isClickable,
    },
    className
  )

  return head != null ? (
    <th
      colSpan={type === 'wide' ? table.columns : undefined}
      {...rest}
      className={cxClassName}
      onClick={isClickable ? () => table.sort(sort) : undefined}
    >
      <Flex
        horizontal
        align="middle"
        overflow="hidden"
        size="medium"
        className={styles.headCellContent}
      >
        <span>{children}</span>
        <CellSortIcon sort={sort} />
      </Flex>
    </th>
  ) : (
    <td
      colSpan={type === 'wide' ? table.columns : undefined}
      {...rest}
      className={cxClassName}
    >
      {children}
    </td>
  )
}
