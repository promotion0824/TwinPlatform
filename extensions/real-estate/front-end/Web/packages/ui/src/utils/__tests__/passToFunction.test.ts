import passToFunction from '../passToFunction'

describe('passToFunction', () => {
  test('should return the result of function call using arguments when first parameter is function', () => {
    const fn = (...args) => [...args]
    const param1 = 1
    const param2 = 2

    const result = passToFunction(fn, param1, param2)

    expect(result).toStrictEqual([param1, param2])
  })

  test('should return first parameter value when first parameter is not function', () => {
    const param0 = 'name'
    const param1 = 1
    const param2 = 2

    const result = passToFunction(param0, param1, param2)

    expect(result).toBe(param0)
  })

  test('should return null when first parameter does not exist', () => {
    const result = passToFunction()

    expect(result).toBeNull()
  })
})
