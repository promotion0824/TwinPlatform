import { InsightPriority } from '@willow/common/insights/insights/types'
import {
  filterMatchedGuids,
  findKeysByValue,
  findModel,
  findModelIndex,
  getDbIdGuidDict,
  getGuids,
  getLayerName,
  getPriority,
  isPriorityValid,
  updateTooltipStyles,
} from './index'

describe('Viewer3D helpers', () => {
  describe('getGuids', () => {
    test('should return guids from model by db ids', async () => {
      const getPropertiesMock = jest.fn()
      const propertyList = [
        [
          {
            displayName: 'GUID',
            displayValue: 'GUID0',
          },
        ],
        [
          {
            displayName: 'GUID',
            displayValue: 'GUID1',
          },
        ],
      ]
      getPropertiesMock.mockImplementation((id, fn) => {
        fn({ properties: propertyList[id] })
      })
      const model = {
        getProperties: getPropertiesMock,
      }

      const guids = await getGuids(
        model as unknown as Autodesk.Viewing.Model,
        [0, 1]
      )

      guids.forEach((guid, i) => {
        const [property] = propertyList[i]
        expect(guid).toBe(property.displayValue)
      })
    })
  })

  describe('getDbIdGuidDict', () => {
    test('should return key: dbid, value guid dict in order', () => {
      const dbIds = [0, 1]
      const guids = ['guid0', 'guid1']

      const dict = getDbIdGuidDict(dbIds, guids)

      dbIds.forEach((_, i) => {
        expect(dict[i]).toBe(guids[i])
      })
    })

    test('should return empty object when there are no dbids', () => {
      const dbIds = []
      const guids = ['guid0', 'guid1']

      const dict = getDbIdGuidDict(dbIds, guids)

      expect(dict).toEqual({})
    })
  })

  describe('findKeysByValue', () => {
    test('should return keys by value', () => {
      const value = 'b0'
      const dict = { a: 'a0', b: value, c: value }

      const result = findKeysByValue(dict as any, value)

      const expectedValues = ['b', 'c']
      expect(result).toEqual(expectedValues)
    })

    test('should return empty array when there is no matched value', () => {
      const value = 'b0'
      const dict = { a: 'a0', b: 'c0', c: 'c0' }

      const result = findKeysByValue(dict as any, value)

      expect(result).toEqual([])
    })
  })

  describe('findModel', () => {
    test('should return a model by model id', () => {
      const modelId = 1
      const models = [{ id: 1 }, { id: 2 }]

      const result = findModel(modelId, models)

      expect(result).toEqual(models[0])
    })

    test('should return undefined when there is no matched id', () => {
      const modelId = 3
      const models = [{ id: 1 }, { id: 2 }]

      const result = findModel(modelId, models)

      expect(result).toBeUndefined()
    })
  })

  describe('isPriorityValid', () => {
    test('should return true when priority is between 1 and 3', () => {
      const priorities = [1, 2, 3]

      priorities.forEach((priority: InsightPriority) => {
        expect(isPriorityValid(priority)).toBeTruthy()
      })
    })

    test.each([
      { priority: -1, isValid: false },
      { priority: 0, isValid: true },
      { priority: 1, isValid: true },
      { priority: 2, isValid: true },
      { priority: 3, isValid: true },
      { priority: 4, isValid: false },
    ])(
      'should return false when priority is not between 0 and 3',
      ({ priority, isValid }) => {
        expect(isPriorityValid(priority as any)).toBe(isValid)
      }
    )
  })

  describe('updateTooltipStyles', () => {
    const getTransformStyle = ({ x, y }) => `translate(${x}px, ${y}px)`
    let $el: any
    beforeEach(() => {
      $el = {
        style: {
          transform: '',
          display: '',
        },
      }
    })
    test('update transform position when mousePosition exists', () => {
      const mousePosition = { x: 1, y: 1 }
      const options: any = {
        mousePosition,
      }

      updateTooltipStyles($el, options)

      expect($el.style.left).toBe(`${mousePosition.x}px`)
      expect($el.style.top).toBe(`${mousePosition.y}px`)
    })

    test('does not update transform position when mousePosition does not exist', () => {
      const options: any = {}

      updateTooltipStyles($el, options)

      expect($el.style.transform).toBe('')
    })

    test('update display style to flex when display option is true', () => {
      const options: any = { display: true }

      updateTooltipStyles($el, options)

      expect($el.style.display).toBe('flex')
    })
    test('update display style to none when display option is false', () => {
      const options: any = { display: false }

      updateTooltipStyles($el, options)

      expect($el.style.display).toBe('none')
    })
  })

  describe('getPriority', () => {
    test('should return priority when layers and matched guid exist', () => {
      const guid = 'abc'
      const layers: any = [
        {
          [guid]: {
            priority: 2,
          },
        },
      ]
      const modelIndex = 0

      const result = getPriority(layers, modelIndex, guid)

      expect(result).toBe(2)
    })

    test('should return undefined when layers and matched guid exist but no priority exist', () => {
      const guid = 'abc'
      const layers: any = [
        {
          [guid]: {},
        },
      ]
      const modelIndex = 0

      const result = getPriority(layers, modelIndex, guid)

      expect(result).toBeUndefined()
    })

    test('should return -1 when layers or matched guid do not exist', () => {
      const guid = 'abc'
      const layers: any = [
        {
          def: {},
        },
      ]
      const modelIndex = 0

      const result = getPriority(layers, modelIndex, guid)

      expect(result).toBe(-1)
    })
  })

  describe('getLayerName', () => {
    test('should return layer name when layers and matched guid exist', () => {
      const guid = 'abc'
      const layerName = 'layer1'
      const layers: any = [
        {
          [guid]: {
            name: layerName,
          },
        },
      ]
      const modelIndex = 0

      const result = getLayerName(layers, modelIndex, guid)

      expect(result).toBe(layerName)
    })

    test('should return undefined when layers or matched guid does not exist', () => {
      const guid = 'abc'
      const layerName = 'layer1'
      const layers: any = [
        {
          def: {
            name: layerName,
          },
        },
      ]
      const modelIndex = 0

      const result = getLayerName(layers, modelIndex, guid)

      expect(result).toBeUndefined()
    })
    test('should return undefined when layers and matched guid exist, but no name exists', () => {
      const guid = 'abc'
      const layers: any = [
        {
          [guid]: {},
        },
      ]
      const modelIndex = 0

      const result = getLayerName(layers, modelIndex, guid)

      expect(result).toBeUndefined()
    })
  })

  describe('filterMatchedGuids', () => {
    test('should return guids by dbIds', () => {
      const dbIds = [1, 2]
      const guids = ['guid1', 'guid2']
      const dbIdGuidDict = {
        1: guids[0],
        2: guids[1],
      }

      const result = filterMatchedGuids(dbIds, dbIdGuidDict)

      expect(result).toEqual(guids)
    })
  })

  describe('findModelIndex', () => {
    test('should return -1 when dbId is -1', () => {
      const dbId = -1

      const result = findModelIndex(dbId, 0, {})

      expect(result).toBe(-1)
    })

    test('should return 0 when modelId is -1', () => {
      const modelId = -1

      const result = findModelIndex(0, modelId, {})

      expect(result).toBe(0)
    })

    const dbId = 1
    const modelId = 2
    test('should return -1 when dbId & modelId are positive number, and there is no an associated value in dict', () => {
      const urnIndexModelIdDict = {
        1: 99,
      }

      const result = findModelIndex(dbId, modelId, urnIndexModelIdDict)

      expect(result).toBe(-1)
    })

    test('should return modelIndex when dbId & modelId are positive number, and there is an associated value in dict', () => {
      const urnIndex = 0
      const urnIndexModelIdDict = {
        [urnIndex]: modelId,
      }

      const result = findModelIndex(dbId, modelId, urnIndexModelIdDict)

      expect(result).toBe(urnIndex)
    })
  })
})
