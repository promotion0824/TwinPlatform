/// <reference types="jest" />

const originalDate = Date

/**
 * Will replace the new Date() behavior to return the mockedDate,
 * other functionalities like Date('2020-01-01') will not be impacted.
 */
export function mockNewDate(mockedDate: Date) {
  jest.spyOn(global, 'Date').mockImplementation((date) => {
    if (date) {
      return new originalDate(date)
    }

    return mockedDate
  })
}

export function dateStringEquals(
  date: Date,
  { year, month, day, hours, minutes }: Record<string, number>
) {
  expect(date.getFullYear()).toEqual(year)
  expect(date.getMonth()).toEqual(month)
  expect(date.getDate()).toEqual(day)
  expect(date.getHours()).toEqual(hours)
  expect(date.getMinutes()).toEqual(minutes)
}
