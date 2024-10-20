import { useEffect } from 'react'
import _ from 'lodash'

/**
 * This hook is used to save user preferences when the user changes them.
 * e.g. when value in preferencesToSave changes, save the preference with the new value
 * @param preferenceNames - array of preference names
 * @param preferencesToSave - array of preference values to save
 * @param preferenceValues - array of existing preference values
 * @param save - function to save the preference
 */
export default function useSaveUserPreferences<T>({
  preferenceNames,
  preferencesToSave,
  preferenceValues,
  save,
}: {
  preferenceNames: string[]
  preferencesToSave: Array<T>
  preferenceValues: Array<T>
  save: (preference: string, value: T) => void
}) {
  useEffect(() => {
    preferenceNames.forEach((preferenceName: string, index) => {
      if (
        preferencesToSave[index] &&
        // comparison for primitive values
        preferencesToSave[index] !== preferenceValues[index] &&
        // comparison for objects
        !_.isEqual(preferencesToSave[index], preferenceValues[index])
      ) {
        save(preferenceName, preferencesToSave[index])
      }
    })
  }, [preferenceNames, preferenceValues, preferencesToSave, save])
}
