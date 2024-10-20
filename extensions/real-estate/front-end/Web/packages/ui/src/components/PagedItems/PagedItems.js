import { useLayoutEffect, useRef, useState } from 'react'
import _ from 'lodash'
import { useDebounce, useWindowEventListener } from '@willow/ui'

const PAGE_SIZE = 30

export default function PagedItems({ items, children, ...rest }) {
  const tableRef = useRef()

  const [pages, setPages] = useState(1)

  function refresh() {
    if (tableRef.current == null) {
      return
    }

    if (Array.isArray(items)) {
      const maxPages = Math.ceil(items.length / PAGE_SIZE)

      if (pages < maxPages) {
        const scrollBottom =
          tableRef.current.scrollTop + tableRef.current.offsetHeight

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

  let content = children

  if (_.isFunction(children)) {
    const pagedItems = Array.isArray(items)
      ? items.slice(0, pages * PAGE_SIZE)
      : items

    content = children(pagedItems)
  }

  return (
    <div {...rest} ref={tableRef} onScroll={refresh}>
      {content}
    </div>
  )
}
