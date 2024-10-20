import {
  createJsonPatch,
  prepareFormValues,
  withoutEmpties,
  populateDeletedFields,
} from '../TwinEditorContext'
import { modelFromSchemas } from '../testUtils'

describe('createJsonPatch', () => {
  test('should generate a replace when existing value is changed', () => {
    const patch = createJsonPatch({
      existingTwin: { x: 'existing' },
      newTwin: { x: 'new' },
      expandedModel: modelFromSchemas({
        x: 'string',
      }),
      ignoreFields: [],
    })
    expect(patch).toEqual([
      {
        op: 'replace',
        path: '/x',
        value: 'new',
      },
    ])
  })

  test('should generate a replace even when existing value was empty string', () => {
    const patch = createJsonPatch({
      existingTwin: { x: '' },
      newTwin: { x: 'new' },
      expandedModel: modelFromSchemas({
        x: 'string',
      }),
      ignoreFields: [],
    })
    expect(patch).toEqual([
      {
        op: 'replace',
        path: '/x',
        value: 'new',
      },
    ])
  })

  test('should transform added numbers', () => {
    const patch = createJsonPatch({
      existingTwin: {},
      newTwin: { x: '999' },
      expandedModel: modelFromSchemas({
        x: 'integer',
      }),
      ignoreFields: [],
    })
    expect(patch).toStrictEqual([
      {
        op: 'add',
        path: '/x',
        value: 999,
      },
    ])
  })

  test('should transform replaced numbers', () => {
    const patch = createJsonPatch({
      existingTwin: { x: 33 },
      newTwin: { x: '999' },
      expandedModel: modelFromSchemas({
        x: 'integer',
      }),
      ignoreFields: [],
    })
    expect(patch).toStrictEqual([
      {
        op: 'replace',
        path: '/x',
        value: 999,
      },
    ])
  })

  test('should generate a remove when existing value is emptied', () => {
    const patch = createJsonPatch({
      existingTwin: { x: 'existing' },
      newTwin: { x: '' },
      expandedModel: modelFromSchemas({
        x: 'string',
      }),
      ignoreFields: [],
    })
    expect(patch).toEqual([
      {
        op: 'remove',
        path: '/x',
      },
    ])
  })

  test('should generate nothing when an absent value becomes null', () => {
    const patch = createJsonPatch({
      existingTwin: {},
      newTwin: { x: null },
      expandedModel: modelFromSchemas({
        x: 'string',
      }),
      ignoreFields: [],
    })
    expect(patch).toEqual([])
  })

  test('should generate nothing when empty string is unchanged', () => {
    const patch = createJsonPatch({
      existingTwin: { x: '' },
      newTwin: { x: '' },
      expandedModel: modelFromSchemas({
        x: 'string',
      }),
      ignoreFields: [],
    })
    expect(patch).toEqual([])
  })

  test('should generate a remove when empty string becomes null', () => {
    const patch = createJsonPatch({
      existingTwin: { x: '' },
      newTwin: { x: null },
      expandedModel: modelFromSchemas({
        x: 'string',
      }),
      ignoreFields: [],
    })
    expect(patch).toEqual([
      {
        op: 'remove',
        path: '/x',
      },
    ])
  })

  test('should generate nothing when nested values are unchanged', () => {
    const patch = createJsonPatch({
      existingTwin: {
        x: {
          first: 'x',
          last: 'y',
        },
      },
      newTwin: {
        x: {
          first: 'x',
          last: 'y',
        },
      },
      expandedModel: modelFromSchemas({
        x: {
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
      }),
      ignoreFields: [],
    })
    expect(patch).toEqual([])
  })

  test('should not create empty objects', () => {
    const patch = createJsonPatch({
      existingTwin: {},
      newTwin: {
        x: {
          first: null,
          last: null,
        },
      },
      expandedModel: modelFromSchemas({
        x: {
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
      }),
      ignoreFields: [],
    })
    expect(patch).toEqual([])
  })

  test('should create a whole object if it did not exist', () => {
    const patch = createJsonPatch({
      existingTwin: {},
      newTwin: {
        x: {
          first: 'first',
          last: null,
        },
      },
      expandedModel: modelFromSchemas({
        x: {
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
      }),
      ignoreFields: [],
    })
    expect(patch).toStrictEqual([
      {
        op: 'add',
        path: '/x',
        value: {
          first: 'first',
        },
      },
    ])
  })

  test('should create nested removes', () => {
    const patch = createJsonPatch({
      existingTwin: {
        x: {
          first: 'x',
          last: 'y',
        },
      },
      newTwin: {
        x: {
          first: null,
          last: null,
        },
      },
      expandedModel: modelFromSchemas({
        x: {
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
      }),
      ignoreFields: [],
    })
    expect(patch).toEqual([
      {
        op: 'remove',
        path: '/x/first',
      },
      {
        op: 'remove',
        path: '/x/last',
      },
    ])
  })

  test('should transform nested numbers', () => {
    const patch = createJsonPatch({
      existingTwin: {
        x: {},
      },
      newTwin: {
        x: {
          first: '999',
        },
      },
      expandedModel: modelFromSchemas({
        x: {
          '@type': 'Map',
          mapKey: {
            name: 'tagName',
            schema: 'string',
          },
          mapValue: {
            name: 'tagValue',
            schema: 'integer',
          },
        },
      }),
      ignoreFields: [],
    })
    expect(patch).toEqual([
      {
        op: 'add',
        path: '/x/first',
        value: 999,
      },
    ])
  })

  test('should ignore ignoredFields', () => {
    const patch = createJsonPatch({
      existingTwin: {
        x: {},
      },
      newTwin: {
        x: {
          first: '999',
        },
      },
      expandedModel: modelFromSchemas({
        x: {
          '@type': 'Map',
          mapKey: {
            name: 'tagName',
            schema: 'string',
          },
          mapValue: {
            name: 'tagValue',
            schema: 'integer',
          },
        },
      }),
      ignoreFields: ['/x/first'],
    })
    expect(patch).toEqual([])
  })

  test('should ignore fields inside ignoredFields', () => {
    const patch = createJsonPatch({
      existingTwin: {
        x: {},
      },
      newTwin: {
        x: {
          first: '999',
        },
      },
      expandedModel: modelFromSchemas({
        x: {
          '@type': 'Map',
          mapKey: {
            name: 'tagName',
            schema: 'string',
          },
          mapValue: {
            name: 'tagValue',
            schema: 'integer',
          },
        },
      }),
      ignoreFields: ['/x'],
    })
    expect(patch).toEqual([])
  })
})

describe('prepareFormValues', () => {
  test('should numericise', () => {
    const numericised = prepareFormValues(
      {
        x: '999',
        y: 'nine nine nine',
      },
      modelFromSchemas({
        x: 'integer',
        y: 'string',
      })
    )
    expect(numericised).toStrictEqual({ x: 999, y: 'nine nine nine' })
  })

  test('should turn empty strings to null', () => {
    const numericised = prepareFormValues(
      {
        x: '',
      },
      modelFromSchemas({
        x: 'integer',
      })
    )
    expect(numericised).toStrictEqual({ x: null })
  })

  test('should transform duration values', () => {
    const prepared = prepareFormValues(
      {
        x: {
          years: '0',
          months: '0',
          days: '12',
          hours: '0',
          minutes: '0',
          seconds: '0',
        },
      },
      modelFromSchemas({
        x: 'duration',
      })
    )
    expect(prepared).toStrictEqual({ x: 'P12D' })
  })
})

describe('withoutEmpties', () => {
  test('base cases', () => {
    expect(withoutEmpties({})).toStrictEqual({})
    expect(withoutEmpties(null)).toStrictEqual(null)
    expect(withoutEmpties(9)).toStrictEqual(9)
    expect(withoutEmpties('hello')).toStrictEqual('hello')
    expect(withoutEmpties([1, 2, 3])).toStrictEqual([1, 2, 3])
  })

  test('object', () => {
    expect(withoutEmpties({ x: '', y: null, z: 'keep' })).toStrictEqual({
      z: 'keep',
    })
  })

  test('nested', () => {
    expect(
      withoutEmpties({
        x: {
          y: null,
          z: {
            q: '',
            v: {},
          },
        },
      })
    ).toStrictEqual({})
  })
})

describe('populateDeletedFields', () => {
  test('should insert null values for missing fields', () => {
    expect(populateDeletedFields({}, { me: null })).toStrictEqual({ me: null })
    expect(populateDeletedFields({ x: {} }, { x: { y: 234 } })).toStrictEqual({
      x: { y: null },
    })
  })
})
