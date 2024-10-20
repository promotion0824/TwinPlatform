import _ from 'lodash'

export default function getSort(sort) {
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
