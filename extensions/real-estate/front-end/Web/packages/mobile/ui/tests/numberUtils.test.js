import numberUtils from '../src/utils/numberUtils'

test('numberUtils format', () => {
  expect(numberUtils.format(123)).toBe('123')
  expect(numberUtils.format(123.456789)).toBe('123.456789')
  expect(numberUtils.format(123, '0.00')).toBe('123.00')
  expect(numberUtils.format(123.456789, '0.00')).toBe('123.46')
  expect(numberUtils.format(123, '0.[00]')).toBe('123')
  expect(numberUtils.format(123.456789, '0.[00]')).toBe('123.46')
  expect(numberUtils.format(NaN)).toBe('')
  expect(numberUtils.format(Infinity)).toBe('')
  expect(numberUtils.format(-Infinity)).toBe('')
  expect(numberUtils.format('invalid')).toBe('')
  expect(numberUtils.format(undefined)).toBe('')
  expect(numberUtils.format(null)).toBe('')
})
