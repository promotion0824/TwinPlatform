import { useDateTime, Flex, Form, ValidationError } from '@willow/ui'
import { useTicketStatuses, priorities } from '@willow/common'
import { Status } from '@willow/common/ticketStatus'
import { useQueryClient } from 'react-query'
import _ from 'lodash'
import { useSite } from 'providers'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import Assets from './Assets/Assets'
import RequestorDetails from './RequestorDetails'
import ScheduleSettings from './ScheduleSettings'
import TicketDetails from './TicketDetails/TicketDetails'
import PushScheduledTicketsConfirmation from './PushScheduledTickets/PushScheduledTicketsConfirmation'
import ScheduleModalButton from './ScheduleModalButton'
import { useScheduleModal } from './Hooks/ScheduleModalProvider'
import useCommandAnalytics from '../../../useCommandAnalytics.ts'
import getRecurrenceValidationErrors from './getRecurrenceValidationErrors'

const StyledFlex = styled(Flex)`
  overflow: unset !important;
`
const mediumPriorityId = priorities.find(
  (priority) => priority.name === 'Medium'
).id

export default function ScheduleForm({
  siteId,
  schedule,
  scheduleId,
  isReadOnly,
  onScheduleIdChange,
}) {
  const dateTime = useDateTime()
  const site = useSite()
  const queryClient = useQueryClient()
  const { t } = useTranslation()
  const {
    showPushScheduledTicket,
    setPushScheduledTicket,
    setNewAssets,
    newAssets,
    setSubmittedNewAssets,
    submittedNewAssets,
    isPushScheduledTickets,
    setIsPushScheduledTickets,
    setIsFutureStartDate,
  } = useScheduleModal()
  const commandAnalytics = useCommandAnalytics(site.id)
  const ticketStatuses = useTicketStatuses()

  // eslint-disable-next-line complexity
  function getValidationErrors(form) {
    const errors = []
    if (form.data.recurrence.startDate == null) {
      errors.push({
        name: 'recurrence.startDate',
        message: t('messages.startDateRequired'),
      })
    }

    errors.push(...getRecurrenceValidationErrors(form.data.recurrence, t))

    if (form.data.assets.length === 0) {
      errors.push({ name: 'assets', message: t('messages.assetsRequired') })
    }
    if (form.data.summary === '') {
      errors.push({ name: 'summary', message: t('messages.summaryRequired') })
    }
    if (form.data.description === '') {
      errors.push({
        name: 'description',
        message: t('messages.descriptionRequired'),
      })
    }
    if (form.data.reporterName === '') {
      errors.push({
        name: 'reporterName',
        message: t('messages.requestorRequired'),
      })
    }
    if (form.data.reporterEmail === '') {
      errors.push({
        name: 'reporterEmail',
        message: t('messages.contactEmailRequired'),
      })
    }
    if (form.data.tasks && form.data.tasks.length) {
      const taskErrors = []

      for (let i = 0; i < form.data.tasks.length; i++) {
        const task = form.data.tasks[i]
        if (task.type.toLowerCase() === 'numeric') {
          const currentTaskErrors = []

          if (task.unit === '') {
            currentTaskErrors.push({
              name: 'unit',
              message: t('messages.required'),
            })
          }
          // In this case 0 (zero) should be truthy value
          if (
            task.decimalPlaces === null ||
            task.decimalPlaces === undefined ||
            task.decimalPlaces < 0
          ) {
            currentTaskErrors.push({
              name: 'decimalPlaces',
              message: t('messages.required'),
            })
          }
          if (
            (task.minValue || task.minValue === 0) &&
            (task.maxValue || task.maxValue === 0) &&
            task.minValue > task.maxValue
          ) {
            currentTaskErrors.push({
              name: 'minValue',
              message: t('messages.noMinMoreThanMax'),
            })
          }

          if (currentTaskErrors.length) {
            taskErrors.push({
              index: i,
              errors: currentTaskErrors,
            })
          }
        }
      }

      if (taskErrors.length) {
        errors.push({ name: 'tasks', nestedErrors: taskErrors })
      }
    }
    return errors
  }

  function handleSubmit(form) {
    const validationErrors = getValidationErrors(form)
    if (validationErrors.length > 0) {
      throw new ValidationError(validationErrors)
    }

    const startDate = dateTime(form.data.recurrence?.startDate).format(
      'dateTimeLocal'
    )
    const endDate = dateTime(form.data.recurrence?.endDate).format(
      'dateTimeLocal'
    )
    const days =
      startDate != null
        ? [+startDate.split('-').slice(-1)[0].split('T')[0]]
        : []

    return form.api.ajax(
      form.data.id == null
        ? `/api/sites/${siteId}/tickettemplate`
        : `/api/sites/${siteId}/tickettemplate/${form.data.id}`,
      {
        method: form.data.id == null ? 'post' : 'put',
        body: {
          floorCode: form.data.floorCode,
          priority: form.data.priority,
          statusCode: form.data.statusCode,
          summary: form.data.summary,
          description: form.data.description,
          reporterId: form.data.reporterId,
          reporterName: form.data.reporterName,
          reporterPhone: form.data.reporterPhone,
          reporterEmail: form.data.reporterEmail,
          reporterCompany: form.data.reporterCompany,
          shouldUpdateReporterId:
            form.initialData.reporterId !== form.data.reporterId,
          assigneeId: form.data.assigneeId,
          assigneeType:
            form.data.assigneeId != null
              ? form.data.assigneeType
              : 'noAssignee',
          sourceType: 'platform',
          categoryId: form.data.categoryId,
          recurrence: {
            ...form.data.recurrence,
            startDate,
            endDate,
            maxOccurrences: 0,
            dayOccurrences: [],
            days,
            timeZoneId: site.timeZoneId,
          },
          overdueThreshold: form.data.overdueThreshold,
          assets: form.data.assets.map((asset) => ({
            id: asset.id,
            assetId: asset.id,
            assetName: asset.name,
          })),
          tasks: form.data.tasks,
          // Should create scheduled tickets for new assets added?
          performScheduleHitOnAddedAssets: isPushScheduledTickets,
        },
      }
    )
  }

  function handleSubmitted(form) {
    if (form.data.id == null) {
      onScheduleIdChange(form.response.id)
    } else {
      form.fetchRefresh('schedule')
    }

    setSubmittedNewAssets([...newAssets, ...submittedNewAssets])

    // Reset states
    if (showPushScheduledTicket) {
      setPushScheduledTicket(false)
    }
    setNewAssets([])
    setIsPushScheduledTickets(undefined)

    queryClient.invalidateQueries('schedules')
    form.fetchRefresh('schedules')

    commandAnalytics.trackTicketsSaveSchedule(form.data)
  }

  function hasNewAssets() {
    return newAssets.length > 0
  }

  return (
    <Form
      readOnly={isReadOnly}
      defaultValue={{
        siteId,
        recurrence: {
          startDate: null,
          endDate: null,
          interval: 3,
          occurs: 'monthly',
        },
        overdueThreshold: { units: 1, unitOfMeasure: 'month' },
        floorCode: '',
        assets: [],
        priority: mediumPriorityId,
        statusCode: ticketStatuses.getByStatus(
          _.capitalize(schedule?.status ?? Status.open)
        )?.statusCode,
        assigneeId: null,
        assigneeType: '',
        assigneeName: '',
        summary: '',
        categoryId: null,
        category: '',
        description: '',
        tasks: [],
        reporterId: null,
        reporterName: '',
        reporterPhone: '',
        reporterEmail: '',
        reporterCompany: '',
        ...schedule,
      }}
      onSubmit={handleSubmit}
      onSubmitted={handleSubmitted}
    >
      {(form) => {
        const isFutureStartDate =
          dateTime
            .now()
            .addDays(-1)
            .differenceInDays(
              dateTime(form.data.recurrence?.startDate).format('dateTimeLocal')
            ) < 0
        return (
          <>
            {showPushScheduledTicket ? (
              <PushScheduledTicketsConfirmation
                newAssets={newAssets}
                onClose={() => setPushScheduledTicket(false)}
              />
            ) : (
              <Flex fill="header">
                <StyledFlex>
                  <ScheduleSettings />
                  <Assets isReadOnly={isReadOnly} />
                  <TicketDetails
                    scheduleId={scheduleId}
                    isReadOnly={isReadOnly}
                  />
                  <RequestorDetails />
                </StyledFlex>

                {!isReadOnly && (
                  <ScheduleModalButton
                    onClick={() => {
                      setPushScheduledTicket(true)
                      setIsFutureStartDate(isFutureStartDate)
                    }}
                    isSubmit={!hasNewAssets() || form.data.id === undefined}
                  >
                    {form.data.id != null
                      ? `${t('plainText.updateSchedule')} ${
                          hasNewAssets() ? `(${newAssets.length})` : ''
                        }`
                      : t('plainText.scheduleTicket')}
                  </ScheduleModalButton>
                )}
              </Flex>
            )}
          </>
        )
      }}
    </Form>
  )
}
