import _ from 'lodash'
import {
  addEmptyFields,
  removeEmptyFields,
  isEmpty,
  getDefaultValue,
  splitTwin,
  getTwinRogueAttributes,
} from '@willow/common/twins/view/twinModel'
import {
  makeStringProperty,
  makeStringField,
  modelFromInstance,
} from '../testUtils'

const myModel = {
  contents: [
    makeStringProperty('stringProperty'),
    {
      '@type': 'Property',
      name: 'alternateClassification',
      displayName: {
        en: 'Alternate Classification',
      },
      writable: true,
      schema: {
        '@type': 'Object',
        fields: [
          {
            name: 'masterFormat',
            displayName: {
              en: 'MasterFormat',
            },
            schema: {
              '@type': 'Object',
              fields: [makeStringField('version'), makeStringField('code')],
            },
          },
          {
            name: 'omniClass',
            displayName: {
              en: 'OmniClass',
            },
            schema: {
              '@type': 'Object',
              fields: [makeStringField('version'), makeStringField('code')],
            },
          },
        ],
      },
    },
    {
      '@type': 'Property',
      name: 'mapProperty',
      displayName: 'My Map prop',
      schema: {
        '@type': 'Map',
        mapKey: {
          name: 'tagName',
          schema: 'string',
        },
        mapValue: {
          name: 'tagValue',
          schema: 'string',
        },
      },
    },
    {
      '@type': 'Property',
      displayName: {
        en: 'Mapped IDs',
      },
      name: 'mappedIds',
      schema: {
        '@type': 'Array',
        elementSchema: 'dtmi:com:willowinc:SpaceMappedIdObject;1',
      },
      writable: true,
    },
  ],
}

describe('getDefaultValue', () => {
  test("should return null string for 'string'", () => {
    expect(getDefaultValue('string')).toBe(null)
  })

  test("should return null for 'double'", () => {
    expect(getDefaultValue('double')).toBe(null)
  })

  test('should traverse an object', () => {
    expect(
      getDefaultValue(
        myModel.contents.find((c) => c.name === 'alternateClassification')
          .schema
      )
    ).toEqual({
      masterFormat: {
        code: null,
        version: null,
      },
      omniClass: {
        code: null,
        version: null,
      },
    })
  })
})

describe('addEmptyFields, removeEmptyFields', () => {
  test('should add empty fields', () => {
    const twin = {}
    const newTwin = addEmptyFields(twin, myModel)
    expect(newTwin).toEqual({
      stringProperty: null,
      alternateClassification: {
        masterFormat: {
          version: null,
          code: null,
        },
        omniClass: {
          version: null,
          code: null,
        },
      },
    })
    const resetTwin = removeEmptyFields(newTwin, twin)
    expect(resetTwin).toEqual(twin)
  })

  test('should add missing nested fields, level 1', () => {
    const twin = {
      alternateClassification: {},
    }
    const newTwin = addEmptyFields(twin, myModel)
    expect(newTwin).toEqual({
      stringProperty: null,
      alternateClassification: {
        masterFormat: {
          version: null,
          code: null,
        },
        omniClass: {
          version: null,
          code: null,
        },
      },
    })

    const resetTwin = removeEmptyFields(newTwin, twin)
    expect(resetTwin).toEqual(twin)
  })

  test('should add missing nested fields, level 2', () => {
    const twin = {
      alternateClassification: {
        masterFormat: {
          version: '123',
        },
      },
    }
    const newTwin = addEmptyFields(twin, myModel)
    expect(newTwin).toEqual({
      stringProperty: null,
      alternateClassification: {
        masterFormat: {
          version: '123',
          code: null,
        },
        omniClass: {
          version: null,
          code: null,
        },
      },
    })

    const resetTwin = removeEmptyFields(newTwin, twin)
    expect(resetTwin).toEqual(twin)
  })

  test('should not touch existing fields', () => {
    const twin = {
      stringProperty: 'i exist!',
      alternateClassification: {
        masterFormat: {
          version: 'I exist too!',
          code: 'me too!',
        },
        omniClass: {
          version: 'I exist too!',
          code: 'me too!',
        },
      },
    }
    const newTwin = addEmptyFields(twin, myModel)
    expect(newTwin).toEqual(twin)
    const resetTwin = removeEmptyFields(newTwin, twin)
    expect(resetTwin).toEqual(twin)
  })
})

describe('isEmpty', () => {
  test('should return true for basic types except null, undefined, empty string', () => {
    expect(isEmpty(undefined)).toBeTrue()
    expect(isEmpty(null)).toBeTrue()
    expect(isEmpty('')).toBeTrue()
    expect(isEmpty(true)).toBeFalse()
    expect(isEmpty(false)).toBeFalse()
    expect(isEmpty('nonempty')).toBeFalse()
    expect(isEmpty(0)).toBeFalse()
    expect(isEmpty(1)).toBeFalse()
  })

  test('should return true for objects with no nonempty values', () => {
    expect(isEmpty({})).toBeTrue()
    expect(isEmpty({ val: null })).toBeTrue()
    expect(isEmpty({ val: '' })).toBeTrue()
    expect(isEmpty({ val: {} })).toBeTrue()
    expect(isEmpty({ val: { nested: '' } })).toBeTrue()
  })

  test('should return false for objects with nonempty values', () => {
    expect(isEmpty({ val: true })).toBeFalse()
    expect(isEmpty({ val: 'string' })).toBeFalse()
    expect(isEmpty({ val: { nested: 'string' } })).toBeFalse()
    expect(isEmpty({ val: { nested: 'string' }, otherVal: null })).toBeFalse()
  })
})

describe('removeEmptyFields', () => {
  test('base case', () => {
    expect(removeEmptyFields({})).toEqual({})
  })

  test('should keep updates', () => {
    expect(
      removeEmptyFields(
        {
          myProp: 'my update',
        },
        {
          myProp: 'old value',
        }
      )
    ).toEqual({
      myProp: 'my update',
    })
  })

  test('should remove null value that was not in initial', () => {
    expect(
      removeEmptyFields(
        {
          myProp: null,
        },
        {}
      )
    ).toEqual({})
  })

  test('should remove empty value that was not in initial', () => {
    expect(
      removeEmptyFields(
        {
          myProp: {
            thisIsEmpty: null,
          },
        },
        {}
      )
    ).toEqual({})
  })

  test('should not remove real nested value even if not in initial', () => {
    expect(
      removeEmptyFields(
        {
          myProp: {
            thisIsNotEmpty: true,
          },
        },
        {}
      )
    ).toEqual({
      myProp: {
        thisIsNotEmpty: true,
      },
    })
  })
})

describe('splitTwin', () => {
  test('empty object', () => {
    const twin = { id: '123' }
    const { topLevelProperties, groups } = splitTwin(
      twin,
      modelFromInstance(twin)
    )
    expect(topLevelProperties).toEqual({ id: '123' })
    expect(groups).toEqual({})
  })

  test('object with no grouped properties', () => {
    const twin = { id: '123', property: 'ignore', another: 'ignore' }
    const { topLevelProperties, groups } = splitTwin(
      twin,
      modelFromInstance(twin)
    )
    expect(topLevelProperties).toEqual(twin)
    expect(groups).toEqual({})
  })

  test('object with grouped properties', () => {
    const twin = {
      property: 'ignore',
      another: 'ignore',
      grouped: { example: 'hi', another: 'there' },
      group: { example: 'hello' },
    }
    const { topLevelProperties, groups } = splitTwin(
      twin,
      modelFromInstance(twin)
    )
    expect(topLevelProperties).toEqual({
      property: 'ignore',
      another: 'ignore',
    })
    expect(groups).toEqual({
      grouped: { example: 'hi', another: 'there' },
      group: { example: 'hello' },
    })
  })

  test('object with nested group properties', () => {
    const twin = {
      property: 'ignore',
      nested: {
        grouped: { example: 'hi' },
      },
    }
    const { topLevelProperties, groups } = splitTwin(
      twin,
      modelFromInstance(twin)
    )
    expect(topLevelProperties).toEqual({
      property: 'ignore',
    })
    expect(groups).toEqual({
      nested: {
        grouped: { example: 'hi' },
      },
    })
  })

  test('nulls are not groups', () => {
    const twin = {
      property: null,
    }
    const { topLevelProperties, groups } = splitTwin(
      twin,
      modelFromInstance(twin)
    )
    expect(topLevelProperties).toEqual({
      property: null,
    })
    expect(groups).toEqual({})
  })

  test('both exclude metadata', () => {
    const twin = {
      metadata: 'we also care about this',
    }
    const { topLevelProperties, groups } = splitTwin(
      twin,
      modelFromInstance(twin)
    )
    expect(topLevelProperties).toEqual({})
    expect(groups).toEqual({})
  })
})

describe('getTwinRogueAttributes', () => {
  test('should report no errors with no fields', () => {
    expect(getTwinRogueAttributes({}, myModel)).toEqual([])
  })

  test('should report no errors with special fields', () => {
    expect(
      getTwinRogueAttributes(
        {
          id: 'ok',
          metadata: 'ok',
          etag: 'ok',
          alternateClassification: {
            $metadata: 'ok',
          },
        },
        myModel
      )
    ).toEqual([])
  })

  test('should report no error for an existing field', () => {
    expect(
      getTwinRogueAttributes({ stringProperty: 'this is fine' }, myModel)
    ).toEqual([])
  })

  test('should report an error for a missing field', () => {
    expect(getTwinRogueAttributes({ whatIsThis: 'not fine' }, myModel)).toEqual(
      [['whatIsThis']]
    )
  })

  test('should report two errors for two missing fields', () => {
    expect(
      getTwinRogueAttributes(
        { whatIsThis: 'not fine', other: 'also bad' },
        myModel
      )
    ).toEqual([['whatIsThis'], ['other']])
  })

  test('should report no errors for correct nested fields', () => {
    expect(
      getTwinRogueAttributes(
        {
          alternateClassification: {
            masterFormat: {
              version: 'ok!',
            },
          },
        },
        myModel
      )
    ).toEqual([])
  })

  test('should report an error for incorrect nested field', () => {
    expect(
      getTwinRogueAttributes(
        {
          alternateClassification: {
            masterFormat: {
              version: 'ok!',
              nested: 'oops',
            },
          },
        },
        myModel
      )
    ).toEqual([['alternateClassification', 'masterFormat', 'nested']])
  })

  test('should not report an error for fields inside a rogue field', () => {
    expect(
      getTwinRogueAttributes(
        {
          rogueGroup: {
            rogueProp: 'X',
          },
        },
        myModel
      )
    ).toEqual([['rogueGroup']])
  })

  test('should not report errors for keys inside a Map property', () => {
    expect(
      getTwinRogueAttributes(
        {
          mapProperty: {
            thisIsOk: true,
            thisIsAlsoOk: true,
          },
        },
        myModel
      )
    ).toEqual([])
  })

  test('should not report errors for Array properties', () => {
    expect(
      getTwinRogueAttributes(
        {
          mappedIds: [
            {
              exactType: 'PostalAddressIdentity',
              scope: 'ORG',
              scopeId: '123',
              value: 'Some address',
            },
          ],
        },
        myModel
      )
    ).toEqual([])
  })
})
