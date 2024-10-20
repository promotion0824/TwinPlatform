import numberUtils from '../numberUtils'

describe('numberUtils', () => {
  const initOptions = (min: number, max: number) => {
    return { min: min, max: max }
  }

  test('parse valid number', () => {
    expect(numberUtils.parse('123')).toBe(123)
    expect(numberUtils.parse('-123')).toBe(-123)
  })

  test('parse valid number with formatting', () => {
    expect(numberUtils.parse('123', '0.00')).toBe(123)
    expect(numberUtils.parse('123.456', '0.00')).toBe(123.46)
  })

  test('parse valid number with simple min/max options', () => {
    const options = initOptions(-50, 100)
    expect(numberUtils.parse('123', null, options)).toBe(100)
    expect(numberUtils.parse('-123', null, options)).toBe(-50)
  })

  test('parse valid number with negative, decimal min/max options', () => {
    const options = initOptions(-23.456, -12.345)
    expect(numberUtils.parse('-13', null, options)).toBe(-13)
    expect(numberUtils.parse('-23.456', null, options)).toBe(-23.456)
    expect(numberUtils.parse('-24', null, options)).toBe(-23.456)
    expect(numberUtils.parse('-12.345', null, options)).toBe(-12.345)
    expect(numberUtils.parse('-11', null, options)).toBe(-12.345)
  })

  test('parse invalid number returns null', () => {
    expect(numberUtils.parse('')).toBe(null)
    expect(numberUtils.parse(null)).toBe(null)
    expect(numberUtils.parse('sDjh45*$#jrGS')).toBe(null)
  })

  test('format invalid number returns empty string', () => {
    expect(numberUtils.format(NaN)).toBe('')
    expect(numberUtils.format(Infinity)).toBe('')
    expect(numberUtils.format(-Infinity)).toBe('')
    expect(numberUtils.format('invalid')).toBe('')
    expect(numberUtils.format(undefined)).toBe('')
    expect(numberUtils.format(null)).toBe('')
  })

  test('format valid number', () => {
    expect(numberUtils.format(123)).toBe('123')
    expect(numberUtils.format(123.456789)).toBe('123.456789')
    expect(numberUtils.format(123, '0.00')).toBe('123.00')
    expect(numberUtils.format(123.456789, '0.00')).toBe('123.46')
    expect(numberUtils.format(123, '0.[00]')).toBe('123')
    expect(numberUtils.format(123.456789, '0.[00]')).toBe('123.46')
    expect(numberUtils.format(4.75455045028e-17, '0.00')).toBe('0.00')
  })
})
