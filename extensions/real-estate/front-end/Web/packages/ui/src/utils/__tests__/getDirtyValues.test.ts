import getDirtyValues from '../getDirtyValues'

describe('getDirtyValues', () => {
  test('no dirty values', () => {
    const result = getDirtyValues({ name: 'name', other: 'other' }, {})
    expect(result).toEqual({})
  })

  test('one dirty value', () => {
    const result = getDirtyValues(
      { name: 'name', other: 'other' },
      { name: true }
    )
    expect(result).toEqual({ name: 'name' })
  })

  test('many dirty values', () => {
    const result = getDirtyValues(
      { name: 'name', other: 'other' },
      { name: true, other: true }
    )
    expect(result).toEqual({ name: 'name', other: 'other' })
  })

  test('nested dirty values', () => {
    const result = getDirtyValues(
      { nested: { name: 'name', other: { name: 'test' } }, other: 'other' },
      { nested: { name: true } }
    )
    expect(result).toEqual({ nested: { name: 'name' } })
  })
})
