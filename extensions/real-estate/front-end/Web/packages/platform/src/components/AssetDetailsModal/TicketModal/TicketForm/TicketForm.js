/* eslint-disable complexity */
import { titleCase, useTicketStatuses } from '@willow/common'
import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import {
  isTicketStatusEquates,
  isTicketStatusIncludes,
  Status,
} from '@willow/common/ticketStatus'
import {
  getDescriptionText,
  getFailedDiagnostics,
} from '@willow/common/utils/ticketUtils'
import {
  api,
  Flex,
  Form,
  Link,
  ModalSubmitButton,
  useAnalytics,
  useConfig,
  useFeatureFlag,
  useScopeSelector,
  useUser,
  ValidationError,
} from '@willow/ui'
import { Button, Loader, useSnackbar } from '@willowinc/ui'
import _ from 'lodash'
import { useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useQueryClient } from 'react-query'
import { useHistory } from 'react-router-dom'
import { css, styled } from 'twin.macro'
import { v4 as uuidv4 } from 'uuid'
import {
  useGetInsightDiagnostics,
  useGetTicketCategories,
  useGetTicketSubStatus,
} from '../../../../hooks'
import { InsightActions } from '../../../Insights/ui/ActionsViewControl'
import Asset from './Asset'

import Attachments from './Attachments'
import Comments from './Comments/Comments'
import CreatorDetails from './CreatorDetails'
import Dates from './Dates'
import Header from './Header'
import Insight from './Insight'
import LinkedInsights from './LinkedInsights'
import RequestorDetails from './RequestorDetails'
import SolutionDetails from './SolutionDetails'
import SourceDetails from './SourceDetails'
import TicketDetails from './TicketDetails/TicketDetails'
import TicketSettings from './TicketSettings'

export default function TicketForm({
  ticket,
  onTicketChange,
  dataSegmentPropPage = 'Tickets Page',
  onClose,
  isTicketUpdated,
  selectedInsight,
}) {
  // track if the form has been submitted
  // this flag is used to determine if the mapped required fields are null after form submission
  const [submitted, setSubmitted] = useState(false)
  const config = useConfig()
  const history = useHistory()
  const prevLocationRef = useRef(history.location)
  const featureFlags = useFeatureFlag()
  const snackbar = useSnackbar()
  const user = useUser()
  const isPolling =
    featureFlags.hasFeatureToggle('ticketSync') &&
    user?.customer?.name === 'Walmart' &&
    featureFlags.hasFeatureToggle('mappedEnabled')

  const handlePollingAndSubmit = (form) => {
    const pollInterval = 60 * 1000 // Poll every 1 minute
    const maxTime = 6 * 60 * 1000 // Finish Polling after 6 minutes
    const startTime = Date.now()
    let timeoutId = null

    const poll = async () => {
      try {
        const { data } = await api.get(`/tickets/${form.response.id}`)

        if (
          (data.sourceType === 'platform' && !!data.externalId) ||
          data.sourceType !== 'platform'
        ) {
          snackbar.update({
            id: form.response.id,
            title: titleCase({ text: t('plainText.syncComplete'), language }),
            intent: 'positive',
            description: form.response.summary,
            loading: false,
            withCloseButton: true,
          })
        } else if (Date.now() - startTime < maxTime) {
          if (prevLocationRef.current.pathname !== history.location.pathname) {
            clearTimeout(timeoutId)
            snackbar.hide(form.response.id)
            return
          }
          timeoutId = setTimeout(poll, pollInterval) // Continue polling
        } else {
          // TODO: PUI Component needs to be updated to see the error message
          // Link : https://dev.azure.com/willowdev/Unified/_workitems/edit/137015

          snackbar.update({
            id: form.response.id,
            title: titleCase({ text: t('plainText.syncFailed'), language }),
            intent: 'negative',
            loading: false,
            withCloseButton: true,
          })
        }
      } catch (error) {
        snackbar.show({
          title: t('plainText.errorOccurred'),
          intent: 'negative',
        })
      }
    }

    poll()

    return () => {
      clearTimeout(timeoutId)
    }
  }

  const [{ action, ticketId }, setSearchParams] = useMultipleSearchParams([
    'action',
    'ticketId',
  ])

  const analytics = useAnalytics()
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const queryClient = useQueryClient()
  const insightId = ticket.insightId || selectedInsight?.id
  const insightDiagnosticQuery = useGetInsightDiagnostics(insightId, {
    enabled: insightId != null,
  })
  const isCategoryEnabled = featureFlags.hasFeatureToggle('mappedEnabled')
  const {
    isScopeSelectorEnabled,
    flattenedLocationList,
    isScopeUsedAsBuilding,
  } = useScopeSelector()
  const sitesTwins = isScopeSelectorEnabled
    ? flattenedLocationList.filter((x) => isScopeUsedAsBuilding(x))
    : []
  // isMappedEnabled is true when ticket integration is enabled for the customer in single tenant
  const isMappedEnabled =
    config.isSingleTenant && isCategoryEnabled && isScopeSelectorEnabled
  const categoryQuery = useGetTicketCategories({
    isEnabled: isMappedEnabled,
  })

  const ticketStatuses = useTicketStatuses()
  const subStatusQuery = useGetTicketSubStatus({
    enabled: isMappedEnabled,
  })
  const ticketStatus = ticketStatuses.getByStatusCode(ticket.statusCode)
  const isTicketClosed =
    ticketStatus && isTicketStatusEquates(ticketStatus, Status.closed)
  const isDiagnosticReadOnly = ticket.diagnostics?.length > 0
  const insightDiagnostic = isDiagnosticReadOnly
    ? {
        id: selectedInsight?.id,
        name: selectedInsight?.name,
        ruleName: selectedInsight?.ruleName,
        diagnostics: ticket.diagnostics,
      }
    : insightDiagnosticQuery.data

  function handleSubmit(form) {
    if (form.data.siteId == null) {
      throw new ValidationError({
        name: 'siteId',
        message: t('messages.siteRequired'),
      })
    }
    if (isMappedEnabled) {
      setSubmitted(true)
      const { serviceNeededId, jobTypeId, categoryId } = form.data
      if (
        [serviceNeededId, jobTypeId, categoryId].some((field) => field == null)
      ) {
        throw new ValidationError(
          'Please fill out all required fields to continue.'
        )
      }
    }

    const twinIdOption = form.data?.twinId ? { twinId: form.data.twinId } : {}

    if (form.data.id != null) {
      /**
       * If the attachment remains unchanged, return null
       * Otherwise, if there are modified attachments, return their IDs in an array
       * In the scenario where all attachments are deleted, provide an array with an empty GUID
       * Reference - https://dev.azure.com/willowdev/Unified/_workitems/edit/84355
       */
      let attachmentIds = []
      if (form.data.isAttachmentModified) {
        attachmentIds =
          form.data.attachments.length > 0
            ? form.data.attachments
                .map((attachment) => attachment.id)
                .filter((attachment) => attachment != null)
            : uuidv4()
      } else {
        attachmentIds = null
      }

      return form.api.put(
        `/api/sites/${form.data.siteId}/tickets/${form.data.id}`,
        {
          template: form.data.template,
          reporterId: form.data.reporterId,
          reporterName: form.data.reporterName,
          reporterPhone: form.data.reporterPhone,
          reporterEmail: form.data.reporterEmail,
          reporterCompany: form.data.reporterCompany,
          statusCode: form.data.statusCode,
          categoryId: form.data.categoryId,
          cause: form.data.cause,
          solution: form.data.solution,
          notes: form.data.notes,
          summary: form.data.summary,
          description: form.data.description,
          floorCode: form.data.floorCode,
          tasks: form.data.tasks,
          attachmentIds,
          newAttachmentFiles: form.data.attachments.filter(
            (attachment) => attachment instanceof File
          ),
          issueId: form.data.issueId,
          issueType: form.data.issueType,
          assigneeId: form.data.assignee?.id,
          assigneeType: form.data.assignee?.type,
          priority: form.data.priority,
          dueDate: form.data.dueDate,
          subStatusId: form.data.subStatusId,
          jobTypeId: form.data.jobTypeId,
          spaceTwinId: form.data.spaceTwinId,
          serviceNeededId: form.data.serviceNeededId,
          diagnostics: form.data.diagnostics,
          ...twinIdOption,
        },
        {
          headers: {
            'Content-Type': 'multipart/form-data',
          },
        }
      )
    }

    // Changing status of diagnostic insights selected by user to 'inProgress' when creating a ticket
    if ((form.data.diagnostics || []).length > 0) {
      const selectedInsightIds = form.data.diagnostics.map(
        ({ id: selectedInsightId }) => selectedInsightId
      )
      const isUpdatingMultipleInsights = selectedInsightIds.length > 1
      const url = isUpdatingMultipleInsights
        ? `/v2/sites/${form.data.siteId}/insights/status`
        : `/v2/sites/${form.data.siteId}/insights/${selectedInsightIds[0]}/status`

      const method = isUpdatingMultipleInsights ? api.post : api.put

      method(url, {
        siteId: form.data.siteId,
        insightIds: selectedInsightIds,
        status: 'inProgress',
      })
    }

    return form.api.post(
      `/api/sites/${form.data.siteId}/tickets`,
      {
        template: form.data.template,
        reporterId: form.data.reporterId,
        reporterName: form.data.reporterName,
        reporterPhone: form.data.reporterPhone,
        reporterEmail: form.data.reporterEmail,
        reporterCompany: form.data.reporterCompany,
        categoryId: form.data.categoryId,
        notes: form.data.notes,
        summary: form.data.summary,
        description: form.data.description,
        floorCode: form.data.floorCode,
        attachmentFiles: form.data.attachments,
        issueId: form.data.issueId,
        issueType: form.data.issueType,
        insightId: form.data.insightId,
        assigneeId: form.data.assignee?.id,
        assigneeType: form.data.assignee?.type,
        priority: form.data.priority,
        dueDate: form.data.dueDate,
        subStatusId: form.data.subStatusId,
        jobTypeId: form.data.jobTypeId,
        spaceTwinId: form.data.spaceTwinId,
        serviceNeededId: form.data.serviceNeededId,
        diagnostics: form.data.diagnostics,
        ...twinIdOption,
        ...(user.customer.features.isDynamicsIntegrationEnabled
          ? { sourceType: form.data.sourceType }
          : undefined),
      },
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    )
  }

  async function handleSubmitted(form) {
    // Polling for ticket sync
    if (isPolling) handlePollingAndSubmit(form)

    // Refresh tickets table after save.
    queryClient.invalidateQueries(['tickets'])
    // Refresh activities tab after saving ticket details.
    queryClient.invalidateQueries(['insightActivities'])

    const { siteId, insightId, id, priority, statusCode, cause, description } =
      form.data

    if (id == null) {
      const shouldClearInsight =
        insightId != null && insightId === user.localOptions.insight?.id
      if (shouldClearInsight) {
        user.clearOptions('insight')
      }

      onTicketChange({
        siteId: form.response.siteId,
        ticketId: form.response.id,
      })

      /**
       * Update the insights table and close the modal after creating a ticket.
       * Display an 'Insights set to in progress' message when the insight is in the open status,
       * and a 'Ticket created' snackbar message.
       */
      queryClient.invalidateQueries(['insights'])
      queryClient.invalidateQueries(['asset-insights'])
      queryClient.invalidateQueries(['all-insights'])
      queryClient.invalidateQueries(['insightInfo', insightId])

      // Show snackbar message when insight diagnostics has been added along with ticket link else show ticket generated as generic message
      if ((form.data.diagnostics || []).length > 0) {
        snackbar.show({
          title: t('interpolation.insightsSetToInProgress', {
            count: form.data.diagnostics.length,
          }),
          intent: 'positive',
        })
        snackbar.show({
          actions: (
            <StyledLink to={`/tickets/${form.response.id}`}>
              {form.response.summary}
            </StyledLink>
          ),
        })
      } else {
        snackbar.show({
          intent: 'positive',
          title: titleCase({ text: t('plainText.ticketCreated'), language }),
          actions: isPolling ? (
            <Button
              kind="primary"
              background="transparent"
              onClick={() => {
                setSearchParams({
                  action: InsightActions.newTicket,
                })
              }}
              css={{
                padding: '0px',
                textDecoration: 'underline',
              }}
            >
              {t('plainText.view')}
            </Button>
          ) : (
            <StyledLink
              to={`/tickets/${form.response.id}`}
              css={css(({ theme }) => ({
                color: theme.color.intent.primary.fg.default,
              }))}
            >
              {t('plainText.view')}
            </StyledLink>
          ),
        })

        if (isPolling) {
          snackbar.show({
            // TODO : Replace 'secondary' with 'sync' icon.
            // Link : https://dev.azure.com/willowdev/Unified/_workitems/edit/137015
            id: form.response.id,
            autoClose: false,
            loading: true,
            withCloseButton: true,
            intent: 'secondary',
            title: titleCase({ text: t('plainText.syncInProgress'), language }),
            description: form.response.summary,
            actions:
              dataSegmentPropPage === 'Tickets Page' ? (
                <StyledLink
                  to={`/tickets/${form.response.id}`}
                  css={css(({ theme }) => ({
                    color: theme.color.intent.primary.fg.default,
                  }))}
                >
                  {titleCase({ text: t('plainText.view'), language })}
                </StyledLink>
              ) : (
                <Button
                  kind="primary"
                  background="transparent"
                  onClick={() => {
                    setSearchParams({
                      action: InsightActions.ticket,
                      ticketId: form.response.id,
                    })
                  }}
                  css={{
                    padding: '0px',
                    textDecoration: 'underline',
                  }}
                >
                  {titleCase({ text: t('plainText.view'), language })}
                </Button>
              ),
          })
        }
      }
      if (ticket.insightStatus === 'open') {
        snackbar.show({
          intent: 'positive',
          title: _.upperFirst(t('plainText.insightSetToInProgress')),
        })
      }
      onClose()

      analytics.track('Ticket Created', {
        priority,
        insight_id: insightId,
        page: dataSegmentPropPage,
      })
    } else {
      onTicketChange()
      const updatedTicketStatus = ticketStatuses.getByStatusCode(statusCode)
      snackbar.show({
        title: _.upperFirst(t('plainText.ticketSaved')),
      })
      if (
        updatedTicketStatus &&
        isTicketStatusIncludes(updatedTicketStatus, [
          Status.resolved,
          Status.closed,
        ])
      ) {
        analytics.track(
          isTicketStatusEquates(updatedTicketStatus, Status.resolved)
            ? 'Desktop Ticket Completed'
            : 'Desktop Ticket Closed',
          {
            priority,
            status: updatedTicketStatus.status,
            cause,
            description,
          }
        )
      }
    }

    // invalidate tickets query to refresh the tickets associated with an insight
    // if the ticket is associated with an insight
    if (insightId) {
      queryClient.invalidateQueries(['insightTickets', siteId, insightId])
    }
  }

  return (
    <Form
      key={insightDiagnosticQuery?.data?.id}
      defaultValue={{
        ...ticket,
        diagnostics: getFailedDiagnostics(insightDiagnostic?.diagnostics ?? []),
        description: getDescriptionText(
          ticket.description,
          insightDiagnosticQuery?.data ?? { diagnostics: [] },
          t,
          language
        ),
      }}
      readOnly={isTicketClosed}
      onSubmit={handleSubmit}
      onSubmitted={handleSubmitted}
    >
      <Flex fill="header">
        <Flex>
          <Header />
          <>
            <SourceDetails ticket={ticket} isPolling={isPolling} />
            {insightDiagnosticQuery.status === 'loading' ? (
              <Loader tw="flex self-center" />
            ) : (
              selectedInsight?.id != null &&
              (isDiagnosticReadOnly || insightDiagnosticQuery.isSuccess) && (
                <LinkedInsights
                  insightDiagnostic={insightDiagnostic}
                  siteId={ticket.siteId}
                  isReadOnly={isDiagnosticReadOnly}
                />
              )
            )}
            <TicketSettings />
            <Asset />
            <TicketDetails
              ticket={ticket}
              isMappedEnabled={categoryQuery.isSuccess}
              categories={categoryQuery.data}
              ticketSubStatus={subStatusQuery.data}
              twins={sitesTwins}
              submitted={submitted}
            />
            <SolutionDetails />
            <Attachments />
            <CreatorDetails />
            <RequestorDetails />
            <Insight />
            <Dates />
            <Comments />
          </>
        </Flex>
        {/**
         * Showing Submit ticket text when user is creating a ticket and for
         * updating existing ticket it will always show Save ticket text
         */}
        {!isTicketClosed && (
          <ModalSubmitButton>
            {!isTicketUpdated
              ? _.upperFirst(t('plainText.submitTicket'))
              : t('plainText.saveTicket')}
          </ModalSubmitButton>
        )}
      </Flex>
    </Form>
  )
}

const StyledLink = styled(Link)({
  textDecoration: 'underline',

  '&:hover': {
    textDecoration: 'underline',
  },
})
