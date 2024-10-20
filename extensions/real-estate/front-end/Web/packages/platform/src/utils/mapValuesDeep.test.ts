import mapValuesDeep from './mapValuesDeep'

describe('mapValuesDeep', () => {
  test('transforms any non object given to it', () => {
    expect(mapValuesDeep(123, (v) => v * 2)).toBe(246)
    expect(mapValuesDeep('string', (v) => v.slice(2))).toBe('ring')
    expect(mapValuesDeep(['array'], (v) => v.concat(['2']))).toEqual([
      'array',
      '2',
    ])
  })

  test('transforms a shallow object', () => {
    expect(
      mapValuesDeep(
        {
          a: 1,
          b: 2,
          c: 3,
        },
        (v) => v * 2
      )
    ).toEqual({ a: 2, b: 4, c: 6 })
  })

  test('transforms a nested object deeply', () => {
    expect(
      mapValuesDeep(
        {
          a: { aa: 1.1, ab: 1.2, ac: 1.3 },
          b: 2,
          c: {
            ca: 3.1,
            cb: 3.2,
            cc: {
              cca: 3.31,
              ccb: 3.32,
            },
          },
        },
        (v) => v * 2
      )
    ).toEqual({
      a: { aa: 2.2, ab: 2.4, ac: 2.6 },
      b: 4,
      c: {
        ca: 6.2,
        cb: 6.4,
        cc: {
          cca: 6.62,
          ccb: 6.64,
        },
      },
    })
  })
})
