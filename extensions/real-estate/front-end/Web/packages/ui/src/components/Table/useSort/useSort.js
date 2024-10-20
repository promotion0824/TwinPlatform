import { useState } from 'react'
import _ from 'lodash'

function getSort(sort) {
  let nextSort = sort ?? []
  if (_.isString(nextSort)) {
    nextSort = [nextSort]
  }

  if (_.isString(nextSort[0])) {
    nextSort = _(nextSort)
      .map((str) => {
        const split = str.split(' ')
        return [split[0], split[1] ?? 'asc']
      })
      .unzip()
      .value()
  }

  return nextSort
}

export default function useSort({ items, defaultSort }) {
  const [state, setState] = useState(() => ({
    sort: getSort(defaultSort),
    hasSorted: false,
  }))

  const sortedItems = _.orderBy(
    items,
    (state.sort[0] ?? []).map((key) => (item) => {
      const value = _.get(item, key) ?? ''

      if (_.isString(value)) {
        return value.toLowerCase()
      }

      return value
    }),
    state.sort[1]
  )

  return {
    items: sortedItems,
    sort: state.sort,
    hasSorted: state.hasSorted,

    sortItems(nextSortValue) {
      let nextSort = getSort(nextSortValue)

      if (_.isEqual(state.sort, nextSort) && nextSort.length > 0) {
        nextSort = [
          nextSort[0],
          [nextSort[1][0] === 'asc' ? 'desc' : 'asc', ...nextSort[1].slice(1)],
        ]
      }

      setState({
        sort: nextSort,
        hasSorted: true,
      })
    },
  }
}
