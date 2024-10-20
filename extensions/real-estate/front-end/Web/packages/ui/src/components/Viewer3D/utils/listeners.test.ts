import {
  aggregateSelectionChangedListener,
  mouseMoveListener,
  objectUnderMouseChangedListener,
} from './listeners'

describe('Viewer3D listeners', () => {
  describe('aggregateSelectionChangedListener', () => {
    test('should not trigger onClick when selections length is 0', () => {
      const selections = []
      const onClickMockFn = jest.fn()

      const listener = aggregateSelectionChangedListener({}, onClickMockFn)
      listener({ selections })

      expect(onClickMockFn).not.toBeCalled()
    })

    const dbIdArray = [1, 2]
    const model = { id: 0 }
    const guids = ['guid0', 'guid1']
    const selections = [{ dbIdArray, model }]
    test('should trigger onClick and pass guids when there is associated data in dicts', () => {
      const dbIdGuidDicts = {
        0: {
          1: guids[0],
          2: guids[1],
        },
      }
      const onClickMockFn = jest.fn(({ guids: guidList }) => {
        expect(guidList).toEqual(guids)
      })

      const listener = aggregateSelectionChangedListener(
        dbIdGuidDicts,
        onClickMockFn
      )
      listener({ selections })
    })

    test('should trigger onClick and pass empty guids when there is no associated data in dicts', () => {
      const dbIdGuidDicts = {
        0: {
          3: guids[0],
          4: guids[1],
        },
      }
      const onClickMockFn = jest.fn(({ guids: guidList }) => {
        expect(guidList).toEqual([])
      })

      const listener = aggregateSelectionChangedListener(
        dbIdGuidDicts,
        onClickMockFn
      )
      listener({ selections })
    })
  })

  describe('objectUnderMouseChangeListener', () => {
    test('should not trigger callback and onMouseHoverChange when model is hidden', () => {
      const dicts = {
        dbIdGuidDicts: {},
        urnIndexModelIdDict: {},
      }
      const onMouseHoverChangeMockFn = jest.fn()
      const callbackMockFn = jest.fn()
      const target = {
        impl: {},
      }

      const listener = objectUnderMouseChangedListener(
        dicts,
        onMouseHoverChangeMockFn,
        callbackMockFn
      )
      listener({ dbId: 0, modelId: 0, target })

      expect(onMouseHoverChangeMockFn).not.toBeCalled()
      expect(callbackMockFn).not.toBeCalled()
    })

    test('should trigger callback and onMouseHoverChange when model is hidden', () => {
      const dicts = {
        dbIdGuidDicts: {},
        urnIndexModelIdDict: {},
      }
      const onMouseHoverChangeMockFn = jest.fn()
      const callbackMockFn = jest.fn()
      const target = {
        impl: {
          model: {
            id: 1,
          },
        },
      }

      const listener = objectUnderMouseChangedListener(
        dicts,
        onMouseHoverChangeMockFn,
        callbackMockFn
      )
      listener({ dbId: 0, modelId: 0, target })

      expect(onMouseHoverChangeMockFn).toBeCalled()
      expect(callbackMockFn).toBeCalled()
    })

    test('callback parameter is true when mouseover is on the model', () => {
      const dicts = {
        dbIdGuidDicts: {},
        urnIndexModelIdDict: {},
      }
      const dbId = 1
      const target = {
        impl: {
          model: {
            id: 1,
          },
        },
      }
      const onMouseHoverChangeMockFn = jest.fn()
      const callbackMockFn = jest.fn((isMouseOverOn) => {
        expect(isMouseOverOn).toBeTruthy()
      })

      const listener = objectUnderMouseChangedListener(
        dicts,
        onMouseHoverChangeMockFn,
        callbackMockFn
      )
      listener({ dbId, modelId: 0, target })

      expect(onMouseHoverChangeMockFn).toBeCalled()
    })

    test('callback parameter is false when mouseover is not on the model', () => {
      const dicts = {
        dbIdGuidDicts: {},
        urnIndexModelIdDict: {},
      }
      const dbId = -1
      const target = {
        impl: {
          model: {
            id: 1,
          },
        },
      }
      const onMouseHoverChangeMockFn = jest.fn()
      const callbackMockFn = jest.fn((isMouseOverOn) => {
        expect(isMouseOverOn).toBeFalsy()
      })

      const listener = objectUnderMouseChangedListener(
        dicts,
        onMouseHoverChangeMockFn,
        callbackMockFn
      )
      listener({ dbId, modelId: 0, target })

      expect(onMouseHoverChangeMockFn).toBeCalled()
    })

    test('onMouseHoverChange parameters have associated guid & modelIndex', () => {
      const dbId = 1
      const modelId = 1
      const target = {
        impl: {
          model: {
            id: modelId,
          },
        },
      }
      const targetGuid = 'guid0'
      const targetModelIndex = 0
      const dicts = {
        dbIdGuidDicts: { [modelId]: { [dbId]: targetGuid } },
        urnIndexModelIdDict: {
          [targetModelIndex]: modelId,
        },
      }
      const callbackMockFn = jest.fn()
      const onMouseHoverChangeMockFn = jest.fn((guid, modelIndex) => {
        expect(guid).toBe(targetGuid)
        expect(modelIndex).toBe(targetModelIndex)
      })

      const listener = objectUnderMouseChangedListener(
        dicts,
        onMouseHoverChangeMockFn,
        callbackMockFn
      )
      listener({ dbId, modelId, target })
    })

    test('onMouseHoverChange parameters have undefined guid when there is no associated data in dbIdGuidDicts', () => {
      const dbId = 1
      const modelId = 1
      const target = {
        impl: {
          model: {
            id: modelId,
          },
        },
      }
      const targetModelIndex = 0
      const dicts = {
        dbIdGuidDicts: { [modelId]: {} },
        urnIndexModelIdDict: {
          [targetModelIndex]: modelId,
        },
      }
      const callbackMockFn = jest.fn()
      const onMouseHoverChangeMockFn = jest.fn((guid) => {
        expect(guid).toBeUndefined()
      })

      const listener = objectUnderMouseChangedListener(
        dicts,
        onMouseHoverChangeMockFn,
        callbackMockFn
      )
      listener({ dbId, modelId, target })
    })

    test('onMouseHoverChange parameters have -1 modelIndex when there is no associated data in urnIndexModelIdDict', () => {
      const dbId = 1
      const modelId = 1
      const target = {
        impl: {
          model: {
            id: modelId,
          },
        },
      }
      const dicts = {
        dbIdGuidDicts: { [modelId]: {} },
        urnIndexModelIdDict: {},
      }
      const callbackMockFn = jest.fn()
      const onMouseHoverChangeMockFn = jest.fn((_, modelIndex) => {
        expect(modelIndex).toBe(-1)
      })

      const listener = objectUnderMouseChangedListener(
        dicts,
        onMouseHoverChangeMockFn,
        callbackMockFn
      )
      listener({ dbId, modelId, target })
    })
  })

  describe('mouseMoveListener', () => {
    test('should pass null to onMousePositionChange when isMouseOverOn is false', () => {
      const isMouseOverOn = false
      const onMouseHoverChangeMockFn = jest.fn((state) => {
        expect(state).toBeNull()
      })

      const listener = mouseMoveListener(
        isMouseOverOn,
        onMouseHoverChangeMockFn,
        {}
      )
      listener({ offsetX: 0, offsetY: 0 })
    })

    test('should pass position change to onMousePositionChange when isMouseOverOn is true', () => {
      const offset = {
        offsetX: 0,
        offsetY: 0,
      }
      const padding = {
        x: 10,
        y: 10,
      }
      const isMouseOverOn = true
      const onMouseHoverChangeMockFn = jest.fn((state) => {
        expect(state).toEqual({
          x: offset.offsetX + padding.x,
          y: offset.offsetY + padding.y,
        })
      })

      const listener = mouseMoveListener(
        isMouseOverOn,
        onMouseHoverChangeMockFn,
        padding
      )
      listener(offset)
    })
  })
})
