import _ from 'lodash'
import { JsonDict } from '@willow/common/twins/view/twinModel'
import isPlainObject from '@willow/common/utils/isPlainObject'

/**
 * Suppose that we started editing object `base`. We made edits and the new
 * value is `mine`. We tried to save but our save was rejected because in the meanwhile
 * someone changed the object to `theirs`.
 *
 * Return `mine` but with fields updated to the respective values from
 * `theirs`, for all fields where `theirs` differs from `base`.
 *
 * Ie. suppose `base` is `{a: 0, b: 0}`. We change `a` to 1, giving `mine`
 * as `{a: 1, b: 0}`. Another user (User B) changed `b` to 1 giving `theirs` as
 * `{a: 0, b: 1}`. The result will be `{a: 1, b: 1}`. If User B removed the value
 * in `b`, we will ensure this is set to `null` to ensure value is removed from
 * the field via the `reset` method from React Hook Form, and the result will be
 * `{a:1, b: null}` -- {@link TwinEditorContext.populateDeletedFields}
 *
 * We return `{result, conflictedFields}` where `result` is the updated object
 * and `conflictedFields` shows which fields were updated. In the example above
 * it will be `{a: false, b: true}`.
 *
 * This function is applied recursively.
 */
export default function merge(
  base: JsonDict,
  mine: JsonDict,
  theirs: JsonDict
): {
  result: JsonDict
  conflictedFields: JsonDict
} {
  const result: JsonDict = {}
  const conflictedFields: JsonDict = {}

  const allKeys = new Set([
    ...Object.keys(base),
    ...Object.keys(mine),
    ...Object.keys(theirs),
  ])

  for (const key of allKeys) {
    const v = base[key] ?? mine[key] ?? theirs[key]
    if (typeof v === 'object' && v != null) {
      // Recurse into sub-objects
      const inBase = base[key] ?? {}
      const inMine = mine[key] ?? {}
      const inTheirs = theirs[key] ?? {}

      if (
        isPlainObject(inBase) &&
        isPlainObject(inMine) &&
        isPlainObject(inTheirs)
      ) {
        const subResult = merge(inBase, inMine, inTheirs)
        result[key] = subResult.result
        conflictedFields[key] = subResult.conflictedFields
      } else {
        throw new Error('Value was changed between object and non-object')
      }
    } else if (theirs[key] === base[key]) {
      result[key] = mine[key]
      conflictedFields[key] = false
    } else {
      result[key] = theirs[key] ?? null
      conflictedFields[key] = true
    }
  }

  return {
    result,
    conflictedFields,
  }
}
