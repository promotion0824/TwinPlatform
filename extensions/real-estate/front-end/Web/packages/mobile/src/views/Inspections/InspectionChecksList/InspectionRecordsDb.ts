/* eslint-disable import/prefer-default-export, max-classes-per-file, no-underscore-dangle */
import produce from 'immer'
import _ from 'lodash'
import {
  getInitialState,
  getInspectionRecordStatus,
} from './InspectionRecordsContext'
import {
  IStore,
  InspectionRecordPage,
  InspectionsState,
  UnsavedAttachment,
  CheckRow,
  AttachmentEntry,
  ServerAttachment,
} from './types'

/**
 * While storing a File object in IndexedDB works fine, it does not work
 * properly with fake-indexeddb. So we write the file's ArrayBuffer instead,
 * and convert it back to a File when we load. This type represents the object
 * we put into the database. "Unsaved" means we have not synced to the server,
 * it doesn't mean we haven't saved to the database.
 */
type DbUnsavedAttachment = Omit<
  UnsavedAttachment & { arrayBuffer: ArrayBuffer },
  'file'
>

/**
 * An InspectionRecordPage but with the `UnsavedAttachment`s replaced with
 * `DbUnsavedAttachment`s.
 */
type DbInspectionRecordPage = Omit<InspectionRecordPage, 'rows'> & {
  rows: Array<
    Omit<CheckRow, 'attachments'> & {
      attachments: Array<
        Omit<AttachmentEntry, 'attachment'> & {
          attachment: DbUnsavedAttachment | ServerAttachment
        }
      >
    }
  >
}

async function readFileAsDataURL(file: File): Promise<string> {
  return new Promise((resolve) => {
    const reader = new FileReader()
    reader.onload = (e) => {
      resolve(reader.result as string)
    }
    reader.readAsDataURL(file)
  })
}

export class IndexedDbStore implements IStore {
  db: IDBDatabase

  state: InspectionsState

  static readonly STORE_NAME = 'inspectionRecords'

  static async create(dbName: string): Promise<IndexedDbStore> {
    return new Promise<IndexedDbStore>((resolve, reject) => {
      const request = indexedDB.open(dbName)

      request.onerror = (event) => {
        reject()
      }

      request.onsuccess = (event) => {
        const db = request.result
        resolve(new IndexedDbStore(db))
      }

      request.onupgradeneeded = (event) => {
        request.result.createObjectStore(IndexedDbStore.STORE_NAME, {
          keyPath: 'id',
        })
      }
    })
  }

  constructor(db: IDBDatabase) {
    this.db = db
    this.state = getInitialState()
  }

  /**
   * Save the inspections state to the IndexedDb. Currently this is a little
   * wasteful. For each inspection record that has been modified since the last
   * save, it will write the whole inspection record, including all the
   * attachments. If this turns out to be slow on mobile devices we may want to
   * store the attachments in a separate object store.
   */
  async save(state: InspectionsState) {
    // While storing a File object in IndexedDB works fine, it does not work
    // properly with fake-indexeddb. So we write the file's ArrayBuffer instead,
    // and convert it back to a File when we load.
    // We make sure to get all the required ArrayBuffers before starting the
    // IndexedDb transaction, since getting the ArrayBuffer is asynchronous,
    // and if we do asynchronous stuff in the middle of a transaction the
    // transaction gets upset.
    const attachments = {}
    for (const inspectionRecord of Object.values(state.inspectionRecords)) {
      if (
        inspectionRecord !== this.state.inspectionRecords[inspectionRecord.id]
      ) {
        for (const r of inspectionRecord.rows) {
          for (const a of r.attachments) {
            if ('file' in a.attachment) {
              const dbAttachment: DbUnsavedAttachment = {
                ..._.omit(a.attachment, 'file'),
                // eslint-disable-next-line no-await-in-loop
                arrayBuffer: await a.attachment.file.arrayBuffer(),
              }
              attachments[a.attachment.id] = dbAttachment
            }
          }
        }
      }
    }

    return new Promise<void>((resolve, reject) => {
      const transaction = this.db.transaction(
        IndexedDbStore.STORE_NAME,
        'readwrite'
      )
      transaction.oncomplete = () => resolve()
      transaction.onabort = () => reject()
      transaction.onerror = () => reject()
      const objectStore = transaction.objectStore(IndexedDbStore.STORE_NAME)
      for (const inspectionRecord of Object.values(state.inspectionRecords)) {
        if (
          inspectionRecord !== this.state.inspectionRecords[inspectionRecord.id]
        ) {
          const dbInspectionRecordPage: DbInspectionRecordPage = {
            ...inspectionRecord,
            rows: inspectionRecord.rows.map((row) => ({
              ...row,
              attachments: row.attachments.map((a) => ({
                ...a,
                attachment: attachments[a.attachment.id] ?? a.attachment,
              })),
            })),
          }
          objectStore.put(dbInspectionRecordPage)
        }
      }
      for (const deletedId of _.difference(
        Object.values(this.state.inspectionRecords).map((ir) => ir.id),
        Object.values(state.inspectionRecords).map((ir) => ir.id)
      )) {
        objectStore.delete(deletedId)
      }
      this.state = state
    })
  }

  _getAllInspectionRecordPages(): Promise<DbInspectionRecordPage[]> {
    return new Promise((resolve, reject) => {
      const transaction = this.db.transaction(
        IndexedDbStore.STORE_NAME,
        'readwrite'
      )
      const objectStore = transaction.objectStore(IndexedDbStore.STORE_NAME)
      const request = objectStore.getAll()
      request.onsuccess = async () => {
        resolve(request.result as DbInspectionRecordPage[])
      }
      request.onerror = () => reject()
    })
  }

  async getInspectionStatuses() {
    const inspectionRecordPages = await this._getAllInspectionRecordPages()

    // We don't care about the attachments for getting the statuses, but we
    // need them to be there for the type checker. This is much less painful
    // than changing the underlying types to make attachments sometimes
    // optional.
    const withEmptyAttachments: InspectionRecordPage[] =
      inspectionRecordPages.map((p) => ({
        ...p,
        rows: p.rows.map((r) => ({ ...r, attachments: [] })),
      }))

    return Object.fromEntries(
      withEmptyAttachments.map((p) => [
        p.inspection.id,
        getInspectionRecordStatus(p),
      ])
    )
  }

  async getInspectionRecordPage(inspectionId: string) {
    const inspectionRecordPages = await this._getAllInspectionRecordPages()
    const inspectionRecordPage = inspectionRecordPages.find(
      (r) => r.inspection.id === inspectionId
    )
    if (inspectionRecordPage != null) {
      const rows: CheckRow[] = []
      for (const row of inspectionRecordPage.rows) {
        const attachments: AttachmentEntry[] = []
        for (const attachmentEntry of row.attachments) {
          const { attachment } = attachmentEntry
          // Unsaved attachments are the ones that still have numeric IDs.
          if (typeof attachment.id === 'number') {
            const dbUnsavedAttachment = attachment as DbUnsavedAttachment
            const file = new File(
              [dbUnsavedAttachment.arrayBuffer],
              attachment.fileName
            )
            // eslint-disable-next-line no-await-in-loop
            const dataUrl = await readFileAsDataURL(file)
            const newAttachment: UnsavedAttachment = {
              id: attachment.id,
              url: dataUrl,
              previewUrl: dataUrl,
              fileName: attachment.fileName,
              file,
              base64: dbUnsavedAttachment.base64,
            }
            attachments.push({ ...attachmentEntry, attachment: newAttachment })
          } else {
            attachments.push({
              ...attachmentEntry,
              attachment: attachmentEntry.attachment as ServerAttachment,
            })
          }
        }
        rows.push({ ...row, attachments })
      }
      return { ...inspectionRecordPage, rows }
    }
    return inspectionRecordPage
  }
}

/**
 * Implementation of IStore that does nothing - can be used in testing.
 */
export class DummyIndexedDbStore implements IStore {
  // eslint-disable-next-line class-methods-use-this
  save(state: InspectionsState) {
    return Promise.resolve()
  }

  // eslint-disable-next-line class-methods-use-this
  getInspectionStatuses() {
    return Promise.resolve({})
  }

  // eslint-disable-next-line class-methods-use-this
  getInspectionRecordPage(inspectionId: string) {
    return Promise.resolve(undefined)
  }
}
