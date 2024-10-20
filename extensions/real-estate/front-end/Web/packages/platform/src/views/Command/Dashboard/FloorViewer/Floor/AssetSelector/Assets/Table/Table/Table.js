import { useLayoutEffect, useRef, useState } from 'react'
import _ from 'lodash'
import cx from 'classnames'
import { useDebounce, useWindowEventListener, NotFound } from '@willow/ui'
import { TableContext } from './TableContext'
import styles from './Table.css'

export default function Table({
  items,
  notFound,
  sort: sortProp,
  defaultSort,
  filter = (nextItems) => nextItems,
  pageSize = 30,
  className,
  tableClassName,
  children,
  ...rest
}) {
  const tableRef = useRef()

  const [columns, setColumns] = useState(0)
  const [sortState, setSortState] = useState({
    sort: sortProp,
    order: 'asc',
  })
  const [pages, setPages] = useState(1)
  const [openIds, setOpenIds] = useState([])

  let sortedItems = items
  if (items == null || Array.isArray(items)) {
    if (sortState.sort == null && defaultSort != null) {
      sortedItems = _.orderBy(items, defaultSort[0], defaultSort[1])
    } else {
      sortedItems = _.orderBy(
        items,
        _.isFunction(sortState.sort)
          ? sortState.sort
          : (item) => {
              const value = _.get(item, sortState.sort) ?? ''

              return _.isFunction(value?.toLowerCase)
                ? value?.toLowerCase()
                : value
            },
        sortState.order
      )
    }
  }

  const filteredItems = filter(sortedItems)

  function refresh() {
    if (_.isArray(filteredItems) && tableRef.current) {
      const maxPages = Math.ceil(filteredItems.length / pageSize)

      if (pages < maxPages) {
        const scrollBottom =
          tableRef.current.scrollTop + tableRef.current.offsetHeight
        if (scrollBottom === tableRef.current.scrollHeight) {
          return
        }

        if (scrollBottom >= tableRef.current.scrollHeight - 50) {
          setPages((prevPages) => prevPages + 1)
        }
      }
    }
  }

  const debouncedRefresh = useDebounce(refresh, 200)

  useLayoutEffect(() => {
    debouncedRefresh()
  }, [pages])

  useWindowEventListener('resize', () => {
    debouncedRefresh()
  })

  const context = {
    columns,

    sortState,

    sort(sort) {
      setSortState((prevSortState) => ({
        ...prevSortState,
        sort,
        order:
          (prevSortState.sort === sort || _.isFunction(sort)) &&
          prevSortState.order === 'asc'
            ? 'desc'
            : 'asc',
      }))
    },

    toggleIsOpen(id) {
      setOpenIds((prevOpenIds) => _.xor(prevOpenIds, [id]))
    },

    isOpen(id) {
      return openIds.includes(id)
    },

    registerColumn() {
      setColumns((prevColumns) => prevColumns + 1)
    },

    unregisterColumn() {
      setColumns((prevColumns) => prevColumns - 1)
    },
  }

  let content = children
  if (_.isFunction(children)) {
    if (notFound != null && filteredItems.length === 0) {
      return <NotFound>{notFound}</NotFound>
    }

    const pagedResponse = _.isArray(filteredItems)
      ? filteredItems.slice?.(0, pages * pageSize)
      : filteredItems

    content = children(pagedResponse)
  }

  const cxClassName = cx(styles.table, className)
  const cxTableClassName = cx(styles.tableComponent, tableClassName)

  return (
    <TableContext.Provider value={context}>
      <div {...rest} ref={tableRef} className={cxClassName} onScroll={refresh}>
        <table className={cxTableClassName}>{content}</table>
      </div>
    </TableContext.Provider>
  )
}
