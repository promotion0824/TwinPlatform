import caseInsensitiveEquals from '../caseInsensitiveEquals'

describe('Case insesitivity comparing', () => {
  test('should have value for string 1', () => {
    const result = caseInsensitiveEquals(null, 'string1')
    expect(result).toBeFalsy()
  })

  test('should return true even if they are not same in case', () => {
    const result = caseInsensitiveEquals('STRING1', 'strINg1')
    expect(result).toBeTruthy()
  })

  test('should return false if strings do not match', () => {
    const result = caseInsensitiveEquals('STRING12', 'strINg1')
    expect(result).toBeFalsy()
  })

  test('should return false if any of the strings is undefined', () => {
    const result = caseInsensitiveEquals('STRING12', undefined)
    expect(result).toBeFalsy()
  })

  test('should return false if any of the strings is undefined', () => {
    const result = caseInsensitiveEquals(undefined, 'STRING12')
    expect(result).toBeFalsy()
  })

  test('should return false if any of the strings is null', () => {
    const result = caseInsensitiveEquals(null, 'STRING12')
    expect(result).toBeFalsy()
  })

  test('should return false if any of the strings is null', () => {
    const result = caseInsensitiveEquals('STRING12', null)
    expect(result).toBeFalsy()
  })
})
