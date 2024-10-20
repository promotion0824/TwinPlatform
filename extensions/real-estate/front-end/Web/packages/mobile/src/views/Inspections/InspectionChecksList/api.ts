import _ from 'lodash'
import { useApi } from '@willow/mobile-ui'
import {
  AttachmentRequest,
  AttachmentResult,
  CheckRecord,
  IApi,
  SyncInspectionRecordResponse,
  SyncInspectionRecordsResponse,
  SyncPayload,
  SyncResult,
} from './types'
import { isSyncPayloadEmpty } from './InspectionRecordsContext'

export default class InspectionsApi implements IApi {
  api: ReturnType<typeof useApi>

  constructor(api: ReturnType<typeof useApi>) {
    this.api = api
  }

  getInspectionLastRecord(
    siteId: string,
    inspectionId: string,
    options?: { cache?: boolean }
  ) {
    return this.api.get(
      `/api/sites/${siteId}/inspections/${inspectionId}/lastRecord`,
      undefined,
      { cache: options?.cache ?? true }
    )
  }

  saveCheckRecord(
    siteId: string,
    inspectionId: string,
    checkRecordId: string,
    checkRecord: Omit<CheckRecord, 'attachments'>
  ) {
    return this.api.put(
      `/api/sites/${siteId}/inspections/${inspectionId}/lastRecord/checkRecords/${checkRecordId}`,
      checkRecord
    )
  }

  addAttachment(
    siteId: string,
    checkRecordId: string,
    attachment: AttachmentRequest
  ) {
    return this.api.post(
      `/api/sites/${siteId}/checkRecords/${checkRecordId}/attachments`,
      {
        fileName: attachment.fileName,
        attachmentFile: attachment.file,
      },
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    )
  }

  deleteAttachment(
    siteId: string,
    checkRecordId: string,
    attachmentId: string
  ) {
    return this.api.delete(
      `/api/sites/${siteId}/checkRecords/${checkRecordId}/attachments/${attachmentId}`
    )
  }

  /**
   * Save a whole SyncPayload. This makes one request per attachment addition
   * or deletion, and one combined request for all the other check record info.
   * We process all the responses into one SyncResult.
   */
  // eslint-disable-next-line complexity
  async sync(syncPayload: SyncPayload): Promise<SyncResult> {
    if (isSyncPayloadEmpty(syncPayload)) {
      return { inspectionRecords: {} }
    }

    const attachmentResultSets: Array<{
      inspectionRecordId: string
      checkRecordId: string
      results: Array<{
        attachmentId: string | number
        result: AttachmentResult
      }>
    }> = []

    for (const [inspectionRecordId, inspectionRecord] of Object.entries(
      syncPayload
    )) {
      for (const [checkRecordId, checkRecord] of Object.entries(
        inspectionRecord.checkRecords
      )) {
        const attachmentResults: Array<{
          attachmentId: string | number
          result: AttachmentResult
        }> = []
        attachmentResultSets.push({
          inspectionRecordId,
          checkRecordId,
          results: attachmentResults,
        })
        for (const op of checkRecord.attachmentOperations) {
          let attachmentResult: AttachmentResult | undefined
          try {
            if (op.type === 'add') {
              // eslint-disable-next-line no-await-in-loop
              const response = await this.addAttachment(
                inspectionRecord.siteId,
                op.checkRecordId,
                op.attachment
              )
              attachmentResult = {
                type: 'added',
                serverId: response.id,
                previewUrl: response.previewUrl,
                url: response.url,
              }
            } else if (op.type === 'delete') {
              if (typeof op.attachmentId !== 'string') {
                throw new Error(
                  'A delete attachment operation must have a string attachmentId'
                )
              }
              // eslint-disable-next-line no-await-in-loop
              await this.deleteAttachment(
                inspectionRecord.siteId,
                op.checkRecordId,
                op.attachmentId
              )
              attachmentResult = { type: 'deleted' }
            } else {
              attachmentResult = { type: 'error' }
            }
          } catch (e) {
            // eslint-disable-next-line no-console
            console.error(e)
            attachmentResult = { type: 'error' }
          }

          attachmentResults.push({
            attachmentId: op.attachmentId,
            result: attachmentResult,
          })
        }
      }
    }

    // The API works per site so we make one request for each site that exists
    // in the data.
    const sites = _.groupBy(
      Object.entries(syncPayload).map(([id, ir]) => ({
        ...ir,
        inspectionRecordId: id,
      })),
      (ir) => ir.siteId
    )

    const responses = await Promise.all(
      Object.entries(sites).map(
        ([siteId, siteInspectionRecords]) =>
          this.api.post(`/api/sites/${siteId}/syncInspectionRecords`, {
            inspectionRecords: siteInspectionRecords.map(
              (inspectionRecord) => ({
                id: inspectionRecord.inspectionRecordId,
                inspectionId: inspectionRecord.inspectionId,
                checkRecords: Object.entries(inspectionRecord.checkRecords).map(
                  ([checkRecordId, checkRecord]) => ({
                    id: checkRecordId,
                    ...checkRecord.data,
                    enteredAt: checkRecord.enteredAt,
                  })
                ),
              })
            ),
          }) as Promise<SyncInspectionRecordsResponse>
      )
    )

    const responseInspectionRecords: SyncInspectionRecordResponse[] = []
    for (const r of responses) {
      responseInspectionRecords.push(...r.inspectionRecords)
    }

    return {
      inspectionRecords: Object.fromEntries(
        responseInspectionRecords.map((inspectionRecord) => [
          inspectionRecord.id,
          {
            checkRecords: Object.fromEntries(
              inspectionRecord.checkRecords.map((checkRecord) => [
                checkRecord.id,
                {
                  result:
                    checkRecord.result === 'Success'
                      ? ('success' as const)
                      : ('error' as const),
                  attachments:
                    attachmentResultSets.find(
                      (rs) =>
                        rs.inspectionRecordId === inspectionRecord.id &&
                        rs.checkRecordId === checkRecord.id
                    )?.results ?? [],
                },
              ])
            ),
          },
        ])
      ),
    }
  }
}
