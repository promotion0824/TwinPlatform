import { numberUtils, Number } from '@willow/ui'

export default function Check({ check }) {
  if (check.statistics.lastCheckSubmittedEntry == null) {
    return '-'
  }

  if (
    check.type !== 'List' &&
    numberUtils.parse(check.statistics.lastCheckSubmittedEntry) != null
  ) {
    return (
      <Number
        value={check.statistics.lastCheckSubmittedEntry}
        format={`,.${'0'.repeat(check.decimalPlaces ?? 0)}`}
      />
    )
  }

  return check.statistics.lastCheckSubmittedEntry
}
