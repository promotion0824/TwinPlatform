import { useLayoutEffect, useRef, useState } from 'react'
import _ from 'lodash'
import cx from 'classnames'
import { useWindowEventListener } from 'hooks'
import NotFound from 'components/NotFound/NotFound'
import { TableContext } from './TableContext'
import styles from './Table.css'

export default function Table({
  response,
  notFound,
  defaultSort,
  filter = (items) => items,
  pageSize = 30,
  className,
  tableClassName,
  children,
  ...rest
}) {
  const tableRef = useRef()

  const [state, setState] = useState({
    sort: undefined,
    order: 'asc',
  })
  const [pages, setPages] = useState(1)

  const cxClassName = cx(styles.table, className)
  const cxTableClassName = cx(styles.tableControl, tableClassName)

  function refresh() {
    if (_.isArray(response) && tableRef.current) {
      const maxPages = Math.ceil(response.length / pageSize)

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

  useLayoutEffect(() => {
    refresh()
  }, [pages])

  useWindowEventListener('resize', () => {
    refresh()
  })

  const context = {
    sort(sort) {
      setState((prevState) => ({
        ...prevState,
        sort,
        order:
          (prevState.sort === sort || _.isFunction(sort)) &&
          prevState.order === 'asc'
            ? 'desc'
            : 'asc',
      }))
    },
  }

  let sortedResponse = response
  if (response == null || _.isArray(response)) {
    if (state.sort == null && defaultSort != null) {
      sortedResponse = _.orderBy(response, defaultSort[0], defaultSort[1])
    } else {
      sortedResponse = _.orderBy(
        response,
        _.isFunction(state.sort)
          ? state.sort
          : (item) => {
              const value = _.get(item, state.sort) ?? ''

              return _.isFunction(value?.toLowerCase)
                ? value?.toLowerCase()
                : value
            },
        state.order
      )
    }
  }

  const filteredResponse = filter(sortedResponse)

  let content = children
  if (_.isFunction(children)) {
    if (notFound != null && filteredResponse.length === 0) {
      return <NotFound>{notFound}</NotFound>
    }

    const pagedResponse = _.isArray(filteredResponse)
      ? filteredResponse.slice?.(0, pages * pageSize)
      : filteredResponse

    content = children(pagedResponse)
  }

  return (
    <TableContext.Provider value={context}>
      <div {...rest} ref={tableRef} className={cxClassName} onScroll={refresh}>
        <div className={cxTableClassName}>{content}</div>
      </div>
    </TableContext.Provider>
  )
}
