import { findIndex } from 'lodash'
import { generateOptions } from './generateTimezones'
import { mockedTimezones } from './mockedTimezones'

const findOption =
  (str: string) =>
  ({ label }: { label: string }) =>
    label.includes(`(${str})`)

describe('return order of generateOptions', () => {
  const timezones = generateOptions(mockedTimezones)

  it('should have AKST/AKDT (due to daylight time) as first option', () => {
    expect(timezones[0].label).toMatch(/AKST|AKDT/)
  })

  it('should have GMT listed before GMT+1', () => {
    expect(findIndex(timezones, findOption('GMT'))).toBeLessThan(
      findIndex(timezones, findOption('GMT+1'))
    )
  })

  it('should have GMT+1 listed before GMT+10', () => {
    expect(findIndex(timezones, findOption('GMT+1'))).toBeLessThan(
      findIndex(timezones, findOption('GMT+10'))
    )
  })

  it('should have GMT+2 listed before GMT+10', () => {
    expect(findIndex(timezones, findOption('GMT+2'))).toBeLessThan(
      findIndex(timezones, findOption('GMT+10'))
    )
  })

  it('should have GMT-10 listed before GMT-1', () => {
    expect(findIndex(timezones, findOption('GMT-10'))).toBeLessThan(
      findIndex(timezones, findOption('GMT-1'))
    )
  })

  it('should have GMT-1 listed before GMT+1', () => {
    expect(findIndex(timezones, findOption('GMT-1'))).toBeLessThan(
      findIndex(timezones, findOption('GMT+1'))
    )
  })

  it('should have PST/PDT (due to daylight time) listed as last option', () => {
    expect(timezones[timezones.length - 1].label).toMatch(/PST|PDT/)
  })
})
