import { DateProximity } from '../types'
import { makeDateTextValue } from '../utils'

describe('makeDateTextValue', () => {
  it('should return "Today" for today\'s date', () => {
    const today = new Date().toISOString()
    expect(makeDateTextValue(today)).toBe(DateProximity.Today)
  })

  it('should return "Yesterday" for yesterday\'s date', () => {
    const yesterday = new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString()
    expect(makeDateTextValue(yesterday)).toBe(DateProximity.Yesterday)
  })

  it('should return "Older" for a date older than yesterday', () => {
    const olderDate = new Date(Date.now() - 48 * 60 * 60 * 1000).toISOString()
    expect(makeDateTextValue(olderDate)).toBe(DateProximity.Older)
  })
})
