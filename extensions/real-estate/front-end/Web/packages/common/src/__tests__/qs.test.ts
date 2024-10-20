import qs from '../qs'

describe('qs (Query String)', () => {
  const initWindowLocation = (url) => {
    Object.defineProperty(window, 'location', {
      value: new URL(url),
      writable: true,
    })
  }

  const URL_WITH_QUERY = 'https://dummy.com?name=me'
  const URL_WITH_NO_QUERY = 'https://dummy.com'

  describe('parse', () => {
    test('should return query object when query parameters exist in url', () => {
      initWindowLocation(URL_WITH_QUERY)

      const result = qs.parse()

      expect(result).toMatchObject({ name: 'me' })
    })

    test('should return empty object when query parameters does not exist in url', () => {
      initWindowLocation(URL_WITH_NO_QUERY)

      const result = qs.parse()

      expect(result).toMatchObject({})
    })
  })

  describe('get', () => {
    test('should return value when key parameter exists', () => {
      initWindowLocation(URL_WITH_QUERY)

      const result = qs.get('name')

      expect(result).toBe('me')
    })

    test('should return undefined when key parameter does not exist', () => {
      initWindowLocation(URL_WITH_NO_QUERY)

      const result = qs.get('name')

      expect(result).toBeUndefined()
    })
  })

  describe('createUrl', () => {
    test('should return using given url and params', () => {
      const params = { name: 'me' }

      const result = qs.createUrl(URL_WITH_NO_QUERY, params)

      expect(result).toBe(URL_WITH_QUERY)
    })

    test('should return a given url when there are no params', () => {
      const result = qs.createUrl(URL_WITH_NO_QUERY)

      expect(result).toBe(URL_WITH_NO_QUERY)
    })
  })
})
