import merge from '../merge'

describe('merge', () => {
  test('should choose theirs if it differs from base', () => {
    // prettier-ignore
    const result = merge(
      { "a": "base" },
      { "a": "mine" },
      { "a": "theirs" }
    );

    expect(result.result).toEqual({ a: 'theirs' })
    expect(result.conflictedFields).toEqual({ a: true })
  })

  test('should choose mine if theirs does not differ from base', () => {
    // prettier-ignore
    const result = merge(
      { "a": "base" },
      { "a": "mine" },
      { "a": "base" }
    );

    expect(result.result).toEqual({ a: 'mine' })
    expect(result.conflictedFields).toEqual({ a: false })
  })

  test('should choose theirs if there is no base', () => {
    // prettier-ignore
    const result = merge(
      { },
      { "a": "mine" },
      { "a": "theirs" }
    );

    expect(result.result).toEqual({ a: 'theirs' })
    expect(result.conflictedFields).toEqual({ a: true })
  })

  test('should choose theirs if there is no base or mine', () => {
    // prettier-ignore
    const result = merge(
      { },
      { },
      { "a": "theirs" }
    );

    expect(result.result).toEqual({ a: 'theirs' })
    expect(result.conflictedFields).toEqual({ a: true })
  })

  test('should choose theirs if it differs from base and we updated the field', () => {
    // prettier-ignore
    const result = merge(
      { "a": "base" },
      { },
      { "a": "theirs" }
    );

    expect(result.result).toEqual({ a: 'theirs' })
    expect(result.conflictedFields).toEqual({ a: true })
  })

  test('should choose theirs if it differs from base and we deleted the field', () => {
    // prettier-ignore
    const result = merge(
      { "a": "base" },
      { "a": "base" },
      { }
    );

    expect(result.result).toEqual({ a: null })
    expect(result.conflictedFields).toEqual({ a: true })
  })

  test('should work recursively', () => {
    // prettier-ignore
    const result = merge(
      {
        "a": {
          "sub": "base"
        }
      },
      {
        "a": {
          "sub": "mine"
        }
      },
      {
        "a": {
          "sub": "theirs"
        }
      },
    );

    expect(result.result).toEqual({ a: { sub: 'theirs' } })
    expect(result.conflictedFields).toEqual({ a: { sub: true } })
  })
})
