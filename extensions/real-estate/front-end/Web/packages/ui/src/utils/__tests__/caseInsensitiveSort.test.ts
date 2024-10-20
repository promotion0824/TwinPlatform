import caseInsensitiveSort from '../caseInsensitiveSort'

describe('caseInsensitiveSort', () => {
  test('will produce a compareFn of function type', () => {
    const compareFn = caseInsensitiveSort(
      (obj: { displayValue: 'randomString' }) => obj.displayValue
    )

    expect(compareFn).not.toBe(null)
    expect(compareFn).toBeInstanceOf(Function)
  })

  test('will throw error when not providing a function as prop', () => {
    expect(caseInsensitiveSort('this input wont work')).toThrow()
    expect(caseInsensitiveSort(null)).toThrow()
    expect(caseInsensitiveSort(undefined)).toThrow()
    expect(caseInsensitiveSort(new Date())).toThrow()
  })

  test('will produce a compareFn that can be used as a prop in Array.prototype.sort', () => {
    const objArray = [
      { displayValue: null },
      { displayValue: 'twin2' },
      { displayValue: 'bUiLdInG' },
      { displayValue: 'building' },
      { displayValue: 'BUILDING' },
      { displayValue: undefined },
      { displayValue: 'twin1' },
      { displayValue: undefined },
      { displayValue: null },
    ]

    const compareFn = caseInsensitiveSort(
      (obj: { displayValue: string }) => obj.displayValue
    )

    const sortedObjArray = objArray.sort(compareFn)

    // case insensitive sort, object with property strings are always sorted
    // before object with property of null/undefined
    expect(sortedObjArray[0]).toMatchObject({ displayValue: 'bUiLdInG' })
    expect(sortedObjArray[1]).toMatchObject({ displayValue: 'building' })
    expect(sortedObjArray[2]).toMatchObject({ displayValue: 'BUILDING' })

    expect(sortedObjArray[3]).toMatchObject({ displayValue: 'twin1' })
    expect(sortedObjArray[4]).toMatchObject({ displayValue: 'twin2' })

    // object with property of null/undefined among themselves will
    // have same relative order as in the original array
    expect(sortedObjArray[5]).toMatchObject({ displayValue: null })
    expect(sortedObjArray[6]).toMatchObject({ displayValue: undefined })
    expect(sortedObjArray[7]).toMatchObject({ displayValue: undefined })
    expect(sortedObjArray[8]).toMatchObject({ displayValue: null })
  })
})
