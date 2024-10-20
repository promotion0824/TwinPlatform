import { GridComparatorFn } from '@willowinc/ui'

// eslint-disable-next-line import/prefer-default-export
export const dateComparator: GridComparatorFn<string> = (v1, v2) =>
  new Date(v1).valueOf() - new Date(v2).valueOf()
