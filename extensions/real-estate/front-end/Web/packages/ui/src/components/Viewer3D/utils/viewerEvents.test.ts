import {
  LoadModelCallbacks,
  LoadModelFn,
  Urn,
  UrnIndexModelIdDict,
} from '../types'
import {
  viewerReset,
  viewerSelect,
  executeUserFunction,
  getDbIdPropertiesDict,
  updateLayers,
  updateViewerCursor,
  getViewerResetFn,
  getShowModelFn,
  getHideModelFn,
  getSelectFn,
  loadModel,
} from './viewerEvents'
import { getDefaultColor } from './index'

describe('Viewer Events', () => {
  describe('ViewerControls', () => {
    test('viewerReset should trigger clearSelection and restreState', () => {
      const viewer = {
        clearSelection: jest.fn(),
        restoreState: jest.fn(),
      } as any
      const viewerState = {}

      viewerReset(viewer, viewerState)

      expect(viewer.clearSelection).toBeCalled()
      expect(viewer.restoreState).toBeCalled()
    })

    test('viewerSelect should trigger fitToView and select', () => {
      const viewer = { fitToView: jest.fn(), select: jest.fn() } as any
      const dbIds = []
      const model = {}

      viewerSelect(viewer, dbIds, model as Autodesk.Viewing.Model)

      expect(viewer.fitToView).toBeCalled()
      expect(viewer.select).toBeCalled()
    })
  })

  describe('executeUserFunction', () => {
    test('should return the result of userFunction', async () => {
      const expectedResult = [
        {
          attributeName: 'viewable_in',
          displayCategory: '__viewable_in__',
          displayName: 'viewable_in',
          displayValue: 'cache',
          hidden: 1,
          precision: 0,
          type: 20,
          units: null,
        },
      ]
      const pDb = {
        executeUserFunction: async (userFn) => userFn(),
      }
      const userFunction = () => expectedResult

      const result = await executeUserFunction(pDb, userFunction)

      expect(result).toBe(expectedResult)
    })

    test('should throw an error when executeUserFunction fails', async () => {
      const errorMessage = 'error'
      const pDb = {
        executeUserFunction: async () => {
          throw new Error(errorMessage)
        },
      }

      await expect(executeUserFunction(pDb, jest.fn())).rejects.toThrowError(
        errorMessage
      )
    })
  })

  describe('getDbIdPropertiesDict', () => {
    test('should return key: dbid, value: properties by model', async () => {
      const expectedResult = [
        {
          attributeName: 'viewable_in',
          displayCategory: '__viewable_in__',
          displayName: 'viewable_in',
          displayValue: 'cache',
          hidden: 1,
          precision: 0,
          type: 20,
          units: null,
        },
      ]
      const pDb = {
        executeUserFunction: async () => expectedResult,
      }

      const result = await getDbIdPropertiesDict(pDb as any)

      expect(result).toBe(expectedResult)
    })
  })

  describe('updateLayers', () => {
    test('should trigger updateMaterial by dbId, and update once in a view', () => {
      const invalidateMockFn = jest.fn()
      const viewer: any = {
        impl: { invalidate: invalidateMockFn },
      }
      const enumNodeFragmentsMockFn = jest.fn()
      const model: any = {
        getInstanceTree: () => ({
          enumNodeFragments: enumNodeFragmentsMockFn,
        }),
        getFragmentList: jest.fn(),
      }
      const dbIdGuidDict = {
        0: 'abc',
        1: 'def',
      }
      const layers = {}
      updateLayers(viewer, model, layers, dbIdGuidDict, getDefaultColor)

      const numberOfDbIds = Object.keys(dbIdGuidDict).length
      expect(enumNodeFragmentsMockFn).toBeCalledTimes(numberOfDbIds)
      expect(invalidateMockFn).toBeCalledTimes(1)
    })
  })

  describe('updateViewerCursor', () => {
    let viewer
    beforeEach(() => {
      viewer = {
        canvas: {
          style: {
            cursor: '',
          },
        },
      }
    })
    test('should change cursor state to auto when mousePosition does not exist', () => {
      const mousePosition = undefined

      updateViewerCursor(viewer, mousePosition)

      expect(viewer.canvas.style.cursor).toBe('auto')
    })

    test('should change cursor state to pointer when mousePosition does not exist', () => {
      const mousePosition = {
        x: 1,
        y: 1,
      }

      updateViewerCursor(viewer, mousePosition)

      expect(viewer.canvas.style.cursor).toBe('pointer')
    })
  })

  describe('getViewerResetFn', () => {
    test('should return a viewerReset function using a viewerState', () => {
      const viewer = {
        viewerState: {
          getState: jest.fn(),
        },
      }
      const viewerResetMockFn = jest.fn()

      const viewerResetFn = getViewerResetFn(viewer, viewerResetMockFn)
      viewerResetFn()

      expect(viewerResetFn).toBeInstanceOf(Function)
      expect(viewerResetMockFn).toBeCalled()
    })
  })

  describe('getShowModelFn', () => {
    const urns = ['model0']
    const urnIndex = 0
    const modelId = 1
    const loadCallbacks = {
      onDocumentLoadSuccess: jest.fn(),
      onDocumentLoadFailure: jest.fn(),
    }
    const onModelLoadChange = jest.fn()

    test('should return a function for showing a model', () => {
      const showModelMockFn = jest.fn()
      const viewer = {
        showModel: showModelMockFn,
      } as any
      const loadModelMockFn = jest.fn()
      const urnIndexModelIdDict = {
        [urnIndex]: modelId,
      }

      const showModelFn = getShowModelFn(
        viewer as Autodesk.Viewing.Viewer3D,
        urnIndexModelIdDict as UrnIndexModelIdDict,
        loadModelMockFn as LoadModelFn,
        urns as Urn[],
        loadCallbacks as LoadModelCallbacks,
        onModelLoadChange
      )

      expect(showModelFn).toBeInstanceOf(Function)
    })

    test('should execute loadModel when urnModelId does not exist in a dict', () => {
      const showModelMockFn = jest.fn()
      const viewer = {
        showModel: showModelMockFn,
      } as any
      const loadModelMockFn = jest.fn()
      const urnIndexModelIdDict = {
        [urnIndex]: modelId,
      }

      const showModelFn = getShowModelFn(
        viewer,
        urnIndexModelIdDict,
        loadModelMockFn,
        urns,
        loadCallbacks,
        onModelLoadChange
      )
      showModelFn(1)

      expect(loadModelMockFn).toBeCalled()
      expect(showModelMockFn).not.toBeCalled()
      expect(onModelLoadChange).toBeCalledWith(1, 'loading')
    })

    test('should execute showModel when urnModelId does exists in a dict', () => {
      const showModelMockFn = jest.fn(() => true)
      const viewer = {
        showModel: showModelMockFn,
      } as any
      const loadModelMockFn = jest.fn()
      const urnIndexModelIdDict = {
        [urnIndex]: modelId,
      }

      const showModelFn = getShowModelFn(
        viewer,
        urnIndexModelIdDict,
        loadModelMockFn,
        urns,
        loadCallbacks,
        onModelLoadChange
      )
      showModelFn(urnIndex)

      expect(loadModelMockFn).not.toBeCalled()
      expect(showModelMockFn).toBeCalled()
    })

    test('should throw an error when Autodesk SDK fails to display a model', () => {
      const showModelMockFn = jest.fn(() => false)
      const viewer = {
        showModel: showModelMockFn,
      } as any
      const loadModelMockFn = jest.fn()
      const urnIndexModelIdDict = {
        [urnIndex]: modelId,
      }

      const showModelFn = getShowModelFn(
        viewer,
        urnIndexModelIdDict,
        loadModelMockFn,
        urns,
        loadCallbacks,
        onModelLoadChange
      )

      expect(() => showModelFn(urnIndex)).toThrowError(
        `Failed to show a model by model id : ${modelId}`
      )
    })
  })

  describe('getHideModelFn', () => {
    const urnIndex = 0
    const modelId = 1
    const urnIndexModelIdDict = {
      [urnIndex]: modelId,
    }

    test('should return a function for hiding a model', () => {
      const hideModelMockFn = jest.fn()
      const viewer = {
        hideModel: hideModelMockFn,
      } as any
      const hideModelFn = getHideModelFn(viewer, urnIndexModelIdDict)

      expect(hideModelFn).toBeInstanceOf(Function)
    })

    test('should execute Autodesk hideModel when urnModelId exists in a dict', () => {
      const hideModelMockFn = jest.fn(() => true)
      const viewer = {
        hideModel: hideModelMockFn,
      } as any

      const hideModelFn = getHideModelFn(viewer, urnIndexModelIdDict)
      hideModelFn(urnIndex)

      expect(hideModelMockFn).toBeCalled()
    })

    test('should throw an error when Autodesk hideModel fails to hide a model', () => {
      const hideModelMockFn = jest.fn(() => false)
      const viewer = {
        hideModel: hideModelMockFn,
      } as any

      const hideModelFn = getHideModelFn(viewer, urnIndexModelIdDict)

      expect(() => hideModelFn(urnIndex)).toThrowError(
        `Failed to hide a model by model id : ${modelId}`
      )
    })
  })

  describe('getSelectFn', () => {
    test('should return a function for selecting object or property ', () => {
      const viewer = {
        getAllModels: jest.fn(),
      } as any
      const viewerSelectMockFn = jest.fn()
      const dictObj = {
        urnIndexModelIdDict: {},
        dbIdGuidDicts: {},
        dbIdPropertyGuidDicts: {},
      }

      const viewerSelectFn = getSelectFn(viewer, dictObj, viewerSelectMockFn)

      expect(viewerSelectFn).toBeInstanceOf(Function)
    })

    const urnIndex = 0
    const modelId = 1
    const guid = 'wefj'

    test('should execute viewerSelect', () => {
      const viewerSelectMockFn = jest.fn()
      const type = 'object'
      const dictObj = {
        urnIndexModelIdDict: {
          [urnIndex]: modelId,
        },
        dbIdGuidDicts: {
          [modelId]: {
            123: guid,
          },
        },
        dbIdPropertyGuidDicts: {},
      }
      const viewer = {
        getAllModels: () => [{ id: modelId }],
      } as any

      const viewerSelectFn = getSelectFn(viewer, dictObj, viewerSelectMockFn)
      viewerSelectFn({
        type,
        urnIndex,
        guid,
      })

      expect(viewerSelectMockFn).toBeCalled()
    })

    test('should use dbIdGuiDicts when type is object', () => {
      const viewerSelectMockFn = jest.fn()
      const type = 'object'
      const dictObj = {
        urnIndexModelIdDict: {
          [urnIndex]: modelId,
        },
        dbIdGuidDicts: {
          [modelId]: {
            123: guid,
          },
        },
        dbIdPropertyGuidDicts: {},
      }
      const viewer = {
        getAllModels: () => [{ id: modelId }],
      } as any

      const viewerSelectFn = getSelectFn(viewer, dictObj, viewerSelectMockFn)

      viewerSelectFn({
        type,
        urnIndex,
        guid,
      })
      expect(viewerSelectMockFn).toBeCalled()

      expect(() =>
        viewerSelectFn({
          type: 'asset',
          urnIndex,
          guid,
        })
      ).toThrow(`Data for model id ${modelId} in asset type dict`)
    })

    test('should use dbIdPropertyGuidDicts when type is asset', () => {
      const viewerSelectMockFn = jest.fn()
      const type = 'asset'
      const dictObj = {
        urnIndexModelIdDict: {
          [urnIndex]: modelId,
        },
        dbIdGuidDicts: {},
        dbIdPropertyGuidDicts: {
          [modelId]: {
            123: guid,
          },
        },
      }
      const viewer = {
        getAllModels: () => [{ id: modelId }],
      } as any

      const viewerSelectFn = getSelectFn(viewer, dictObj, viewerSelectMockFn)

      viewerSelectFn({
        type,
        urnIndex,
        guid,
      })
      expect(viewerSelectMockFn).toBeCalled()

      expect(() =>
        viewerSelectFn({
          type: 'object',
          urnIndex,
          guid,
        })
      ).toThrow(`Data for model id ${modelId} in object type dict`)
    })

    test('should throw an error when model id does not exist in a urnIndexModelIdDict', () => {
      const viewerSelectMockFn = jest.fn()
      const type = 'object'
      const dictObj = {
        urnIndexModelIdDict: {},
        dbIdGuidDicts: {
          [modelId]: {
            123: guid,
          },
        },
        dbIdPropertyGuidDicts: {},
      }
      const viewer = {
        getAllModels: () => [{ id: modelId }],
      } as any

      const viewerSelectFn = getSelectFn(viewer, dictObj, viewerSelectMockFn)

      expect(() =>
        viewerSelectFn({
          type,
          urnIndex,
          guid,
        })
      ).toThrowError(`Data for model id undefined in ${type} type dict`)
    })

    test('should throw an error when model id does not exist in a dict with the type', () => {
      const viewerSelectMockFn = jest.fn()
      const type = 'object'
      const dictObj = {
        urnIndexModelIdDict: {
          [urnIndex]: modelId,
        },
        dbIdGuidDicts: {},
        dbIdPropertyGuidDicts: {},
      }
      const viewer = {
        getAllModels: () => [{ id: modelId }],
      } as any

      const viewerSelectFn = getSelectFn(viewer, dictObj, viewerSelectMockFn)

      expect(() =>
        viewerSelectFn({
          type,
          urnIndex,
          guid,
        })
      ).toThrowError(`Data for model id ${modelId} in ${type} type dict`)
    })
  })

  describe('loadModel', () => {
    test('should return a function for loading a model using urn', () => {
      const loadModelFn = loadModel(jest.fn())

      expect(loadModelFn).toBeInstanceOf(Function)
    })

    test('should return a function for loading a model using urn', () => {
      const urnIndex = 0
      const urns = ['urn0']
      const loadCallbacks = {
        onDocumentLoadSuccess: jest.fn(),
        onDocumentLoadFailure: jest.fn(),
      }
      const documentLoadMockFn = jest.fn()

      const loadModelFn = loadModel(documentLoadMockFn)
      loadModelFn(urnIndex, urns, loadCallbacks)

      expect(documentLoadMockFn).toBeCalled()
    })
  })
})
