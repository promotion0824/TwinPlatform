import { useState, useEffect } from 'react'
import _ from 'lodash'
import cx from 'classnames'
import { styled } from 'twin.macro'
import NotFound from '../NotFound/NotFound'
import PagedItems from '../PagedItems/PagedItems'
import Progress from '../Progress/Progress'
import Error from '../Error/Error'
import { TableContext } from './TableContext'
import useSort from './useSort/useSort'
import styles from './Table.css'

export { useTable } from './TableContext'
export { default as Head } from './Head'
export { default as Body } from './Body'
export { default as Row } from './Row'
export { default as Cell } from './Cell/Cell'

const Container = styled.div({
  display: 'flex',
  height: '100%',
  flexShrink: 0,
  flexDirection: 'column',
  alignItems: 'center',
  margin: 'auto',
  justifyCenter: 'center',
  padding: 'var(--padding-large)',
  overflow: 'hidden',
})

export default function Table({
  items = [],
  notFound,
  defaultSort,
  className,
  tableClassName,
  children,
  tableStyle,
  onItemsSortChange = () => {},
  isLoading,
  isError,
  ...rest
}) {
  const sort = useSort({ items, defaultSort })

  const [cells, setCells] = useState([])

  const cxClassName = cx(styles.table, className)
  const cxTableClassName = cx(styles.tableControl, tableClassName)

  useEffect(() => {
    onItemsSortChange(sort)
  }, [sort])

  const context = {
    ...sort,

    registerHeadCell(nextCell) {
      setCells((prevCells) =>
        _([...prevCells, nextCell])
          .map((cell) => ({
            ...cell,
            index: Array.from(cell.cellRef.current.parentNode.children).indexOf(
              cell.cellRef.current
            ),
          }))
          .orderBy((cell) => cell.index)
          .value()
      )
    },

    unregisterHeadCell(cellId) {
      setCells((prevCells) =>
        prevCells.filter((prevCell) => prevCell.cellId !== cellId)
      )
    },
  }

  const width = cells.every((cell) => cell.width == null) ? '1fr' : undefined

  const nextTableStyle = {
    gridTemplateColumns: cells
      .map((cell) => width ?? cell.width ?? 'min-content')
      .join(' '),
    ...tableStyle,
  }

  if (isError) {
    return (
      <Container>
        <Error />
      </Container>
    )
  }

  if (isLoading) {
    return (
      <Container>
        <Progress />
      </Container>
    )
  }

  if (notFound != null && items.length === 0) {
    return (
      <Container>
        <NotFound>{notFound}</NotFound>
      </Container>
    )
  }

  return (
    <TableContext.Provider value={context}>
      <PagedItems {...rest} items={sort.items} className={cxClassName}>
        {(pagedItems) => (
          <div className={cxTableClassName} style={nextTableStyle} role="table">
            {_.isFunction(children) ? children(pagedItems) : children}
          </div>
        )}
      </PagedItems>
    </TableContext.Provider>
  )
}
