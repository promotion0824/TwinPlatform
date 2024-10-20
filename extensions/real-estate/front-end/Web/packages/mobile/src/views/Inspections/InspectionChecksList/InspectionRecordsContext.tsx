/* eslint-disable  arrow-body-style */
import _ from 'lodash'
import produce from 'immer'
import { DateTime } from 'luxon'
import { ReactNode, useCallback, useEffect } from 'react'
import create, { UseBoundStore, Mutate, StoreApi } from 'zustand'
import createContext from 'zustand/context'
import {
  AttachmentEntry,
  AttachmentOperation,
  CheckRecord,
  CheckRecordInput,
  CheckRow,
  ExtendedStatus,
  IApi,
  IStore,
  InspectionRecord,
  InspectionRecordPage,
  InspectionsContextType,
  InspectionsState,
  SyncPayload,
  SyncResult,
  UnsavedAttachment,
  PartialInspectionRecordPage,
  NumericCheckRecordInput,
  ListCheckRecordInput,
  DateCheckRecordInput,
} from './types'

// https://github.com/pmndrs/zustand/issues/882#issuecomment-1091756847
type UseInspectionRecordsStore = UseBoundStore<
  Mutate<StoreApi<InspectionsContextType>, []>
>

export const InspectionRecordsContext =
  createContext<UseInspectionRecordsStore>()

export function getInitialState() {
  return {
    inspectionRecords: {},
    isSyncing: false,
    unsynced: {
      justSubmittedFirst: false,
      displayMessage: false,
    },
  }
}

/**
 * Take an `InspectionRecord` (from a server response) and turn it into an
 * `InspectionRecordPage`, which is the data structure we use for an inspection
 * record in the frontend.
 */
export function makeInspectionRecordPage(
  inspectionRecord: InspectionRecord,
  siteId: string,
  isInitialLoad?: boolean
): InspectionRecordPage {
  const rows: CheckRow[] = inspectionRecord.checkRecords.map((checkRecord) => {
    const check = inspectionRecord.inspection.checks.find(
      (c) => c.id === checkRecord.checkId
    )

    if (check == null) {
      throw new Error(`Check record ${checkRecord.id} had no matching check`)
    }

    return {
      checkRecord,
      check,
      attachments: (checkRecord.attachments ?? []).map((a) => ({
        attachment: a,
        status: 'existing',
      })),
      modified: false,
      syncStatus: null,
    }
  })
  rows.sort((a, b) => a.check.sortOrder - b.check.sortOrder)

  if (isInitialLoad) {
    const temp = {
      id: inspectionRecord.id,
      inspection: inspectionRecord.inspection,
      effectiveAt: inspectionRecord.effectiveAt,
      expiresAt: inspectionRecord.expiresAt,
      rows,
      siteId,
    }
    const isComplete = (getNextCheckRecord(temp)?.check?.id ?? null) == null
    return {
      ...temp,
      activeCheckId: null,
      isComplete,
    }
  }
  return setActiveCheckId({
    id: inspectionRecord.id,
    inspection: inspectionRecord.inspection,
    effectiveAt: inspectionRecord.effectiveAt,
    expiresAt: inspectionRecord.expiresAt,
    rows,
    siteId,
  })
}

function setActiveCheckId(
  ir: PartialInspectionRecordPage
): InspectionRecordPage {
  const activeCheckId = getNextCheckRecord(ir)?.check?.id ?? null
  return {
    ...ir,
    activeCheckId,
    isComplete: activeCheckId == null,
  }
}

function loadInspectionRecord(
  state: InspectionsState,
  inspectionRecord: InspectionRecordPage
) {
  return {
    ...state,
    inspectionRecords: {
      ...state.inspectionRecords,
      [inspectionRecord.id]: inspectionRecord,
    },
    inspectionRecordId: inspectionRecord.id,
  }
}

function getTimeStamp() {
  return DateTime.utc().toISO()?.replace(/Z$/, '') ?? ''
}

/**
 * Select the specified check, but only if it's not notRequired.
 */
function selectCheck(
  state: InspectionsState,
  inspectionRecordId: string,
  checkId: string
) {
  const inspectionRecord = state.inspectionRecords[inspectionRecordId]
  const row = inspectionRecord?.rows?.find((r) => r.check.id === checkId)
  if (
    inspectionRecord != null &&
    row != null &&
    getCheckRecordStatus(row, inspectionRecord) === 'notRequired'
  ) {
    return state
  }

  return {
    ...state,
    inspectionRecords: {
      ...state.inspectionRecords,
      [inspectionRecordId]: {
        ...state.inspectionRecords[inspectionRecordId],
        activeCheckId: checkId,
      },
    },
  }
}

function submitCheckRecord(
  state: InspectionsState,
  inspectionRecordId: string,
  checkRecordId: string,
  checkData: CheckRecordInput
) {
  const isFirst = !hasUnsyncedRecords(state)
  const ir: InspectionRecordPage = setActiveCheckId({
    ...state.inspectionRecords[inspectionRecordId],
    rows: state.inspectionRecords[inspectionRecordId].rows.map(
      (checkRecord) => {
        if (checkRecord.checkRecord.id === checkRecordId) {
          return {
            ...checkRecord,
            checkRecord: {
              ...checkRecord.checkRecord,
              ...checkData,
            },
            modified: true,
            enteredAt: DateTime.utc().toISO()?.replace(/Z$/, ''),
          }
        } else {
          return checkRecord
        }
      }
    ),
  })

  return {
    ...state,
    inspectionRecords: {
      ...state.inspectionRecords,
      [inspectionRecordId]: ir,
    },
    unsynced: {
      ...state.unsynced,
      justSubmittedFirst: isFirst && !state.unsynced.justSubmittedFirst,
    },
  }
}

function addAttachment(
  state: InspectionsState,
  inspectionRecordId: string,
  checkRecordId: string,
  attachment: UnsavedAttachment
) {
  const ir: InspectionRecordPage = {
    ...state.inspectionRecords[inspectionRecordId],
    rows: state.inspectionRecords[inspectionRecordId].rows.map(
      (checkRecord) => {
        if (checkRecord.checkRecord.id === checkRecordId) {
          return {
            ...checkRecord,
            attachments: [
              ...checkRecord.attachments,
              {
                attachment,
                status: 'added',
              },
            ],
          }
        } else {
          return checkRecord
        }
      }
    ),
  }

  return {
    ...state,
    inspectionRecords: {
      ...state.inspectionRecords,
      [inspectionRecordId]: ir,
    },
  }
}

function deleteAttachment(
  state: InspectionsState,
  inspectionRecordId: string,
  checkRecordId: string,
  attachmentId: number | string
) {
  const ir: InspectionRecordPage = {
    ...state.inspectionRecords[inspectionRecordId],
    rows: state.inspectionRecords[inspectionRecordId].rows.map(
      (checkRecord) => {
        if (checkRecord.checkRecord.id === checkRecordId) {
          const existingAttachment = checkRecord.attachments.find(
            (a) => a.attachment.id === attachmentId
          )
          if (existingAttachment != null) {
            let attachments: AttachmentEntry[] | undefined
            if (existingAttachment.status === 'added') {
              // If the attachment was added offline, we can hard delete it.
              attachments = checkRecord.attachments.filter(
                (a) => a !== existingAttachment
              )
            } else {
              // Otherwise we just mark it as deleted, and save the deletion
              // when we sync.
              attachments = checkRecord.attachments.map((a) =>
                a === existingAttachment ? { ...a, status: 'deleted' } : a
              )
            }
            return { ...checkRecord, attachments }
          }
        }
        return checkRecord
      }
    ),
  }

  return {
    ...state,
    inspectionRecords: {
      ...state.inspectionRecords,
      [inspectionRecordId]: ir,
    },
  }
}

function beginSync(state: InspectionsState, syncPayload: SyncPayload) {
  return produce(state, (draftState) => {
    draftState.isSyncing = true
    for (const [inspectionRecordId, inspectionRecord] of Object.entries(
      syncPayload
    )) {
      for (const [checkRecordId, checkRecord] of Object.entries(
        inspectionRecord.checkRecords
      )) {
        const cr = draftState.inspectionRecords[inspectionRecordId].rows.find(
          (r) => r.checkRecord.id === checkRecordId
        )
        if (cr != null) {
          cr.syncStatus = 'loading'
        }
      }
    }
  })
}

function resetLoading(state: InspectionsState, syncPayload: SyncPayload) {
  return produce(state, (draftState) => {
    for (const [inspectionRecordId, inspectionRecord] of Object.entries(
      syncPayload
    )) {
      for (const [checkRecordId, checkRecord] of Object.entries(
        inspectionRecord.checkRecords
      )) {
        const cr = draftState.inspectionRecords[inspectionRecordId].rows.find(
          (r) => r.checkRecord.id === checkRecordId
        )
        if (cr != null) {
          cr.syncStatus = null
        }
      }
    }
  })
}

function displayWillSyncLater(state: InspectionsState, value: boolean) {
  return {
    ...state,
    unsynced: {
      displayMessage: value,
      justSubmittedFirst: !value && state.unsynced.justSubmittedFirst,
    },
  }
}

/**
 * Get the current status of a check record. This should be the same value that
 * the server would return for a check record in this state. It may not be the
 * same as the `status` field of the check record as originally returned.
 */
export function getCheckRecordStatus(
  row: CheckRow,
  inspectionRecord: PartialInspectionRecordPage
): CheckRecord['status'] {
  if (row.checkRecord.status === 'missed') {
    return 'missed'
  } else if (
    ('stringValue' in row.checkRecord && row.checkRecord.stringValue != null) ||
    ('numberValue' in row.checkRecord && row.checkRecord.numberValue != null) ||
    ('dateValue' in row.checkRecord && row.checkRecord.dateValue != null)
  ) {
    return 'completed'
  } else if (hasUnmetDependency(row, inspectionRecord)) {
    return 'notRequired'
  } else if (row.checkRecord.status === 'overdue') {
    return 'overdue'
  } else {
    // See https://willow.atlassian.net/wiki/spaces/MAR/pages/2318926382/The+due+vs.+overdue+saga
    return 'due'
  }
}

const statusesByPriority: CheckRecord['status'][] = [
  'overdue',
  'due',
  'missed',
  'completed',
  'notRequired',
]

/**
 * Get the summary status for an inspection record based on the statuses of the
 * check records in it. The rules are:
 *
 * - If all the check records are complete or not required, and there is a sync
 *   error, return "syncError".
 * - If all the check records are complete or not required, and there is a sync
 *   pending, return "syncPending"
 * - Otherwise, return the check record status which appears first in the
 *   `statusesByPriority` list above.
 */
export function getInspectionRecordStatus(
  inspectionRecord: PartialInspectionRecordPage
): ExtendedStatus {
  const checkRecordStatuses = inspectionRecord.rows.map((r) =>
    getCheckRecordStatus(r, inspectionRecord)
  )
  if (
    checkRecordStatuses.every((s) => ['complete', 'notRequired'].includes(s))
  ) {
    if (inspectionRecord.rows.some((r) => r.syncStatus === 'error')) {
      return 'syncError'
    } else if (inspectionRecord.rows.some((r) => r.modified)) {
      return 'syncPending'
    }
  }

  return statusesByPriority[
    Math.min(...checkRecordStatuses.map((s) => statusesByPriority.indexOf(s)))
  ]
}

/**
 * Get the next check record that should be expanded. This is the first uncompleted
 * check that does not have an unmet dependency.
 */
function getNextCheckRecord(
  inspectionRecord: PartialInspectionRecordPage
): CheckRow | undefined {
  return inspectionRecord.rows.find(
    (row) =>
      getCheckRecordStatus(row, inspectionRecord) !== 'completed' &&
      !hasUnmetDependency(row, inspectionRecord)
  )
}

/**
 * Return true if the row has a dependency which is not met.
 */
function hasUnmetDependency(
  row: CheckRow,
  inspectionRecord: PartialInspectionRecordPage
) {
  return (
    row.check.dependencyId != null &&
    !inspectionRecord.rows.some(
      (r) =>
        r.check.id === row.check.dependencyId &&
        'stringValue' in r.checkRecord &&
        r.checkRecord.stringValue === row.check.dependencyValue
    )
  )
}

/**
 * Get a SyncPayload for the specified InspectionsState that will save
 * all the modified entries.
 */
export function getSyncPayload(state: InspectionsState): SyncPayload {
  const inspectionRecords: SyncPayload = {}
  const attachmentOperations = getAttachmentOperations(state)
  for (const inspectionRecord of Object.values(state.inspectionRecords)) {
    const modifiedRows = inspectionRecord.rows.filter((r) => r.modified)
    if (modifiedRows.length > 0) {
      inspectionRecords[inspectionRecord.id] = {
        inspectionId: inspectionRecord.inspection.id, // Only because we don't have the right api yet
        siteId: inspectionRecord.siteId,
        checkRecords: Object.fromEntries(
          modifiedRows.map((r) => [
            r.checkRecord.id,
            {
              data: r.checkRecord,
              // If for some reason we didn't get an enteredAt earlier, fill it
              // in now.
              enteredAt: r.enteredAt ?? getTimeStamp(),
              attachmentOperations: attachmentOperations.filter(
                (op) => op.checkRecordId === r.checkRecord.id
              ),
            },
          ])
        ),
      }
    }
  }
  return inspectionRecords
}

/**
 * Get an array of `AttachmentOperation`s representing all the unsaved adds and
 * deletes of attachments.
 */
export function getAttachmentOperations(
  state: InspectionsState
): AttachmentOperation[] {
  const operations: AttachmentOperation[] = []

  for (const inspectionRecord of Object.values(state.inspectionRecords)) {
    for (const row of inspectionRecord.rows) {
      for (const attachment of row.attachments) {
        if (attachment.status === 'added') {
          if (!('file' in attachment.attachment)) {
            throw new Error('An unsaved attachment must have a file')
          }
          operations.push({
            type: 'add',
            checkRecordId: row.checkRecord.id,
            attachmentId: attachment.attachment.id,
            attachment: {
              fileName: attachment.attachment.fileName,
              file: attachment.attachment.file,
            },
          })
        } else if (attachment.status === 'deleted') {
          operations.push({
            type: 'delete',
            checkRecordId: row.checkRecord.id,
            attachmentId: attachment.attachment.id,
          })
        }
      }
    }
  }

  return operations
}

/**
 * Take a SyncResult and apply it to the State. This means resetting the status
 * of successfully-synced entries (checks and attachments), and hard-deleting
 * successfully-deleted attachments.
 */
export function applySyncResult(
  state: InspectionsState,
  syncResult: SyncResult
): InspectionsState {
  return produce(state, (draftState) => {
    for (const [inspectionRecordId, inspectionRecordResult] of Object.entries(
      syncResult.inspectionRecords
    )) {
      for (const [checkRecordId, checkRecordResult] of Object.entries(
        inspectionRecordResult.checkRecords
      )) {
        const existingCheckRecord = draftState.inspectionRecords[
          inspectionRecordId
        ].rows.find((r) => r.checkRecord.id === checkRecordId)
        if (existingCheckRecord != null) {
          if (checkRecordResult.result === 'success') {
            existingCheckRecord.modified = false
            existingCheckRecord.syncStatus = 'success'
          } else if (checkRecordResult.result === 'error') {
            existingCheckRecord.syncStatus = 'error'
          }
          for (const attachmentResult of checkRecordResult.attachments) {
            if (attachmentResult.result.type === 'added') {
              const existingAttachment = existingCheckRecord.attachments.find(
                (a) => a.attachment.id === attachmentResult.attachmentId
              )
              if (existingAttachment != null) {
                existingAttachment.status = 'existing'
                existingAttachment.attachment.id =
                  attachmentResult.result.serverId
                existingAttachment.attachment.url = attachmentResult.result.url
                existingAttachment.attachment.previewUrl =
                  attachmentResult.result.previewUrl
              }
            }
          }
          const deletedIds = new Set(
            checkRecordResult.attachments
              .filter((a) => a.result.type === 'deleted')
              .map((a) => a.attachmentId)
          )
          if (deletedIds.size > 0) {
            existingCheckRecord.attachments =
              existingCheckRecord.attachments.filter(
                (a) => !deletedIds.has(a.attachment.id)
              )
          }
        }
      }
    }

    if (
      !hasUnsyncedRecords(draftState) &&
      (draftState.unsynced.displayMessage || draftState.unsynced.displayMessage)
    ) {
      draftState.unsynced = {
        justSubmittedFirst: false,
        displayMessage: false,
      }
    }

    return draftState
  })
}

/**
 * If we are asked to sync this SyncPayload, can we just do nothing?
 */
export function isSyncPayloadEmpty(syncPayload: SyncPayload): boolean {
  return Object.values(syncPayload).every(({ checkRecords }) =>
    _.isEmpty(checkRecords)
  )
}

function hasUnsyncedRecords(state: InspectionsState) {
  return Object.values(state.inspectionRecords).some((ir) =>
    ir.rows.some((r) => r.modified)
  )
}

const { Provider, useStore: useInspectionRecords } =
  createContext<UseInspectionRecordsStore>()

export { useInspectionRecords }

/**
 * A provider that provides:
 * - Inspection record and check record data for multiple inspections (each
 *   inspection must be loaded via the `loadInspection` function).
 * - The ability to buffer check record updates, including adding and deleting
 *   attachments.
 * - The ability to sync all the buffered updates via the `sync` function.
 *
 * It requires an implementation of `IApi` that needs to make the required
 * server calls to load and save the required data.
 *
 * Currently this provider is not used by any components. It also only buffers
 * the updates in memory, so they will not survive a refresh. We must persist
 * the buffer to IndexedDb before we integrate this with the live components.
 */
export function InspectionRecordsProvider({
  api,
  store,
  children,
}: {
  api: IApi
  store: IStore
  children: ReactNode
}) {
  const createStore = useCallback(() => {
    return create<InspectionsContextType>((set, get) => {
      /**
       * Sync the data to persistent storage (ie. IndexedDb). Try to sync to
       * the server.
       */
      async function sync() {
        const state = get()
        const syncPayload = getSyncPayload(state)
        if (!state.isSyncing && !isSyncPayloadEmpty(syncPayload)) {
          set((s) => beginSync(s, syncPayload))
          try {
            const syncResult = await api.sync(syncPayload)
            set((s) => applySyncResult(s, syncResult))
          } catch (e) {
            if (
              state.unsynced.justSubmittedFirst &&
              !state.unsynced.displayMessage
            ) {
              set((s) => displayWillSyncLater(s, true))
            }
            set((s) => resetLoading(s, syncPayload))
          } finally {
            set((s) => ({ ...s, isSyncing: false }))
          }
          store.save(get())
        }
      }

      /**
       * Load the inspection record page into the state, and remove any other
       * inspection records pages for the same inspection.
       */
      function loadInspectionRecordPage(
        inspectionRecordPage: InspectionRecordPage
      ) {
        const currentInspectionRecordIds = Object.values(
          get().inspectionRecords
        )
          .filter(
            (p) =>
              p.inspection.id === inspectionRecordPage.inspection.id &&
              p.id !== inspectionRecordPage.id
          )
          .map((p) => p.id)
        set((state) =>
          loadInspectionRecord(
            {
              ...state,
              inspectionRecords: _.omit(
                state.inspectionRecords,
                currentInspectionRecordIds
              ),
            },
            inspectionRecordPage
          )
        )
      }

      return {
        ...getInitialState(),
        sync,
        loadInspection: async (siteId: string, inspectionId: string) => {
          const fromApi = await api.getInspectionLastRecord(
            siteId,
            inspectionId
          )
          const fromStore = await store.getInspectionRecordPage(inspectionId)
          if (fromStore?.id === fromApi.id) {
            // If the inspection record ID from the IndexedDb matches the
            // inspection record ID from the call to `getInspectionLastRecord`,
            // then we have the most up-to-date inspection record.
            set((state) => {
              return loadInspectionRecord(state, {
                ...fromStore,
                activeCheckId: null,
              })
            })
          } else {
            const inspectionRecordPage = makeInspectionRecordPage(
              fromApi,
              siteId,
              true
            )
            loadInspectionRecordPage(inspectionRecordPage)
          }
        },
        selectCheck: (inspectionRecordId: string, checkId: string) => {
          set((state) => selectCheck(state, inspectionRecordId, checkId))
        },
        submitCheck: async (
          inspectionRecordId: string,
          checkRecordId: string,
          checkData: CheckRecordInput
        ) => {
          set((state) =>
            submitCheckRecord(
              state,
              inspectionRecordId,
              checkRecordId,
              checkData
            )
          )
          store.save(get())
          sync()
        },
        addAttachment: (
          inspectionRecordId: string,
          checkRecordId: string,
          attachment: UnsavedAttachment
        ) => {
          set((state) =>
            addAttachment(state, inspectionRecordId, checkRecordId, attachment)
          )
          store.save(get())
        },
        deleteAttachment: (
          inspectionRecordId: string,
          checkRecordId: string,
          attachmentId: string | number
        ) => {
          set((state) =>
            deleteAttachment(
              state,
              inspectionRecordId,
              checkRecordId,
              attachmentId
            )
          )
          store.save(get())
        },
        dismissSyncLater: () => {
          set((state) => displayWillSyncLater(state, false))
        },
        getInspectionStatuses: () => store.getInspectionStatuses(),

        /**
         * Get the latest inspection record for the specified site and
         * inspection ID, and replace any current inspection record for that
         * inspection with the new one. Transfer any values for check records
         * in the existing inspection record to the new one.
         *
         * This method deliberately does not fall back to the cache when
         * retrieving the new record, so it will throw an exception if the user
         * is offline.
         */
        refreshInspection: async (siteId: string, inspectionId: string) => {
          const fromApi = await api.getInspectionLastRecord(
            siteId,
            inspectionId,
            { cache: false }
          )
          const inspectionRecordPage = makeInspectionRecordPage(fromApi, siteId)
          const currentInspectionRecord = Object.values(
            get().inspectionRecords
          ).find((p) => p.inspection.id === inspectionId)
          if (currentInspectionRecord != null) {
            if (fromApi.id === currentInspectionRecord.id) {
              throw new Error('Did not receive a different inspection record')
            }

            for (const row of inspectionRecordPage.rows) {
              const existingRow = currentInspectionRecord.rows.find(
                (r) => r.check.id === row.check.id
              )

              if (existingRow != null) {
                if ('numberValue' in existingRow.checkRecord) {
                  // This could also be a TotalCheckRecordInput but it doesn't
                  // really matter, we are only casting so we can create the
                  // new property without Typescript complaining.
                  ;(row.checkRecord as NumericCheckRecordInput).numberValue =
                    existingRow.checkRecord.numberValue
                  row.modified = true
                } else if ('stringValue' in existingRow.checkRecord) {
                  ;(row.checkRecord as ListCheckRecordInput).stringValue =
                    existingRow.checkRecord.stringValue
                  row.modified = true
                } else if ('dateValue' in existingRow.checkRecord) {
                  ;(row.checkRecord as DateCheckRecordInput).dateValue =
                    existingRow.checkRecord.dateValue
                  row.modified = true
                }
                row.checkRecord.notes = existingRow.checkRecord.notes
                row.attachments = existingRow.attachments
              }
            }
          }

          loadInspectionRecordPage(inspectionRecordPage)
          sync()
        },
      }
    })
  }, [api, store])

  return (
    <Provider createStore={createStore}>
      <AutoSyncWrapper>{children}</AutoSyncWrapper>
    </Provider>
  )
}

/**
 * Uses the store from InspectionRecordsProvider and tries to sync every 5
 * seconds. The `sync` function is responsible for not doing anything if
 * there's nothing to sync.
 */
function AutoSyncWrapper({ children }: { children: ReactNode }) {
  const inspectionRecordsContext = useInspectionRecords()

  useEffect(() => {
    const timer = setInterval(() => {
      inspectionRecordsContext.sync()
    }, 5000)

    return () => {
      clearInterval(timer)
    }
  }, [inspectionRecordsContext])

  return <>{children}</>
}
