import {
  constructDefaultIndices,
  constructRenderDropdownObject,
  getEnabledLayersCount,
  toggleModel,
} from '../utils'
import { RenderDropdownObject, RenderLayer } from '../types'
import { Modules3d } from '../../types'

describe('constructDefaultIndices', () => {
  test('should provide an array as result', async () => {
    const result = constructDefaultIndices({
      renderDropdownObject: {},
      urns,
    })

    expect(Array.isArray(result)).toBe(true)
  })

  test('should provide indices array as expected when there is only 1 model enabled', async () => {
    const result = constructDefaultIndices({
      renderDropdownObject: makeModels({
        isFirstModelEnabled: false,
        isSecondModelEnabled: true,
        isFirstModelUrlValid: true,
        isSecondModelUrValid: true,
      }),
      urns,
    })

    expect(result).toEqual([1])
  })

  test('should provide indices array as expected when there is more than one models enabled', async () => {
    const result = constructDefaultIndices({
      renderDropdownObject: makeModels({
        isFirstModelEnabled: true,
        isSecondModelEnabled: true,
        isFirstModelUrlValid: true,
        isSecondModelUrValid: true,
      }),
      urns,
    })

    expect(result).toEqual([0, 1])
  })

  test('should provide empty array when url does not match anything in urns even there is models enabled', async () => {
    const result = constructDefaultIndices({
      renderDropdownObject: makeModels({
        isFirstModelEnabled: true,
        isSecondModelEnabled: true,
        isFirstModelUrlValid: false,
        isSecondModelUrValid: false,
      }),
      urns,
    })

    expect(result.length).toBe(0)
  })

  test('should provide empty indices array as expected no model is enabled', async () => {
    const result = constructDefaultIndices({
      renderDropdownObject: makeModels({
        isFirstModelEnabled: false,
        isSecondModelEnabled: false,
        isFirstModelUrlValid: true,
        isSecondModelUrValid: true,
      }),
      urns,
    })

    expect(result.length).toBe(0)
  })
})

beforeEach(() => jest.clearAllMocks())

describe('toggleModel', () => {
  const viewerControls = {
    showModel: jest.fn(),
    hideModel: jest.fn(),
    reset: jest.fn(),
    select: jest.fn(),
  }
  test('should not fire viewerControl methods when modelUrn is not provided', async () => {
    toggleModel({
      urns: ['url-1', 'url-2'],
      modelUrn: undefined,
      isEnable: true,
      viewerControls,
    })

    expect(viewerControls.showModel).not.toBeCalled()
    expect(viewerControls.hideModel).not.toBeCalled()
  })

  test('should not fire viewerControl methods when isEnabled is not provided', async () => {
    toggleModel({
      urns: ['url-1', 'url-2'],
      modelUrn: 'url-1',
      isEnable: undefined,
      viewerControls,
    })

    expect(viewerControls.showModel).not.toBeCalled()
    expect(viewerControls.hideModel).not.toBeCalled()
  })

  test('should not fire viewerControl methods when modelUrn is not found in urns', async () => {
    toggleModel({
      urns: ['url-1', 'url-2'],
      modelUrn: 'url-3',
      isEnable: true,
      viewerControls,
    })

    expect(viewerControls.showModel).not.toBeCalled()
    expect(viewerControls.hideModel).not.toBeCalled()
  })

  test('should not fire viewerControl methods when viewControls is not provided', async () => {
    toggleModel({
      urns: ['url-1', 'url-2'],
      modelUrn: 'url-2',
      isEnable: true,
      viewerControls: undefined,
    })

    expect(viewerControls.showModel).not.toBeCalled()
    expect(viewerControls.hideModel).not.toBeCalled()
  })

  test('should fire viewerControl.showModel with index value of modelUrn in urns when isEnabled is true', async () => {
    toggleModel({
      urns: ['url-1', 'url-2'],
      modelUrn: 'url-1',
      isEnable: true,
      viewerControls,
    })

    expect(viewerControls.showModel).toBeCalledWith(0)
    expect(viewerControls.hideModel).not.toBeCalled()
  })

  test('should fire viewerControl.hideModel with index value of modelUrn in urns when isEnabled is false', async () => {
    toggleModel({
      urns: ['url-1', 'url-2'],
      modelUrn: 'url-2',
      isEnable: false,
      viewerControls,
    })

    expect(viewerControls.showModel).not.toBeCalled()
    expect(viewerControls.hideModel).toBeCalledWith(1)
  })
})

const makeModels = ({
  isFirstModelEnabled,
  isSecondModelEnabled,
  isFirstModelUrlValid,
  isSecondModelUrValid,
}: {
  isFirstModelEnabled: boolean
  isSecondModelEnabled: boolean
  isFirstModelUrlValid: boolean
  isSecondModelUrValid: boolean
}) => ({
  'Mechanical Air': {
    'General Exhaust': {
      id: 'a23ee4a3-79e5-4b34-951c-a855325465bd',
      name: '1MW-GX-B1.NWD',
      visualId: '00000000-0000-0000-0000-000000000000',
      url: isFirstModelUrlValid
        ? 'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNGU1ZmMyMjktZmZkOS00NjJhLTg4MmItMTZiNGE2M2IyYThhLXVhdC8xTVctR1gtQjFfMjAyMjAyMDMyMjMxNTAuTldE'
        : 'random',
      sortOrder: 8,
      canBeDeleted: true,
      isDefault: false,
      typeName: 'General Exhaust',
      groupType: 'Mechanical Air',
      moduleTypeId: '7367d0cf-1d01-4f80-9f51-2ee87980a0dc',
      moduleGroup: {
        id: '026d29f1-6da2-40bb-bc48-802fc05caed2',
        name: 'Mechanical Air',
        sortOrder: 5,
        siteId: '4e5fc229-ffd9-462a-882b-16b4a63b2a8a',
      },
      isEnabled: isFirstModelEnabled,
    },
  },
  Plumbing: {
    'Domestic Water': {
      id: 'd56b9da4-0593-44b0-b0a1-df3fffa4ebb5',
      name: '1MW-DW.NWD',
      visualId: '00000000-0000-0000-0000-000000000000',
      url: isSecondModelUrValid
        ? 'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNGU1ZmMyMjktZmZkOS00NjJhLTg4MmItMTZiNGE2M2IyYThhLXVhdC8xTVctRFdfMjAyMjAyMDMyMjMwMDYuTldE'
        : 'random',
      sortOrder: 2,
      canBeDeleted: true,
      isDefault: false,
      typeName: 'Domestic Water',
      groupType: 'Plumbing',
      moduleTypeId: 'd301a2ca-4534-496b-ba11-c3ed42921f04',
      moduleGroup: {
        id: '37f67cd2-11c8-442d-9972-73617b888c39',
        name: 'Plumbing',
        sortOrder: 0,
        siteId: '4e5fc229-ffd9-462a-882b-16b4a63b2a8a',
      },
      isEnabled: isSecondModelEnabled,
    },
  },
})

const urns = [
  'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNGU1ZmMyMjktZmZkOS00NjJhLTg4MmItMTZiNGE2M2IyYThhLXVhdC8xTVctR1gtQjFfMjAyMjAyMDMyMjMxNTAuTldE',
  'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNGU1ZmMyMjktZmZkOS00NjJhLTg4MmItMTZiNGE2M2IyYThhLXVhdC8xTVctRFdfMjAyMjAyMDMyMjMwMDYuTldE',
]

const makeLayer = (
  url,
  isDefault,
  groupType,
  typeName,
  isUngroupedLayer,
  moduleTypeId
) => ({
  url,
  isDefault,
  groupType,
  typeName,
  isUngroupedLayer,
  moduleTypeId,
})

const groupType = 'Base'

const url =
  'dXJuOmFkc2sub2JqZWN0czpvcy5vYmplY3Q6d2lsbG93LXNpdGUtNDA0YmQzM2MtYTY5Ny00MDI3LWI2YTYtNjc3ZTMwYTUzZDA3LXVhdC9BcmNoaXRlY3R1cmVfMjAyMTEyMDkwMTE2Mzkubndk'

const A = 'Model A'
const modelA = makeLayer(url, true, groupType, A, true, '1')

const B = 'Model B'
const modelB = makeLayer(url, false, groupType, B, false, '2')

const C = 'Model C'
const modelC = makeLayer(url, true, groupType, C, false, '3')

const D = 'Model D'
const modelD = makeLayer(url, false, groupType, D, false, '4')

const E = 'Model E'
const modelE = makeLayer(url, true, groupType, E, false, '5')

// mimick response from /api/sites/${siteId}/floors/${floorId}/layerGroups
const modules3D = [modelA, modelB, modelC, modelD, modelE] as Modules3d

// expected dropdown object
const dropdownObject = {
  [A]: {
    url,
    isEnabled: true,
    isUngroupedLayer: true,
    groupType,
    moduleTypeId: '1',
  },
  [groupType]: {
    [B]: {
      url,
      isEnabled: false,
      isUngroupedLayer: false,
      groupType,
      moduleTypeId: '2',
    },
    [C]: {
      url,
      isEnabled: true,
      isUngroupedLayer: false,
      groupType,
      moduleTypeId: '3',
    },
    [D]: {
      url,
      isEnabled: false,
      isUngroupedLayer: false,
      groupType,
      moduleTypeId: '4',
    },
    [E]: {
      url,
      isEnabled: true,
      isUngroupedLayer: false,
      groupType,
      moduleTypeId: '5',
    },
  },
} as unknown as RenderDropdownObject

const testNestedLayers = (testObject: RenderDropdownObject) => {
  // test nested layers
  const expectedModelA = dropdownObject[A]
  const testModelA = testObject[A]
  expect(testModelA).toMatchObject(expectedModelA)

  const expectedModelB = dropdownObject[groupType][B]
  const testModelB = testObject[groupType][B]
  expect(testModelB).toMatchObject(expectedModelB)

  const expectedModelC = dropdownObject[groupType][C]
  const testModelC = testObject[groupType][C]
  expect(testModelC).toMatchObject(expectedModelC)

  const expectedModelD = dropdownObject[groupType][D]
  const testModelD = testObject[groupType][D]
  expect(testModelD).toMatchObject(expectedModelD)

  const expectedModelE = dropdownObject[groupType][E]
  const testModelE = testObject[groupType][E]
  expect(testModelE).toMatchObject(expectedModelE)
}

describe('constructRenderDropdownObject', () => {
  test('Construct dropdown object: no sorting', () => {
    const testObject = constructRenderDropdownObject(modules3D)
    expect(testObject).toMatchObject(dropdownObject)

    testNestedLayers(testObject)
  })

  test('Construct dropdown object: with sorting', () => {
    const testObject = constructRenderDropdownObject(modules3D)
    expect(testObject).toMatchObject(dropdownObject)

    testNestedLayers(testObject)
  })
})

describe('getEnabledLayersCount', () => {
  test('Calculate the number of enabled layers in dropdown object', () => {
    const testNumberOfEnabledLayers = getEnabledLayersCount(dropdownObject)

    const expectedNumberOfEnabledLayers = Object.values(modules3D).filter(
      (model) => model.isDefault
    ).length

    expect(testNumberOfEnabledLayers).toEqual(expectedNumberOfEnabledLayers)
  })
})
