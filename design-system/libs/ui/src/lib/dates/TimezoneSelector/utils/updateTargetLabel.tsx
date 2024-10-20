import { ComboboxData } from '@mantine/core'
import { isArray, isObject, isString } from 'lodash'

import { SelectItem } from '../../../inputs/Select'
import { getLocalTimezone } from './getLocalTimezone'

/**
 * Will find the option which matches the value as `targetValue`,
 * and update its label with the result of `generateLabel(label)`.
 * Currently, it will default as appending the label with " (browser default)"
 * when the option value is the browser's default timezone.
 */
export const updateTargetLabel = (
  options: ComboboxData,
  targetValue: string = getLocalTimezone(),
  generateLabel = (label: string) => `${label} (browser default)`
) =>
  options.map((option) => {
    if (isString(option) || 'value' in option) {
      return replaceLabelForOption(option, targetValue, generateLabel)
    }

    if ('items' in option && option.items && isArray(option.items)) {
      return {
        ...option,
        items: option.items.map((item) =>
          replaceLabelForOption(item, targetValue, generateLabel)
        ),
      }
    }

    return option
  })

export const replaceLabelForOption = (
  option: string | SelectItem,
  targetValue: string,
  generateLabel: (label: string) => string
) => {
  if (isString(option) && option === targetValue) {
    return {
      value: option,
      label: generateLabel(option),
    }
  }

  if (isObject(option) && option.value === targetValue) {
    return {
      ...option,
      label: generateLabel(option.label),
    }
  }

  return option
}
