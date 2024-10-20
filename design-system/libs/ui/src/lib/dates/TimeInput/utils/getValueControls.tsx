/**
 *
 * This function will determine what value or defaultValue will be assigned to
 * the input element, so that an input will not have both value and defaultValue
 * at the same time. However, if user passing both value and defaultValue, it will
 * pass all of them to input, so that user will get warnings about being both
 * controlled and uncontrolled.
 */
export const getValueControls = ({
  externalValue,
  defaultValue,
  internalValue,
}: {
  externalValue?: string
  defaultValue?: string
  internalValue: string
}) => {
  if (externalValue) {
    // user controlled
    // passing both so that user will get warning about having
    // both value and defaultValue
    return { defaultValue, value: externalValue }
  }

  if (!defaultValue) {
    // not defaultValue and not controlled by user
    return { value: internalValue }
  }

  if (internalValue) {
    // user has made a selection but not controlled by user
    return { value: internalValue }
  }

  // user not controlled and no selection made
  // cannot use defaultValue as it will change to value if user make an selection,
  // so we use value to mock defaultValue here.
  return { value: defaultValue }
}
