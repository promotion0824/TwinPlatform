import _ from 'lodash'
import { useHistory } from 'react-router'
import { DateTime } from 'luxon'
import { useCallback, useEffect, useRef, useState } from 'react'
import { useQuery } from 'react-query'
import {
  useApi,
  Button,
  NotFound,
  Error as ErrorMessage,
  Icon,
  Loader,
  useAnalytics,
  useDateTime,
  useSnackbar,
} from '@willow/mobile-ui'
import { styled } from 'twin.macro'
import { useLayout } from '../../../providers'
import CheckRecordItem from './CheckRecordItem'
import { FormValue } from './CheckRecordForm'
import { CheckRecord } from './types'
import {
  getCheckRecordStatus,
  useInspectionRecords,
} from './InspectionRecordsContext'

/**
 * Display all the check records in an inspection record. The user can
 * navigate between them, enter data and submit them individually.
 */
export default function InspectionChecksList({
  siteId,
  inspectionId,
  inspectionZoneId,
}: {
  siteId: string
  inspectionId: string
  inspectionZoneId: string
}) {
  const api = useApi()
  const history = useHistory()
  const analytics = useAnalytics()
  const { setTitle, setShowBackButton } = useLayout()
  const inspectionRecordsContext = useInspectionRecords()
  const wasComplete = useRef<boolean | undefined>()
  const snackbar = useSnackbar()
  const dateTime = useDateTime()

  const inspectionQuery = useQuery(
    ['inspection', inspectionId],
    async () => {
      try {
        return await inspectionRecordsContext.loadInspection(
          siteId,
          inspectionId
        )
      } catch (e) {
        console.error(e)
        analytics.track('unhandledException', {
          context: 'InspectionChecksList inspectionQuery',
          message: e.message,
          stack: e.stack,
        })
        throw e
      }
    },
    // We already have a lot of caches, we don't need another one for React Query.
    { cacheTime: 0 }
  )

  const inspectionRecord = Object.values(
    inspectionRecordsContext.inspectionRecords
  ).find((p) => p.inspection.id === inspectionId)

  useEffect(() => {
    const assetName = inspectionRecord?.inspection?.assetName
    const name = inspectionRecord?.inspection?.name

    if (name) {
      setTitle(`${name}${assetName ? ` - ${assetName}` : ''}`)
    }
  }, [
    inspectionRecord?.inspection?.assetName,
    inspectionRecord?.inspection?.name,
    setTitle,
  ])

  /**
   * On submitting, make a sync call with the single check record we updated
   * (including attachment updates if any). On completion, reload the
   * inspection record to get the updated statuses. There are no more
   * incomplete check records, redirect to the inspections list for the current
   * zone.
   */
  const handleSubmit = useCallback(
    async (checkRecord: CheckRecord, data: FormValue) => {
      if (inspectionRecord == null) {
        throw new Error('Somehow got to submit without an inspectionRecord')
      }

      for (const a of data.attachmentEntries) {
        if (
          a.status === 'added' &&
          typeof a.attachment.id === 'number' &&
          'file' in a.attachment
        ) {
          inspectionRecordsContext.addAttachment(
            inspectionRecord.id,
            checkRecord.id,
            a.attachment
          )
        } else if (a.status === 'deleted') {
          inspectionRecordsContext.deleteAttachment(
            inspectionRecord.id,
            checkRecord.id,
            a.attachment.id
          )
        }
      }
      inspectionRecordsContext.submitCheck(
        inspectionRecord.id,
        checkRecord.id,
        data.checkRecord
      )
    },
    [api, inspectionRecord]
  )

  useEffect(() => {
    if (wasComplete.current === false && inspectionRecord?.isComplete) {
      history.push(
        `/sites/${siteId}/inspectionZones/${inspectionZoneId}/inspections`
      )
    }
    wasComplete.current = inspectionRecord?.isComplete
  }, [history, inspectionRecord?.isComplete, inspectionZoneId, siteId])

  setShowBackButton(
    true,
    `/sites/${siteId}/inspectionZones/${inspectionZoneId}/inspections`
  )

  if (inspectionQuery.status === 'loading') {
    return <Loader />
  } else if (inspectionQuery.status === 'error') {
    return <ErrorMessage />
  } else if (inspectionRecord != null) {
    return (
      <>
        <EffectiveFrom>
          <div tw="flex-1">
            {'This inspection is for '}
            <strong>
              {dateTime(inspectionRecord.effectiveAt).format('dateTimeLong')}
            </strong>
          </div>
        </EffectiveFrom>
        <MaybeExpired
          expiresAt={inspectionRecord.expiresAt}
          onRefresh={async () => {
            try {
              await inspectionRecordsContext.refreshInspection(
                siteId,
                inspectionId
              )
            } catch (err) {
              console.error(err)
              snackbar.show(
                'There was a problem retrieving the latest inspection record. ' +
                  'You must be online to do this.'
              )
            }
          }}
        />
        {inspectionRecord.rows.map((row, index) => {
          const { check, checkRecord, attachments } = row
          const status = getCheckRecordStatus(row, inspectionRecord)

          return (
            <div key={check.id} data-index={index}>
              <CheckRecordItem
                siteId={siteId}
                check={check}
                checkRecord={{ ...checkRecord, status }}
                isExpanded={check.id === inspectionRecord.activeCheckId}
                modified={row.modified}
                syncStatus={row.syncStatus}
                dependentCheck={
                  check.dependencyId != null
                    ? inspectionRecord.rows.find(
                        (r) => r.check.id === check.dependencyId
                      )?.check
                    : undefined
                }
                attachmentEntries={attachments}
                // It is not necessary to disable editing attachments for
                // completed records - we just have this for now so there's one
                // fewer moving part. In future we should remove this flag and
                // always allow editing attachments.
                isAttachmentEnabled={status !== 'completed'}
                onToggle={(checkId) => {
                  inspectionRecordsContext.selectCheck(
                    inspectionRecord.id,
                    checkId === inspectionRecord.activeCheckId ? null : checkId
                  )
                }}
                onSubmit={(data) => handleSubmit(checkRecord, data)}
              />
            </div>
          )
        })}
        {inspectionRecord.rows.length === 0 && (
          <NotFound>No inspection checks found</NotFound>
        )}
      </>
    )
  } else {
    return null
  }
}

const isInPast = (dateTime: DateTime | undefined) =>
  dateTime != null && dateTime < DateTime.local()

/**
 * If the inspection is expired, display a message to that effect and a Refresh
 * button. If the inspection is not expired, display nothing, but periodically
 * check the time to see if it has become expired.
 */
function MaybeExpired({
  expiresAt,
  onRefresh,
}: {
  expiresAt?: string
  onRefresh: () => void
}) {
  const expiresDateTime =
    expiresAt != null ? DateTime.fromISO(expiresAt) : undefined
  const [isExpired, setExpired] = useState(() => isInPast(expiresDateTime))

  useEffect(() => {
    const expired = isInPast(expiresDateTime)
    setExpired(expired)

    if (!expired) {
      const timer = setInterval(() => {
        if (isInPast(expiresDateTime)) {
          setExpired(true)
          clearInterval(timer)
        }
      }, 5000)

      return () => clearInterval(timer)
    }

    return undefined
  }, [expiresAt])

  if (isExpired) {
    return (
      <EffectiveFrom>
        <div tw="flex gap-1 flex-1">
          <div tw="flex-initial">
            <WarningIcon icon="error" />
          </div>
          <div tw="flex-1">
            <div>
              This inspection has expired. Click Refresh to load the current
              inspection.
            </div>
            <div tw="mt-1">
              <Button onClick={onRefresh} color="blue">
                Refresh
              </Button>
            </div>
          </div>
        </div>
      </EffectiveFrom>
    )
  } else {
    return null
  }
}

const WarningIcon = styled(Icon)({
  fill: 'orange',
})

const EffectiveFrom = styled.div({
  borderBottom: '1px solid var(--theme-color-neutral-border-default)',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
  flexDirection: 'column',
  padding: '1em 0',
})
