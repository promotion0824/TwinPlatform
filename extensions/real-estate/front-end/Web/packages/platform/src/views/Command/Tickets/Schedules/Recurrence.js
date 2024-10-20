import _ from 'lodash'

export const translationKeys = {
  minutely: {
    plural: 'plainText.minutes',
    singular: 'plainText.minute',
  },
  hourly: {
    plural: 'plainText.hours',
    singular: 'plainText.hour',
  },
  daily: {
    plural: 'plainText.days',
    singular: 'plainText.day',
  },
  weekly: {
    plural: 'plainText.weeks',
    singular: 'plainText.week',
  },
  monthly: {
    plural: 'plainText.months',
    singular: 'plainText.month',
  },
  yearly: {
    plural: 'plainText.years',
    singular: 'plainText.year',
  },
}

/**
 * Display eg. "Every 12 years", "Every week" (note: not "Every 1 week")
 */
export default function getRecurrence({ recurrence, t }) {
  if (recurrence != null) {
    const durationKey =
      translationKeys[recurrence.occurs][
        recurrence.interval > 1 ? 'plural' : 'singular'
      ]
    return _.capitalize(
      t('interpolation.everyDuration', {
        duration: t(durationKey, {
          // Ideally we would just have "count" here, but the translation values
          // currently use {{ num }} instead of count, so we need to pass the
          // number  twice - `count` so the translator will choose the right
          // singular / plural version and `num` so it will insert the correct
          // number. After we update the translation values we can remove `num`.
          count: recurrence.interval,
          num: recurrence.interval,
        }),
      })
    )
  } else {
    return ''
  }
}
