/* eslint-disable import/prefer-default-export */
/* eslint-disable no-continue */
import { renderHook, waitFor } from '@testing-library/react'
import { rest } from 'msw'
import { setupServer } from 'msw/node'
import BaseWrapper from '@willow/ui/utils/testUtils/Wrapper'
import _ from 'lodash'
import { act } from 'react-test-renderer'
import useTwinHistory from '../useTwinHistory'

const versions = {
  versions: [
    {
      user: { id: 'id1', firstName: 'John', lastName: 'Doe' },
      timestamp: '2022-10-12T22:39:26.264Z',
      twin: {
        etag: 'W/"933ecdaf-6f32-4583-87fc-4fe6b3fb6273"',
        serialNumber: 'new value',
        connectorID: "bet you can't edit this",
        registrationID: '',
        ownedByRef: { targetId: '', name: 'new value', targetModelId: '' },
        metadata: { modelId: 'dtmi:com:willowinc:BACnetController;1' },
        id: 'twinId1',
        siteID: 'siteId1',
        uniqueID: '123123-u',
        name: 'Twin 1',
        geometryViewerID: 'new value',
        enabled: true,
        expectedLife: 'P1D',
        maintenanceInterval: 'P1Y',
      },
    },
    {
      user: { id: 'id1', firstName: 'John', lastName: 'Doe' },
      timestamp: '2021-10-16T07:14:22.000Z',
      twin: {
        etag: 'W/"933ecdaf-6f32-4583-87fc-4fe6b3fb6273"',
        serialNumber: 'new value',
        connectorID: "bet you can't edit this",
        registrationID: '',
        ownedByRef: { targetId: '', name: '', targetModelId: '' },
        metadata: { modelId: 'dtmi:com:willowinc:BACnetController;1' },
        id: 'twinId1',
        siteID: 'siteId1',
        uniqueID: '123123-u',
        name: 'Twin 1',
        geometryViewerID: 'new value',
        enabled: true,
        expectedLife: 'P1D',
        maintenanceInterval: 'P1Y',
      },
    },
    {
      user: { id: 'id1', firstName: 'John', lastName: 'Doe' },
      timestamp: '2021-10-15T22:00:00.000Z',
      twin: {
        etag: 'W/"933ecdaf-6f32-4583-87fc-4fe6b3fb6273"',
        geometrySpatialReference: 'some value',
        modelNumber: 'CRAC-31-1',
        serialNumber: 'new value',
        connectorID: "bet you can't edit this",
        registrationID: '',
        ownedByRef: { targetId: '', name: '', targetModelId: '' },
        metadata: { modelId: 'dtmi:com:willowinc:BACnetController;1' },
        id: 'twinId1',
        siteID: 'siteId1',
        uniqueID: '123123-u',
        name: 'Twin 1',
        geometryViewerID: 'new value',
        enabled: true,
        expectedLife: 'P1D',
        maintenanceInterval: 'P1Y',
      },
    },
    {
      user: { id: 'id1', firstName: 'John', lastName: 'Doe' },
      timestamp: '2021-10-13T17:00:00.000Z',
      twin: {
        etag: 'W/"933ecdaf-6f32-4583-87fc-4fe6b3fb6273"',
        geometrySpatialReference: 'some value',
        modelNumber: 'CRAC-31-1',
        serialNumber: 'new value',
        connectorID: "bet you can't edit this",
        registrationID: '',
        ownedByRef: { targetId: '', name: '', targetModelId: '' },
        metadata: { modelId: 'dtmi:com:willowinc:BACnetController;1' },
        id: 'twinId1',
        siteID: 'siteId1',
        uniqueID: '123123-u',
        name: 'Twin 1',
        geometryViewerID: 'new value',
        enabled: true,
      },
    },
    {
      user: { id: 'id1', firstName: 'John', lastName: 'Doe' },
      timestamp: '2021-10-12T07:00:00.000Z',
      twin: {
        etag: 'W/"933ecdaf-6f32-4583-87fc-4fe6b3fb6273"',
        geometrySpatialReference: 'some value',
        modelNumber: 'CRAC-31-1',
        serialNumber: 'Y18D6S0',
        connectorID: "bet you can't edit this",
        registrationID: '',
        ownedByRef: { targetId: '', name: '', targetModelId: '' },
        metadata: { modelId: 'dtmi:com:willowinc:BACnetController;1' },
        id: 'twinId1',
        siteID: 'siteId1',
        uniqueID: '123123-u',
        name: 'Twin 1',
      },
    },
  ],
}

const version1Edits = {
  geometryViewerID: 'new value',
  serialNumber: 'new value',
  enabled: true,
}

const version2Edits = { expectedLife: 'P1D', maintenanceInterval: 'P1Y' }

const version3Edits = {
  geometrySpatialReference: null,
  modelNumber: null,
}

const version4Edits = {
  ownedByRef: { name: 'new value' },
}

const twinId = 'twinId1'
const siteId = 'siteId1'

const site1 = { id: siteId, userRole: 'admin' }
const server = setupServer(
  rest.get(`/api/sites/:siteId/twins/:twinId/history`, (_req, res, ctx) =>
    res(ctx.json(versions))
  )
)

beforeAll(() => server.listen())
afterEach(() => {
  server.resetHandlers()
})
afterAll(() => server.close())

function Wrapper({ children }: { children?: JSX.Element }) {
  return (
    <BaseWrapper user={{ isCustomerAdmin: true }} sites={[site1]}>
      {children}
    </BaseWrapper>
  )
}

describe('useTwinHistory', () => {
  test('Should return correct values', async () => {
    const { result } = renderHook(
      (props: { siteId?: string; twinId?: string }) =>
        useTwinHistory({ siteId: props?.siteId!, twinId: props?.twinId! }),
      {
        wrapper: Wrapper,
        initialProps: {
          siteId,
          twinId,
        },
      }
    )
    await waitFor(() => {
      // Check initial state
      expect(result.current.showVersionHistory).toBeTruthy()
      expect(result.current.versionHistories.length).toBe(5)
      expect(result.current.selectedVersion).toBeNull()
      expect(result.current.previousVersion).toBeUndefined()
      expect(result.current.versionHistoryEditedFields).toEqual({})
    })

    // Select initial version, the last object in versionHistories.
    act(() => {
      result.current.setVersionHistoryIndex(4)
    })
    expect(result.current.selectedVersion).toEqual(
      result.current.versionHistories[
        result.current.versionHistories.length - 1
      ]
    )
    // There will be no previousVersion when we select the initial version.
    expect(result.current.previousVersion).toBeUndefined()
    // versionHistoryEditedFields should contain all the fields in the initial version.
    expect(result.current.versionHistoryEditedFields).toEqual(
      _.omit(result.current.selectedVersion!.twin, [
        'metadata',
        'etag',
        '$metadata',
      ])
    )

    // Select version 1
    act(() => {
      result.current.setVersionHistoryIndex(3)
    })
    expect(result.current.selectedVersion).toEqual(
      result.current.versionHistories[3]
    )
    expect(result.current.previousVersion).toEqual(
      result.current.versionHistories[4]
    )
    // versionHistoryEditedFields should contain all the fields that've been edited.
    expect(result.current.versionHistoryEditedFields).toEqual(version1Edits)

    // Select version 2
    act(() => {
      result.current.setVersionHistoryIndex(2)
    })
    expect(result.current.selectedVersion).toEqual(
      result.current.versionHistories[2]
    )
    expect(result.current.previousVersion).toEqual(
      result.current.versionHistories[3]
    )
    // versionHistoryEditedFields should contain all the fields that've been edited
    expect(result.current.versionHistoryEditedFields).toEqual(version2Edits)

    // Select version 3
    act(() => {
      result.current.setVersionHistoryIndex(1)
    })
    expect(result.current.selectedVersion).toEqual(
      result.current.versionHistories[1]
    )
    expect(result.current.previousVersion).toEqual(
      result.current.versionHistories[2]
    )
    // versionHistoryEditedFields should contain all the fields that've been edited
    expect(result.current.versionHistoryEditedFields).toEqual(version3Edits)

    // Select version 4
    act(() => {
      result.current.setVersionHistoryIndex(0)
    })
    expect(result.current.selectedVersion).toEqual(
      result.current.versionHistories[0]
    )
    expect(result.current.previousVersion).toEqual(
      result.current.versionHistories[1]
    )
    // versionHistoryEditedFields should contain all the fields that've been edited
    expect(result.current.versionHistoryEditedFields).toEqual(version4Edits)
  })
})
