import { useLayoutEffect, useRef } from 'react'
import cx from 'classnames'
import { useUniqueId } from '@willow/ui'
import Flex from 'components/Flex/Flex'
import Text from 'components/Text/Text'
import SortIcon from './SortIcon'
import { useTable } from '../TableContext'
import styles from '../Table.css'

export default function HeadCell({
  headType,
  type,
  align,
  width,
  sort,
  className,
  children,
  ...rest
}) {
  const table = useTable()
  const cellId = useUniqueId()

  const cellRef = useRef()

  const isClickable = sort != null

  useLayoutEffect(() => {
    if (headType === 'normal') {
      table.registerHeadCell({
        cellId,
        cellRef,
        width,
      })
    }

    return () => {
      if (headType === 'normal') {
        table.unregisterHeadCell(cellId)
      }
    }
  }, [width])

  const cxClassName = cx(
    styles.th,
    {
      [styles.headTypeHeader]: headType === 'header',
      [styles.typeFill]: type === 'fill',
      [styles.typeNone]: type === 'none',
      [styles.clickable]: isClickable,
      [styles.alignLeft]: align?.includes('left'),
      [styles.alignCenter]: align?.includes('center'),
      [styles.alignRight]: align?.includes('right'),
    },
    className
  )

  return (
    <div // eslint-disable-line
      ref={cellRef}
      {...rest}
      className={cxClassName}
      onClick={isClickable ? () => table.sortItems(sort) : undefined}
    >
      <Flex
        position="relative"
        align="middle"
        className={styles.headCellContent}
      >
        <Text size="small">{children}</Text>
        {type !== 'none' && <SortIcon sort={sort} />}
      </Flex>
    </div>
  )
}
