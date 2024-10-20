/* eslint-disable @typescript-eslint/no-non-null-assertion */
import { ReactNode, useState } from 'react'
import { renderHook, RenderHookResult, waitFor } from '@testing-library/react'
import { act } from 'react-test-renderer'
import 'fake-indexeddb/auto'
import {
  InspectionRecordsProvider,
  useInspectionRecords,
  getSyncPayload,
  getAttachmentOperations,
  applySyncResult,
} from '../InspectionRecordsContext'
import {
  Inspection,
  InspectionRecord,
  IApi,
  IStore,
  CheckRecord,
  CheckWithoutInspectionId,
  AttachmentRequest,
  InspectionsContextType,
  SyncPayload,
  SyncResult,
  NumericCheckRecordInput,
} from '../types'
import { makeInspection, makeInspectionRecord } from '../testUtils'
import { DummyIndexedDbStore, IndexedDbStore } from '../InspectionRecordsDb'
import { Blob } from 'blob-polyfill'

global.Blob = Blob

// A couple of temporary hacks in this file to unblock build.
async function waitForValueToChange(fn) {
  const val = fn()
  await waitFor(() => expect(fn()).not.toEqual(val))
}

const oneCheck: CheckWithoutInspectionId[] = [
  {
    id: 'check1',
    sortOrder: 2,
    name: '432',
    type: 'numeric',
    typeValue: '1',
    decimalPlaces: 2,
    minValue: 3,
    maxValue: 4,
  },
]

const twoChecks: CheckWithoutInspectionId[] = [
  {
    id: 'check1',
    sortOrder: 2,
    name: '432',
    type: 'numeric',
    typeValue: '1',
    decimalPlaces: 2,
    minValue: 3,
    maxValue: 4,
  },
  {
    id: 'check2',
    sortOrder: 2,
    name: '543',
    type: 'numeric',
    typeValue: '1',
    decimalPlaces: 2,
    minValue: 3,
    maxValue: 4,
  },
]

const twoChecksWithDependency: CheckWithoutInspectionId[] = [
  {
    id: 'check1',
    sortOrder: 2,
    name: '432',
    type: 'list',
    typeValue: 'on|off',
  },
  {
    id: 'check2',
    sortOrder: 2,
    name: '543',
    type: 'numeric',
    typeValue: '1',
    decimalPlaces: 2,
    minValue: 3,
    maxValue: 4,
    dependencyId: 'check1',
    dependencyValue: 'on',
  },
]

function defaultSync(payload: SyncPayload): SyncResult {
  return {
    inspectionRecords: Object.fromEntries(
      Object.entries(payload).map(([inspectionRecordId, inspectionRecord]) => [
        inspectionRecordId,
        {
          checkRecords: Object.fromEntries(
            Object.entries(inspectionRecord.checkRecords).map(
              ([checkRecordId, checkRecord]) => [
                checkRecordId,
                {
                  result: 'success',
                  attachments: [],
                },
              ]
            )
          ),
        },
      ])
    ),
  }
}

function makeDummyApi(
  inspectionRecords: InspectionRecord[],
  sync: (payload: SyncPayload) => SyncResult
): { api: IApi; syncPayloads: SyncPayload[] } {
  const syncPayloads: SyncPayload[] = []
  return {
    api: {
      getInspectionLastRecord: (siteId, inspectionId) =>
        Promise.resolve(
          inspectionRecords.find((r) => r.inspectionId === inspectionId)!
        ),
      addAttachment: (
        siteId: string,
        checkRecordId: string,
        attachment: AttachmentRequest
      ) =>
        Promise.resolve({
          id: '123',
          type: 'image',
          fileName: attachment.fileName,
          url: 'https://willowinc.com/image',
          previewUrl: 'https://willowinc.com/image',
          createdDate: '123',
        }),
      deleteAttachment: (
        _siteId: string,
        _checkRecordId: string,
        _attachmentId: string
      ) => Promise.resolve(),
      saveCheckRecord: (
        _siteId: string,
        _inspectionId: string,
        _checkRecordId: string,
        _checkRecord: CheckRecord
      ) => Promise.resolve(),
      sync: (payload: SyncPayload) => {
        syncPayloads.push(payload)
        return Promise.resolve((sync ?? defaultSync)(payload))
      },
    },

    syncPayloads,
  }
}

function setup({
  inspections,
  inspectionRecords = inspections.map(makeInspectionRecord),
  sync = defaultSync,
  store = new DummyIndexedDbStore(),
}: {
  inspections: Inspection[]
  inspectionRecords?: InspectionRecord[]
  sync?: (payload: SyncPayload) => SyncResult
  store?: IStore
}) {
  const { api, syncPayloads } = makeDummyApi(inspectionRecords, sync)

  function Wrapper({ children }: { children?: ReactNode }) {
    return (
      <InspectionRecordsProvider api={api} store={store}>
        {children}
      </InspectionRecordsProvider>
    )
  }

  const renderResult = renderHook<InspectionsContextType, unknown>(
    useInspectionRecords,
    { wrapper: Wrapper }
  )

  return {
    renderResult,
    syncPayloads,
    initialInspections: inspections,
    initialInspectionRecords: inspectionRecords,
  }
}

const waitForInspectionRecordToChange = async (
  result: RenderHookResult<InspectionsContextType, unknown>['result']
) => {
  const initialRecords = result.current.inspectionRecords
  await waitFor(() => {
    expect(result.current.inspectionRecords).not.toBe(initialRecords)
  })
}

describe('InspectionContext', () => {
  test('should initialise properly', async () => {
    const {
      renderResult: { result },
      initialInspections,
      initialInspectionRecords,
    } = setup({ inspections: [makeInspection({ checks: oneCheck })] })
    const inspection = initialInspections[0]
    const inspectionRecord = initialInspectionRecords[0]

    result.current.loadInspection('site123', inspection.id)

    await waitForInspectionRecordToChange(result)

    const { inspectionRecords } = result.current
    expect(Object.keys(inspectionRecords)).toEqual([inspectionRecord.id])

    const ir = inspectionRecords[inspectionRecord.id]
    expect(ir.id).toEqual(inspectionRecord.id)
    expect(ir.inspection.id).toEqual(inspection.id)
    expect(ir.rows).toHaveLength(1)
    expect(ir.rows[0].check.id).toEqual('check1')
    expect(ir.rows[0].checkRecord.checkId).toEqual('check1')
    expect(ir.rows[0].attachments).toEqual([])
    expect(ir.rows[0].modified).toBeFalse()

    expect(getSyncPayload(result.current)).toEqual({})
    expect(getAttachmentOperations(result.current)).toEqual([])
  })

  test('should add an entry to the buffer on submitCheck', async () => {
    const {
      renderResult: { result },
      initialInspectionRecords,
    } = setup({ inspections: [makeInspection({ checks: oneCheck })] })
    const inspectionRecord = initialInspectionRecords[0]

    result.current.loadInspection('site123', inspectionRecord.inspectionId)
    await waitForInspectionRecordToChange(result)

    act(() => {
      result.current.submitCheck(
        inspectionRecord.id,
        inspectionRecord.checkRecords[0].id,
        {
          numberValue: 999,
        }
      )
    })

    const { inspectionRecords } = result.current

    const ir = inspectionRecords[inspectionRecord.id]
    const { checkRecord } = ir.rows[0]
    expect(
      'numberValue' in checkRecord && checkRecord.numberValue === 999
    ).toBeTrue()

    const syncPayload = getSyncPayload(result.current)
    expect(Object.keys(syncPayload)).toEqual([inspectionRecord.id])
    expect(Object.keys(syncPayload[inspectionRecord.id].checkRecords)).toEqual([
      inspectionRecord.checkRecords[0].id,
    ])
    const syncCheckRecord =
      syncPayload[inspectionRecord.id].checkRecords[
        inspectionRecord.checkRecords[0].id
      ]
    expect(
      'numberValue' in syncCheckRecord.data &&
        syncCheckRecord.data.numberValue === 999
    ).toBeTrue()

    expect(getAttachmentOperations(result.current)).toEqual([])
  })

  test('should advance to the next entry on submitCheck', async () => {
    const {
      renderResult: { result },
      initialInspectionRecords,
    } = setup({ inspections: [makeInspection({ checks: twoChecks })] })
    const inspectionRecord = initialInspectionRecords[0]

    result.current.loadInspection('site123', inspectionRecord.inspectionId)
    await waitForInspectionRecordToChange(result)
    act(() => {
      result.current.submitCheck(
        inspectionRecord.id,
        inspectionRecord.checkRecords[0].id,
        {
          numberValue: 999,
        }
      )
    })

    const { inspectionRecords } = result.current

    const ir = inspectionRecords[inspectionRecord.id]
    expect(ir.activeCheckId).toEqual('check2')
  })

  test('should not open notRequired records', async () => {
    // We have two checks. The second check depends on the first check, which
    // we haven't filled out yet. So when we click on the second check, it
    // should do nothing and the first check should remain selected.
    const {
      renderResult: { result },
      initialInspectionRecords,
    } = setup({
      inspections: [makeInspection({ checks: twoChecksWithDependency })],
    })
    const inspectionRecord = initialInspectionRecords[0]

    result.current.loadInspection('site123', inspectionRecord.inspectionId)
    await waitForInspectionRecordToChange(result)
    act(() => {
      result.current.selectCheck(
        inspectionRecord.id,
        twoChecksWithDependency[1].id
      )
    })

    await waitFor(() => {
      const ir = result.current.inspectionRecords[inspectionRecord.id]
      expect(ir.activeCheckId).toEqual(null)
    })
  })

  test('should add an attachment to the buffer on addAttachment', async () => {
    const {
      renderResult: { result },
      initialInspectionRecords,
    } = setup({ inspections: [makeInspection({ checks: oneCheck })] })
    const inspectionRecord = initialInspectionRecords[0]

    result.current.loadInspection('site123', inspectionRecord.inspectionId)
    await waitForInspectionRecordToChange(result)
    act(() => {
      result.current.addAttachment(
        inspectionRecord.id,
        inspectionRecord.checkRecords[0].id,
        {
          id: 123,
          fileName: 'chucknorris.png',
          file: new File(['(⌐□_□)'], 'chucknorris.png', { type: 'image/png' }),
          url: 'https://willowinc.com/something',
          previewUrl: 'https://willowinc.com/something',
          base64: '123',
        }
      )
    })

    const { inspectionRecords } = result.current

    const ir = inspectionRecords[inspectionRecord.id]
    expect(ir.rows[0].attachments).toHaveLength(1)
    expect(ir.rows[0].attachments[0].status).toEqual('added')
    expect(ir.rows[0].attachments[0].attachment.fileName).toEqual(
      'chucknorris.png'
    )

    const attachmentOperations = getAttachmentOperations(result.current)
    expect(attachmentOperations).toHaveLength(1)
    const attachment = attachmentOperations[0]
    expect(attachment.type).toEqual('add')
    expect(attachment.checkRecordId).toEqual(ir.rows[0].checkRecord.id)
    expect(
      attachment.type === 'add' &&
        attachment.attachment.fileName === 'chucknorris.png'
    ).toBeTrue()
  })

  test('should not do anything if we add and remove the same attachment before saving', async () => {
    const {
      renderResult: { result },
      initialInspectionRecords,
    } = setup({ inspections: [makeInspection({ checks: oneCheck })] })
    const inspectionRecord = initialInspectionRecords[0]

    result.current.loadInspection('site123', inspectionRecord.inspectionId)
    await waitForInspectionRecordToChange(result)
    act(() => {
      result.current.addAttachment(
        inspectionRecord.id,
        inspectionRecord.checkRecords[0].id,
        {
          id: 123,
          fileName: 'chucknorris.png',
          file: new File(['(⌐□_□)'], 'chucknorris.png', { type: 'image/png' }),
          url: 'https://willowinc.com/something',
          previewUrl: 'https://willowinc.com/something',
          base64: '123',
        }
      )
    })

    act(() => {
      result.current.deleteAttachment(
        inspectionRecord.id,
        inspectionRecord.checkRecords[0].id,
        123
      )
    })

    const { inspectionRecords } = result.current

    const ir = inspectionRecords[inspectionRecord.id]
    expect(ir.rows[0].attachments).toHaveLength(0)

    const attachmentOperations = getAttachmentOperations(result.current)
    expect(attachmentOperations).toHaveLength(0)
  })

  test('should delete existing attachments if we say so', async () => {
    const inspection = makeInspection({ checks: oneCheck })
    const inspectionRecord = makeInspectionRecord(inspection)
    inspectionRecord.checkRecords[0].attachments = [
      {
        id: 'chuck',
        fileName: 'chucknorris.png',
        url: 'https://willowinc.com/something',
        previewUrl: 'https://willowinc.com/something',
        type: 'image',
        createdDate: '123',
      },
    ]

    const {
      renderResult: { result },
      initialInspectionRecords,
    } = setup({
      inspections: [inspection],
      inspectionRecords: [inspectionRecord],
    })

    result.current.loadInspection('site123', inspectionRecord.inspectionId)
    await waitForInspectionRecordToChange(result)
    act(() => {
      result.current.deleteAttachment(
        inspectionRecord.id,
        inspectionRecord.checkRecords[0].id,
        'chuck'
      )
    })

    const { inspectionRecords } = result.current

    const ir = inspectionRecords[inspectionRecord.id]
    expect(ir.rows[0].attachments).toHaveLength(1)
    expect(ir.rows[0].attachments[0].status).toEqual('deleted')

    const attachmentOperations = getAttachmentOperations(result.current)
    expect(attachmentOperations).toEqual([
      {
        type: 'delete',
        checkRecordId: inspectionRecord.checkRecords[0].id,
        attachmentId: 'chuck',
      },
    ])
  })

  test('should work with multiple inspection records', async () => {
    const inspections = [
      makeInspection({ checks: oneCheck }),
      makeInspection({ checks: twoChecks }),
    ]

    const {
      renderResult: { result },
      initialInspections,
      initialInspectionRecords,
    } = setup({ inspections })

    result.current.loadInspection('site123', initialInspections[0].id)
    result.current.loadInspection('site123', initialInspections[1].id)

    await waitForInspectionRecordToChange(result)

    const { inspectionRecords } = result.current

    expect(Object.keys(inspectionRecords)).toIncludeSameMembers([
      initialInspectionRecords[0].id,
      initialInspectionRecords[1].id,
    ])
    expect(
      inspectionRecords[initialInspectionRecords[0].id].rows.map(
        (r) => r.check.id
      )
    ).toEqual(['check1'])
    expect(
      inspectionRecords[initialInspectionRecords[1].id].rows.map(
        (r) => r.check.id
      )
    ).toEqual(['check1', 'check2'])

    act(() => {
      result.current.submitCheck(
        initialInspectionRecords[0].id,
        initialInspectionRecords[0].checkRecords[0].id,
        {
          numberValue: 111,
        }
      )
    })
    act(() => {
      result.current.submitCheck(
        initialInspectionRecords[1].id,
        initialInspectionRecords[1].checkRecords[1].id,
        {
          numberValue: 222,
        }
      )
    })

    const syncPayload = getSyncPayload(result.current)

    const firstCheckRecord =
      syncPayload[initialInspectionRecords[0].id].checkRecords[
        initialInspectionRecords[0].checkRecords[0].id
      ]
    expect(
      'numberValue' in firstCheckRecord.data &&
        firstCheckRecord.data.numberValue === 111
    )

    const secondCheckRecord =
      syncPayload[initialInspectionRecords[1].id].checkRecords[
        initialInspectionRecords[1].checkRecords[1].id
      ]
    expect(
      'numberValue' in secondCheckRecord.data &&
        secondCheckRecord.data.numberValue === 222
    )
  })

  describe('with IndexedDb store', () => {
    test('should be able to submit a check in one session and read it in another', async () => {
      const inspections = [makeInspection({ checks: twoChecks })]
      const inspectionRecords = inspections.map(makeInspectionRecord)

      const { renderResult: renderResult1 } = setup({
        inspections,
        inspectionRecords,
        store: await IndexedDbStore.create('hello'),
      })

      renderResult1.result.current.loadInspection('site123', inspections[0].id)

      await waitForInspectionRecordToChange(renderResult1.result)

      renderResult1.result.current.submitCheck(
        inspectionRecords[0].id,
        inspectionRecords[0].checkRecords[0].id,
        {
          numberValue: 11911,
        }
      )

      await waitFor(() => {
        const { checkRecord } =
          renderResult1.result.current.inspectionRecords[
            inspectionRecords[0].id
          ].rows[0]
        expect(
          'numberValue' in checkRecord && checkRecord.numberValue === 11911
        ).toBeTrue()
      })

      const { renderResult: renderResult2 } = setup({
        inspections,
        inspectionRecords,
        store: await IndexedDbStore.create('hello'),
      })

      renderResult2.result.current.loadInspection('site123', inspections[0].id)

      await waitForInspectionRecordToChange(renderResult2.result)

      await waitFor(() => {
        const { checkRecord } =
          renderResult2.result.current.inspectionRecords[
            inspectionRecords[0].id
          ].rows[0]
        expect(
          'numberValue' in checkRecord && checkRecord.numberValue === 11911
        ).toBeTrue()
      })
    })

    test('should be able to add an attachment in one session and read it in another', async () => {
      const inspections = [makeInspection({ checks: twoChecks })]
      const inspectionRecords = inspections.map(makeInspectionRecord)

      const { renderResult: renderResult1 } = setup({
        inspections,
        inspectionRecords,
        store: await IndexedDbStore.create('hello'),
      })

      renderResult1.result.current.loadInspection('site123', inspections[0].id)
      await waitForInspectionRecordToChange(renderResult1.result)

      renderResult1.result.current.addAttachment(
        inspectionRecords[0].id,
        inspectionRecords[0].checkRecords[0].id,
        {
          id: 123,
          fileName: 'chucknorris.png',
          file: new File(['(⌐□_□)'], 'chucknorris.png', { type: 'image/png' }),
          url: 'https://willowinc.com/something',
          previewUrl: 'https://willowinc.com/something',
          base64: '123',
        }
      )

      await waitFor(() => {
        const row =
          renderResult1.result.current.inspectionRecords[
            inspectionRecords[0].id
          ].rows[0]
        expect(row.attachments.length).toBe(1)
      })

      await new Promise((r) => setTimeout(r, 1000))

      const { renderResult: renderResult2 } = setup({
        inspections,
        inspectionRecords,
        store: await IndexedDbStore.create('hello'),
      })

      renderResult2.result.current.loadInspection('site123', inspections[0].id)
      await waitForInspectionRecordToChange(renderResult2.result)

      await waitFor(() => {
        const row =
          renderResult2.result.current.inspectionRecords[
            inspectionRecords[0].id
          ].rows[0]
        expect(row.attachments.length).toBe(1)
      })

      await new Promise((r) => setTimeout(r, 200))
    })

    test('should not load an old inspection record if it has been replaced', async () => {
      const inspections = [makeInspection({ checks: twoChecks })]

      const inspectionRecords = inspections.map(makeInspectionRecord)
      const store = await IndexedDbStore.create('hello')

      const { renderResult, syncPayloads } = setup({
        inspections,
        inspectionRecords,
        store,
      })

      renderResult.result.current.loadInspection('site123', inspections[0].id)

      await waitForValueToChange(
        () => renderResult.result.current.inspectionRecords
      )

      renderResult.result.current.submitCheck(
        inspectionRecords[0].id,
        inspectionRecords[0].checkRecords[0].id,
        {
          numberValue: 1111,
        }
      )

      await waitForValueToChange(
        () => renderResult.result.current.inspectionRecords
      )

      // Replace the inspection record. Do this in place, because our dummy API
      // keeps a reference to this array.
      inspectionRecords.splice(
        0,
        inspectionRecords.length,
        ...inspections.map(makeInspectionRecord)
      )

      renderResult.result.current.loadInspection('site123', inspections[0].id)

      await waitForValueToChange(
        () => renderResult.result.current.inspectionRecords
      )

      renderResult.result.current.submitCheck(
        inspectionRecords[0].id,
        inspectionRecords[0].checkRecords[0].id,
        {
          numberValue: 2222,
        }
      )

      await waitForValueToChange(
        () => renderResult.result.current.inspectionRecords
      )

      // Make sure that we submitted to two different inspection records.
      expect(syncPayloads).toHaveLength(2)
      expect(Object.keys(syncPayloads[0])).toHaveLength(1)
      expect(Object.keys(syncPayloads[1])).toHaveLength(1)
      expect(Object.keys(syncPayloads[0])).not.toEqual(
        Object.keys(syncPayloads[1])
      )

      const firstCheckRecords = Object.values(syncPayloads[0])[0].checkRecords
      const secondCheckRecords = Object.values(syncPayloads[1])[0].checkRecords
      expect(
        (Object.values(firstCheckRecords)[0].data as NumericCheckRecordInput)
          .numberValue === 1111
      )
      expect(
        (Object.values(secondCheckRecords)[0].data as NumericCheckRecordInput)
          .numberValue === 2222
      )
      expect(Object.keys(firstCheckRecords)).toHaveLength(1)
      expect(Object.keys(secondCheckRecords)).toHaveLength(1)
      expect(Object.keys(firstCheckRecords)).not.toEqual(
        Object.keys(secondCheckRecords)
      )
    })
  })
})

describe('applySyncResult', () => {
  let inspection: Inspection | undefined
  let inspectionRecord: InspectionRecord | undefined
  let result:
    | RenderHookResult<InspectionsContextType, unknown>['result']
    | undefined
  const newAttachmentId = 123

  async function setupWithSyncResult(
    sync: (payload: SyncPayload) => SyncResult
  ) {
    inspection = makeInspection({ checks: oneCheck })
    inspectionRecord = makeInspectionRecord(inspection)
    inspectionRecord.checkRecords[0].attachments = [
      {
        id: 'existing-attachment',
        fileName: 'chucknorris.png',
        url: 'https://willowinc.com/something',
        previewUrl: 'https://willowinc.com/something',
        type: 'image',
        createdDate: '123',
      },
    ]

    const { renderResult } = setup({
      inspections: [inspection],
      inspectionRecords: [inspectionRecord],
      sync,
    })

    renderResult.result.current.loadInspection(
      'site123',
      inspectionRecord.inspectionId
    )
    const initialValue = renderResult.result.current.inspectionRecords
    await waitFor(() => {
      expect(renderResult.result.current.inspectionRecords).not.toBe(
        initialValue
      )
    })

    result = renderResult.result

    act(() => {
      result!.current.submitCheck(
        inspectionRecord!.id,
        inspectionRecord!.checkRecords[0].id,
        {
          numberValue: 999,
        }
      )
    })

    act(() => {
      result!.current.addAttachment(
        inspectionRecord!.id,
        inspectionRecord!.checkRecords[0].id,
        {
          id: newAttachmentId,
          fileName: 'chucknorris.png',
          file: new File(['(⌐□_□)'], 'chucknorris.png', { type: 'image/png' }),
          url: 'https://willowinc.com/something',
          previewUrl: 'https://willowinc.com/something',
          base64: '123',
        }
      )
    })

    return { renderResult }
  }

  test('should mark check record as not modified on success', async () => {
    const { renderResult } = await setupWithSyncResult(() => ({
      inspectionRecords: {
        [inspectionRecord!.id]: {
          checkRecords: {
            [inspectionRecord!.checkRecords[0].id]: {
              result: 'success',
              attachments: [],
            },
          },
        },
      },
    }))

    await new Promise((r) => setTimeout(r, 1000))

    const appliedSuccess = renderResult.result.current
    expect(
      appliedSuccess.inspectionRecords[inspectionRecord!.id].rows[0].modified
    ).toBeFalse()
  })

  test('should leave check record as modified on failure', async () => {
    const { renderResult } = await setupWithSyncResult(() => ({
      inspectionRecords: {
        [inspectionRecord!.id]: {
          checkRecords: {
            [inspectionRecord!.checkRecords[0].id]: {
              result: 'error',
              attachments: [],
            },
          },
        },
      },
    }))
    const appliedError = renderResult.result.current
    expect(
      appliedError.inspectionRecords[inspectionRecord!.id].rows[0].modified
    ).toBeTrue()
  })

  test('should leave check record as modified if there is no result', async () => {
    const { renderResult } = await setupWithSyncResult(() => ({
      inspectionRecords: {},
    }))
    const appliedAbsent = renderResult.result.current
    expect(
      appliedAbsent.inspectionRecords[inspectionRecord!.id].rows[0].modified
    ).toBeTrue()
  })

  test("should mark added attachment as 'existing' on success", async () => {
    const { renderResult } = await setupWithSyncResult(() => ({
      inspectionRecords: {
        [inspectionRecord!.id]: {
          checkRecords: {
            [inspectionRecord!.checkRecords[0].id]: {
              result: 'success',
              attachments: [
                {
                  attachmentId: newAttachmentId,
                  result: {
                    type: 'added',
                    serverId: '123',
                    url: 'https://command.willowinc.com/attachments/123',
                    previewUrl:
                      'https://command.willowinc.com/attachments/preview/123',
                  },
                },
              ],
            },
          },
        },
      },
    }))

    await new Promise((r) => setTimeout(r, 1000))

    const appliedAttachmentAdd = renderResult.result.current

    const addedAttachment =
      appliedAttachmentAdd.inspectionRecords[inspectionRecord!.id].rows[0]
        .attachments[1]
    expect(addedAttachment.status).toEqual('existing')
    expect(addedAttachment.attachment.id).toEqual('123')
    expect(addedAttachment.attachment.url).toEqual(
      'https://command.willowinc.com/attachments/123'
    )
    expect(addedAttachment.attachment.previewUrl).toEqual(
      'https://command.willowinc.com/attachments/preview/123'
    )
  })

  test('should hard delete deleted attachment on success', async () => {
    const { renderResult } = await setupWithSyncResult(() => ({
      inspectionRecords: {
        [inspectionRecord!.id]: {
          checkRecords: {
            [inspectionRecord!.checkRecords[0].id]: {
              result: 'success',
              attachments: [
                {
                  attachmentId:
                    inspectionRecord!.checkRecords[0].attachments[0].id,
                  result: { type: 'deleted' },
                },
              ],
            },
          },
        },
      },
    }))

    await new Promise((r) => setTimeout(r, 1000))

    const appliedAttachmentDelete = renderResult.result.current
    expect(
      appliedAttachmentDelete.inspectionRecords[
        inspectionRecord!.id
      ].rows[0].attachments.map((a) => a.attachment.id)
    ).toEqual([newAttachmentId])
  })

  test('should leave attachments as they are on error', async () => {
    const { renderResult } = await setupWithSyncResult(() => ({
      inspectionRecords: {
        [inspectionRecord!.id]: {
          checkRecords: {
            [inspectionRecord!.checkRecords[0].id]: {
              result: 'success',
              attachments: [
                {
                  attachmentId:
                    inspectionRecord!.checkRecords[0].attachments[0].id,
                  result: { type: 'error' },
                },
                {
                  attachmentId: newAttachmentId,
                  result: { type: 'error' },
                },
              ],
            },
          },
        },
      },
    }))
    const appliedAttachmentError = renderResult.result.current
    expect(
      appliedAttachmentError.inspectionRecords[
        inspectionRecord!.id
      ].rows[0].attachments.map((a) => a.attachment.id)
    ).toEqual(['existing-attachment', newAttachmentId])
    expect(
      appliedAttachmentError.inspectionRecords[
        inspectionRecord!.id
      ].rows[0].attachments.map((a) => a.status)
    ).toEqual(['existing', 'added'])
  })
})
