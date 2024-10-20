import _ from 'lodash'
import { TFunction } from 'react-i18next'
import { Frequency } from './types'
import frequencyLabels from './frequencyLabels'

const limits: { [key in Frequency]: number } = {
  weekly: 52,
  monthly: 12,
  yearly: 10,
}

/**
 * Given a string `val`, does the string parse to an integer that is not less
 * than `min` and not greater than `max`?
 */
function isIntegerInRange(val: string, min: number, max: number) {
  const asInt = Number(val)
  return Number.isInteger(asInt) && min <= asInt && asInt <= max
}

/**
 * Return a list of validation errors for the recurrence object. Currently this
 * just checks that the `interval` attribute is in our expected range for the
 * given frequency unit (eg. weeks must be <= 52, months must be <= 12).
 */
export default function getRecurrenceValidationErrors(
  recurrence: {
    occurs: 'weekly' | 'monthly' | 'yearly'
    interval: string
  },
  t: TFunction
) {
  const occursLimit = limits[recurrence.occurs]
  if (
    occursLimit != null &&
    !isIntegerInRange(recurrence.interval, 1, occursLimit)
  ) {
    const quantity = t('interpolation.numberOf', {
      item: t(frequencyLabels[recurrence.occurs]),
    })
    const message = _.upperFirst(
      t('interpolation.valueMustBeBetween', {
        quantity,
        min: 1,
        max: occursLimit,
      })
    )
    return [{ name: 'recurrence', message }]
  } else {
    return []
  }
}
