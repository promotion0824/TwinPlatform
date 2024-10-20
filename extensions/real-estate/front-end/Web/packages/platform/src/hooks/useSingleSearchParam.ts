import useMultipleSearchParams, {
  NavigateOptions,
} from '@willow/common/hooks/useMultipleSearchParams'
import { useCallback } from 'react'

/**
 * Allows you to get and set one URL search parameter without affecting the others.
 * Setting the parameter to undefined or null will remove it.
 * Array parameters set to empty arrays will also be removed.
 */
export default function useSingleSearchParam(
  name: string,
  isArray?: boolean,
  navigateOptions?: NavigateOptions
) {
  const [vals, setVals] = useMultipleSearchParams([
    {
      name,
      type: isArray ? 'array' : 'string',
    },
  ])

  const setValue = useCallback(
    (
      newValue: string | string[] | null,
      overrideNavigateOptions?: NavigateOptions
    ) => {
      setVals({ [name]: newValue }, overrideNavigateOptions ?? navigateOptions)
    },
    [name, setVals, navigateOptions]
  )
  return [vals[name], setValue] as const
}
