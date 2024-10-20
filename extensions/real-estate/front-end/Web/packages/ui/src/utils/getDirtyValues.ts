/**
 * React-hook-form utility function to return an object of values that have been updated in the form. Also works on nested objects.
 * https://github.com/react-hook-form/react-hook-form/discussions/1991
 *
 * @param dirtyFields - object map of boolean values if a field is dirty (from formState)
 * @param allValues - object map of all field values from react hook form
 * @returns object of only dirty values ie. values in the field that have been updated
 */
const getDirtyValues = (allValues: any, dirtyFields: any): any => {
  // If *any* item in an array was modified, the entire array must be submitted, because there's no way to indicate
  // "placeholders" for unchanged elements. `dirtyFields` is `true` for leaves.
  if (dirtyFields === true || Array.isArray(dirtyFields)) return allValues
  // Here, we have an object
  return Object.fromEntries(
    Object.keys(dirtyFields).map((key) => [
      key,
      getDirtyValues(allValues[key], dirtyFields[key]),
    ])
  )
}

export default getDirtyValues
