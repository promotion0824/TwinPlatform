/**
 * Note that the types in this file match the responses from MobileXL endpoints
 * such as `/{siteId}/inspections/{inspectionId}/lastRecord`. There are endpoints
 * in PlatformPortalXL which refer to equivalent objects but in different forms,
 * for example different capitalisation in enumeration values.
 */
export type Inspection = {
  id: string
  name: string
  zoneId: string
  assetId: string
  assetName?: string
  floorCode: string
  assignedWorkgroupId: string
  sortOrder: number
  frequencyInHours: number
  startDate: string
  endDate: string
  nextEffectiveDate: string
  checks: Check[]
}

/**
 * An inspection record returned by MobileXL.
 */
export type InspectionRecord = {
  id: string
  inspectionId: string
  inspection: Inspection
  checkRecords: CheckRecord[]
  effectiveAt?: string
  expiresAt?: string
}

/**
 * The properties that exist on all check types returned by MobileXL.
 */
type BaseCheck = {
  id: string
  inspectionId: string
  sortOrder: number
  name: string

  dependencyId?: string
  dependencyValue?: string
}

/**
 * A numeric check returned by MobileXL.
 */
type NumericCheck = BaseCheck & {
  type: 'numeric'
  typeValue: string
  decimalPlaces: number
  minValue?: number
  maxValue?: number
  lastSubmittedRecord?: NumericCheckRecord
}

/**
 * A total check returned by MobileXL.
 */
type TotalCheck = BaseCheck & {
  type: 'total'
  typeValue: string
  decimalPlaces: number
  lastSubmittedRecord?: TotalCheckRecord
  minValue?: number
  maxValue?: number
  multiplier?: number
}

/**
 * A list check returned by MobileXL.
 */
type ListCheck = BaseCheck & {
  type: 'list'
  /**
   * a "|"-delimited list of available values
   */
  typeValue: string
  lastSubmittedRecord?: ListCheckRecord
}

/**
 * A date check returned by MobileXL.
 */
type DateCheck = BaseCheck & {
  type: 'date'
  typeValue: string // seems to be just empty string maybe?
  lastSubmittedRecord?: DateCheckRecord
}

/**
 * A union of all the complete check types returned by MobileXL
 */
export type Check = NumericCheck | TotalCheck | ListCheck | DateCheck

/**
 * This is a util type. See here:
 * https://www.typescriptlang.org/docs/handbook/2/conditional-types.html#distributive-conditional-types
 *
 * Basically this makes `DistributiveOmit<Check, 'inspectionId'>` do what we would naively
 * expect `Omit<Check, 'inspectionId'>` to do. But the latter removes attributes that are
 * only in some of the elements of the union type.
 *
 * See also https://stackoverflow.com/a/57103940
 */
type DistributiveOmit<T, K extends PropertyKey> = T extends unknown
  ? Omit<T, K>
  : never

/**
 * A check but without the `inspectionId` attribute. This is for test utils,
 * and only exists because `Omit` does not do what we want it to do to union types,
 * {@see DistributiveOmit}.
 */
export type CheckWithoutInspectionId = DistributiveOmit<Check, 'inspectionId'>

/**
 * The base attributes for a check record returned by MobileXL. This does not
 * include the attributes that the user can edit - they are combined
 * separately.
 */
export type CheckWithoutInspectionIds = DistributiveOmit<Check, 'inspectionId'>

type CheckRecordBase = {
  id: string
  inspectionId: string
  checkId: string
  inspectionRecordId: string
  status: 'due' | 'overdue' | 'completed' | 'missed' | 'notRequired'
  submittedUserId?: string
  submittedDate?: string
  submittedSiteLocalDate?: string
  effectiveDate: string
  attachments: Array<ServerAttachment | UnsavedAttachment>
  enteredBy?: {
    id: string
    firstName?: string
    lastName?: string
    avatarId?: string
    createdDate?: string
    initials?: string
    auth0UserId?: string
    email?: string
    mobile?: string
    company?: string
    customerId?: string
    status?: 'active' | 'inactive' | 'pending' | 'deleted'
  }
}

export type ExtendedStatus =
  | CheckRecordBase['status']
  | 'syncPending'
  | 'syncError'

/**
 * User-editable attributes common to all check types.
 */
type CheckRecordInputBase = {
  notes?: string
}

/**
 * The user-editable attributes for a numeric check record
 */
export type NumericCheckRecordInput = CheckRecordInputBase & {
  numberValue?: number
}

/**
 * The user-editable attributes for a total check record
 */
export type TotalCheckRecordInput = CheckRecordInputBase & {
  numberValue?: number
  minValue?: string
  maxValue?: string
  multiplier?: number
  calculated?: number
}

/**
 * The user-editable attributes for a list check record
 */
export type ListCheckRecordInput = CheckRecordInputBase & {
  stringValue?: string
}

/**
 * The user-editable attributes for a date check record
 */
export type DateCheckRecordInput = CheckRecordInputBase & { dateValue?: string }

/**
 * A union of all the check record input types
 */
export type CheckRecordInput =
  | NumericCheckRecordInput
  | TotalCheckRecordInput
  | ListCheckRecordInput
  | DateCheckRecordInput

/**
 * All the attributes for a numeric check record (user-editable and not)
 */
type NumericCheckRecord = CheckRecordBase & NumericCheckRecordInput

/**
 * All the attributes for a total check record (user-editable and not)
 */
type TotalCheckRecord = CheckRecordBase & TotalCheckRecordInput

/**
 * All the attributes for a list check record (user-editable and not)
 */
type ListCheckRecord = CheckRecordBase & ListCheckRecordInput

/**
 * All the attributes for a date check record (user-editable and not)
 */
type DateCheckRecord = CheckRecordBase & DateCheckRecordInput

/**
 * A union of all the user-editable and non-user-editable attributes
 * for all the check types
 */
export type CheckRecord =
  | NumericCheckRecord
  | TotalCheckRecord
  | ListCheckRecord
  | DateCheckRecord

/**
 * An attachment that has not been saved to the server yet. Will be replaced by
 * a ServerAttachment when successfully saved.
 */
export type UnsavedAttachment = {
  id: number
  previewUrl: string
  url: string
  fileName: string
  file: File
  base64: string
}

/**
 * An attachment that has been retrieved from the server, either from an
 * attachments endpoint or from an inspection record endpoint.
 */
export type ServerAttachment = {
  id: string
  type: 'image' | 'file'
  fileName: string
  createdDate: string
  previewUrl: string
  url: string
}

/**
 * The data that is sent to the attachments endpoint when we save a new
 * attachment
 */
export type AttachmentRequest = {
  fileName: string
  file: File
}

/**
 * The complete state of the inspections page.
 */
export type InspectionsState = {
  inspectionRecords: {
    [inspectionRecordId: string]: InspectionRecordPage
  }
  inspectionRecordId?: string
  /**
   * Is a sync to the server currently in progress?
   */
  isSyncing: boolean
  unsynced: {
    /**
     * Was the most recent submitted record submitted when there weren't any
     * unsynced records yet?
     */
    justSubmittedFirst: boolean

    /**
     * Should we display the "There are unsynced records" message to the user?
     */
    displayMessage: boolean
  }
}

/**
 * The context data returned by the InspectionsProvider.
 */
export type InspectionsContextType = InspectionsState & {
  loadInspection: (siteId: string, inspectionId: string) => Promise<void>

  submitCheck: (
    inspectionRecordId: string,
    checkRecordId: string,
    checkData: CheckRecordInput
  ) => void

  addAttachment: (
    inspectionRecordId: string,
    checkRecordId: string,
    attachment: UnsavedAttachment
  ) => void

  deleteAttachment: (
    inspectionRecordId: string,
    checkRecordId: string,
    attachmentId: number | string
  ) => void

  selectCheck: (inspectionRecordId: string, checkId: string | null) => void

  sync: () => void

  dismissSyncLater: () => void

  getInspectionStatuses: () => Promise<{
    [inspectionId: string]: ExtendedStatus
  }>

  refreshInspection: (siteId: string, inspectionId: string) => Promise<void>
}

/**
 * The data for an inspection record.
 */
export type InspectionRecordPage = {
  /**
   * The inspection record ID
   */
  id: string
  siteId: string
  inspection: Inspection

  /**
   * Time in UTC when this inspection record was created. Sometimes we refer to
   * this as when the inspection is "due". This is now always returned by the
   * backend, but users may not have a value for this field for existing
   * records in their local DB.
   */
  effectiveAt?: string

  /**
   * Time in UTC when this inspection record will expire and be replaced by a
   * new one. This is now always returned by the backend, but users may not
   * have a value for this field for existing records in their local DB.
   */
  expiresAt?: string

  rows: CheckRow[]

  /**
   * Which check ID is currently open, if any? If the user clicks the active
   * check, it will close and there won't be any checks open.
   */
  activeCheckId: string | null

  /**
   * Are all the required checks complete?
   */
  isComplete: boolean
}

/**
 * An `InspectionRecordPage` that may or may not have an activeCheckId and/or
 * isComplete. Used so we can pass an almost-complete InspectionRecordPage to a
 * function that fills in those attributes.
 */
export type PartialInspectionRecordPage = Omit<
  InspectionRecordPage,
  'activeCheckId' | 'isComplete'
> &
  Partial<Pick<InspectionRecordPage, 'activeCheckId' | 'isComplete'>>

/**
 * Since there may be a delay between the time the user entered the check data
 * and when the check data was synced, we send the entry time when we sync. We
 * send this as a string in the form "YYYY-MM-DDTHH:mm:ss", in UTC.
 */
type EnteredAt = string

/**
 * The data for a check on an inspections page
 */
export type CheckRow = {
  check: Check
  checkRecord: CheckRecord
  attachments: AttachmentEntry[]
  modified: boolean
  syncStatus: null | 'loading' | 'success' | 'error'
  enteredAt?: EnteredAt
}

/**
 * An attachment. May have already existed on the server or created in this
 * session. May also have been deleted during this session but the deletion may
 * not have been persisted. In this case the `status` is set to "deleted". The
 * UI is responsible for not showing `AttachmentEntry`s with this status. On a
 * successful sync, an attachment with a status of "deleted" will finally be
 * deleted from the array.
 */
export type AttachmentEntry = {
  attachment: UnsavedAttachment | ServerAttachment
  status: 'existing' | 'added' | 'deleted'
}

/**
 * A collection of check record updates that can be saved in one batch. Does
 * not include attachment updates.
 */
export type SyncPayload = {
  [inspectionRecordId: string]: {
    inspectionId: string
    siteId: string
    checkRecords: {
      [checkRecordId: string]: {
        data: CheckRecordInput
        enteredAt: EnteredAt
        attachmentOperations: AttachmentOperation[]
      }
    }
  }
}

export type AttachmentResult =
  | {
      type: 'added'
      /**
       * When the UI creates an attachment, it is assigned an arbitrary numeric
       * ID. When the attachment is saved to the server, the server sends back
       * the real server ID of the attachment. We save that back to our attachment
       * entry so we can use it if we later want to delete the attachment.
       */
      serverId: string

      /**
       * When we save an attachment, we also update the reference to its URL
       */
      url: string

      /**
       * When we save an attachment, we also update the reference to its preview URL
       */
      previewUrl: string
    }
  | { type: 'deleted' }
  | { type: 'error' }

/**
 * The response from the /sites/{siteId}/syncInspectionRecords endpoint
 */
export type SyncInspectionRecordsResponse = {
  inspectionRecords: SyncInspectionRecordResponse[]
}

/**
 * The shape of a particular inspection record returned by the
 * /sites/{siteId}/syncInspectionRecords endpoint
 */
export type SyncInspectionRecordResponse = {
  id: string
  checkRecords: Array<{
    id: string
    result: 'Success' | 'Error'
    message?: string
  }>
}

/**
 * The result of a sync. Includes check records and attachment updates which can
 * succeed or fail independently.
 */
export type SyncResult = {
  inspectionRecords: {
    [inspectionRecordId: string]: {
      checkRecords: {
        [checkRecordId: string]: {
          result: 'success' | 'error'
          attachments: Array<{
            attachmentId: string | number
            result: AttachmentResult
          }>
        }
      }
    }
  }
}

export type AttachmentOperation =
  | {
      type: 'add'
      checkRecordId: string
      attachmentId: number
      attachment: AttachmentRequest
    }
  | {
      type: 'delete'
      checkRecordId: string
      attachmentId: number | string
    }

/**
 * The inspections provider does not itself know how to retrieve or save
 * inspections to the server - it delegates to an API object that must follow
 * this interface.
 */
export interface IApi {
  getInspectionLastRecord(
    siteId: string,
    inspectionId: string,
    options?: { cache?: boolean }
  ): Promise<InspectionRecord>

  addAttachment(
    siteId: string,
    checkRecordId: string,
    attachment: AttachmentRequest
  ): Promise<ServerAttachment>

  deleteAttachment(
    siteId: string,
    checkRecordId: string,
    attachmentId: string
  ): Promise<void>

  saveCheckRecord(
    siteId: string,
    inspectionId: string,
    checkRecordId: string,
    checkRecord: CheckRecord
  ): Promise<void>

  sync: (payload: SyncPayload) => Promise<SyncResult>
}

export interface IStore {
  save: (state: InspectionsState) => Promise<void>
  getInspectionStatuses: () => Promise<{
    [inspectionId: string]: ExtendedStatus
  }>
  getInspectionRecordPage: (
    inspectionId: string
  ) => Promise<InspectionRecordPage | undefined>
}
