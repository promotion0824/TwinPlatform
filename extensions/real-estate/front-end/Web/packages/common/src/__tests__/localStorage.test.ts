import localStorage from '../localStorage'

describe('localStorage', () => {
  beforeEach(() => {
    window.localStorage.clear()
  })

  const keyForNonObject = 'key1'
  const nonObjectVal = 'val1'
  const keyForObject = 'key2'
  const objectVal = { name: 'val1' }
  describe('set', () => {
    test('should return true when setting value is successful', () => {
      const nonObjectValSetResult = localStorage.set(
        keyForNonObject,
        nonObjectVal
      )
      const objectValSetResult = localStorage.set(keyForObject, objectVal)

      expect(nonObjectValSetResult).toBeTruthy()
      expect(objectValSetResult).toBeTruthy()
    })

    test('should return false when setting value is not successful', () => {
      const storageSpy = jest.spyOn(Storage.prototype, 'setItem')
      storageSpy.mockImplementation(() => {
        throw new Error('error')
      })

      const result = localStorage.set(keyForObject, nonObjectVal)

      expect(result).toBeFalsy()
      storageSpy.mockRestore()
    })
  })

  describe('get', () => {
    test('should return parsed value when key exists', () => {
      localStorage.set(keyForNonObject, nonObjectVal)
      localStorage.set(keyForObject, objectVal)

      const nonObjectResult = localStorage.get(keyForNonObject)
      const objectResult = localStorage.get(keyForObject)

      expect(nonObjectResult).toBe(nonObjectVal)
      expect(objectResult).toMatchObject(objectVal)
    })

    test('should return null when key does notexists', () => {
      const result = localStorage.get(keyForNonObject)

      expect(result).toBeNull()
    })
  })
})
