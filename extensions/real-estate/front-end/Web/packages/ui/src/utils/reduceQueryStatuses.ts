import { UseQueryResult } from 'react-query'
import _ from 'lodash'

/**
 * takes an array of react query statuses and:
 * - return 'error' if the array contains 1 error status
 * - return 'loading' if the array contains no error status but 'loading'/'idle' status
 * - return last status if none of above criterias are met, if last status isn't defined, return 'loading'
 */
export default function reduceQueryStatuses(
  queries: Array<UseQueryResult['status']>
) {
  if (queries.includes('error')) {
    return 'error'
  }
  if (queries.some((q) => q === 'loading' || q === 'idle')) {
    return 'loading'
  }
  return _.last(queries) || 'loading'
}
