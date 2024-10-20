import { useCallback, useMemo } from 'react'

import useSearchParams from './useSearchParams'

export type ParamsDict = { [key: string]: string | string[] | undefined }
type UpdateParamsDict = { [key: string]: string | string[] | null }

export type NavigateOptions = {
  replace?: boolean
  // history state really is `any`.
  state: any // eslint-disable-line @typescript-eslint/no-explicit-any
}

/**
 * Retrieves and sets a subset of the values from the search parameters.
 *
 * Suppose the current search parameters are "myval=123,yourval=234". We can do:
 *
 *   const [{ myval, yourval }, setParams] = useMultipleSearchParams(['myval', 'yourval'])
 *   // myval is "123", yourval is "234"
 *
 * We can then:
 *   update just myval: `setParams({myval: "new my val"})`
 *   update just yourval: `setParams({yourval: "new your val"})`
 *   update both: `setParams({myval: "yay", yourval: "both"})`
 *   unset myval: `setParams({myval: null})`
 *   unset both: `setParams({myval: null, yourval: null})`
 *   do nothing: `setParams({})`
 *
 * Any search parameters not specified in the input to
 * `useMultipleSearchParams` will be untouched.
 *
 * We also support array values. If we do
 *
 *   const [{ myvals }, setParams] = useMultipleSearchParams([
 *     {
 *       name: "myvals",
 *       type: "array"
 *     }
 *   ])
 *
 * then `myvals` will be an array of strings, one for each time `myvals`
 * appears in the query string. `setParams` will also expect an array of
 * strings, and add one entry in the query string per item. Eg.
 *
 *   setParams({ myvals: ["a", "b", "c"] })
 *
 * will add "myvals=a&myvals=b&myvals=c".
 *
 * The `navigateOptions` parameter determines whether the query string will be updated
 * using `history.push` (default) or `history.replace`, and what `state` will be passed
 * to the history call, if any.
 *
 * The `setParams` callback also takes `navigateOptions` - if provided, this will be
 * used instead of the `navigateOptions` passed to `useMultipleSearchParams`.
 */
export default function useMultipleSearchParams(
  params: Array<string | { name: string; type?: 'array' | 'string' }>,
  navigateOptions?: NavigateOptions
) {
  const [searchParams, setSearchParams] = useSearchParams() as [
    URLSearchParams,
    (
      vals: URLSearchParams | ParamsDict,
      navigateOptions?: NavigateOptions // eslint-disable-line no-shadow
    ) => void
  ]

  const values = useMemo(() => {
    const vals: ParamsDict = {}
    for (const param of params) {
      if (typeof param === 'string') {
        vals[param] = searchParams.get(param) ?? undefined
      } else if (param.type === 'array') {
        vals[param.name] = searchParams.getAll(param.name) ?? undefined
      } else {
        vals[param.name] = searchParams.get(param.name) ?? undefined
      }
    }
    return vals
  }, [params, searchParams])

  const setValues = useCallback(
    (
      newValues: UpdateParamsDict,
      overrideNavigateOptions?: NavigateOptions
    ) => {
      const newSearchParams = new URLSearchParams(
        Array.from(searchParams.entries())
      )

      for (const [key, val] of Object.entries(newValues)) {
        if (val == null) {
          newSearchParams.delete(key)
        } else if (Array.isArray(val)) {
          newSearchParams.delete(key)
          for (const v of val) {
            newSearchParams.append(key, v)
          }
        } else {
          newSearchParams.set(key, val)
        }
      }
      setSearchParams(
        newSearchParams,
        overrideNavigateOptions ?? navigateOptions
      )
    },
    [searchParams, setSearchParams, navigateOptions]
  )

  return [values, setValues] as const
}
