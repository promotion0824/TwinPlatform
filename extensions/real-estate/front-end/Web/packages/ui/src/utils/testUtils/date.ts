import { DateTime } from 'luxon'

/**
 * Helper function to assert the expected DateTime (ISO 8601-compliant)
 * is close to the actual DateTime (ISO 8601-compliant).
 */
export const assertDateTimeClose = (
  actualDateTime: string,
  expectedDateTime: string
) => {
  expect(
    DateTime.fromISO(expectedDateTime).diff(
      DateTime.fromISO(actualDateTime),
      'minutes'
    ).minutes
  ).toBeCloseTo(0)
}

/**
 * Helper function to assert the expected date range (from:to ISO 8601-compliant)
 * is close to the actual date range (from:to ISO 8601-compliant)
 */
export const assertDateTimeRangeClose = (
  actualDateRange: [string, string],
  expectedDateRange: [string, string]
) => {
  assertDateTimeClose(actualDateRange[0], expectedDateRange[0])
  assertDateTimeClose(actualDateRange[1], expectedDateRange[1])
}
