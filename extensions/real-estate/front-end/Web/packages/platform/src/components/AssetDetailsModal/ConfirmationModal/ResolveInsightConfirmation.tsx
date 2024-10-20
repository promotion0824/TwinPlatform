/* eslint-disable complexity */
import { titleCase, useTicketStatuses } from '@willow/common'
import useMultipleSearchParams from '@willow/common/hooks/useMultipleSearchParams'
import { Insight } from '@willow/common/insights/insights/types'
import {
  api,
  Icon as LegacyIcon,
  Message,
  Progress,
  reduceQueryStatuses,
  TextArea,
  useModal,
  useSnackbar,
  useUser,
} from '@willow/ui'
import { Icon } from '@willowinc/ui'
import _ from 'lodash'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useQuery } from 'react-query'
import { useParams } from 'react-router'
import styled from 'styled-components'
import tw from 'twin.macro'
import useGetInsights from '../../../hooks/Insight/useGetInsights'
import useUpdateInsightsStatuses from '../../../hooks/Insight/useUpdateInsightsStatuses'
import useUpdateTicketStatus from '../../../hooks/Tickets/useUpdateTicketStatus'
import { FilterOperator } from '../../../services/Insight/InsightsService'
import { TicketSimpleDto } from '../../../services/Tickets/TicketsService'
import { InsightActions } from '../../Insights/ui/ActionsViewControl'
import {
  CancelButton,
  Circle,
  ConfirmButton,
  Container,
  HeadingExtraSmall,
  HeadingSmall,
  IndicatorContainer,
  Line,
  ModalHeader,
  PurpleBorderButton,
  StepContentContainer,
  stepIndicators,
  StyledIcon,
  TicketLink,
  TicketStatusSelect,
} from './shared'

/**
 * Modal content to be used for resolving single insight
 * reference: https://www.figma.com/file/dUfwhUC42QG7UkxGTgjv7Q/Insights-to-Action-V2?node-id=5912%3A107600
 */
const ResolveInsightConfirmation = ({
  onActionChange,
  onClose,
}: {
  onActionChange?: (action?: string) => void
  onClose?: () => void
}) => {
  const user = useUser()
  const { isClosed } = useTicketStatuses()
  const modal = useModal()
  const snackbar = useSnackbar()
  const params = useParams<{ insightId?: string }>()
  const [{ insightId }] = useMultipleSearchParams(['insightId'])
  const selectedInsightId = params.insightId ?? insightId
  const insightsQuery = useGetInsights(
    {
      filterSpecifications: [
        {
          field: 'id',
          operator: FilterOperator.equalsLiteral,
          value: selectedInsightId,
        },
      ],
    },
    { enabled: selectedInsightId != null }
  )
  const selectedInsight = insightsQuery.data?.find(
    (insight) => insight.id === selectedInsightId
  )

  const [step, setStep] = useState(minStep)
  const [isInitialLoad, setIsInitialLoad] = useState(true)
  const [isTicketsReady, setIsTicketsReady] = useState(false)

  const [reason, setReason] = useState<undefined | string>(
    user?.localOptions?.[`resolveInsightReason-${selectedInsight?.id}`] ?? ''
  )
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const ticketsQuery = useQuery(
    ['insightTickets', selectedInsight?.siteId, selectedInsight?.id],
    async (): Promise<TicketSimpleDto[]> => {
      const { data } = await api.get(
        `/sites/${selectedInsight?.siteId}/insights/${selectedInsight?.id}/tickets`
      )
      return data
    },
    { enabled: !!selectedInsight?.id && !!selectedInsight?.siteId }
  )

  // business logic to determine which step of resolve insight modal to show
  // 1. if all tickets are closed, show step 2
  // 2. otherwise, show step 1
  // logic is only executed on initial load, and the dispatcher to update isInitialLoad
  // is passed down so that e.g. when user closes all tickets, UI does not automatically
  // jump to step 2
  // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/84470
  useEffect(() => {
    if (ticketsQuery.isSuccess) {
      const isAllTicketsClosed = (ticketsQuery.data ?? []).every((ticket) =>
        isClosed(ticket.statusCode)
      )
      if (isInitialLoad && isAllTicketsClosed) {
        setStep(2)
        setIsInitialLoad(false)
      }

      setIsTicketsReady(true)
    }
  }, [isClosed, isInitialLoad, ticketsQuery])

  const resolveInsightMutation = useUpdateInsightsStatuses({
    siteId: selectedInsight?.siteId ?? '',
    insightIds: [selectedInsight?.id ?? ''],
    newStatus: 'resolved',
    reason,
  })

  const handleConfirmationClose = () => {
    modal.close()
  }

  const handleStepChange = (diff: number) => {
    if ((step === minStep && diff === -1) || (step === maxStep && diff === 1)) {
      // do nothing
    } else {
      setStep((prevStep) => prevStep + diff)
    }
  }

  const handleResolveInsight = () => {
    resolveInsightMutation.mutate(undefined, {
      onError: () => {
        snackbar.show(t('plainText.errorOccurred'))
      },
      onSuccess: () => {
        snackbar.show(
          _.capitalize(
            t('interpolation.insightsActioned', {
              count: 1,
              action: t('headers.resolved'),
            })
          ),
          {
            isToast: true,
            closeButtonLabel: t('plainText.dismiss'),
          }
        )
        onClose?.()
      },
    })
  }

  const handleTicketLinkClick = (ticket: TicketSimpleDto) => {
    modal.close({
      ...ticket,
      modalType: 'ticket',
    })
    // set selected action to open resolve insight modal following
    // closure of ticket modal since all tickets need to be closed
    // before insight can be resolved
    // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/80180
    onActionChange?.(InsightActions.resolve)
  }

  const isContinueDisabled = (currentStep: number) => {
    if (
      selectedInsight == null ||
      ['loading', 'error'].includes(ticketsQuery.status)
    ) {
      return true
    }

    if (currentStep === 1) {
      // disable continue button if there exists an ticket not having closed status on step 1
      return (ticketsQuery.data ?? []).some(
        (ticket) => !isClosed(ticket.statusCode)
      )
    }

    if (currentStep === 2) {
      return reason == null || reason === ''
    }

    if (currentStep === 3) {
      return resolveInsightMutation.isLoading
    }

    return false
  }

  // save user's input for reason to resolve insight in local storage
  // so that it can be restored when user reopens the modal
  const handleReasonChange = (value?: string) => {
    setReason(value ?? '')
    user.saveLocalOptions(`resolveInsightReason-${selectedInsight?.id}`, value)
  }

  const continueButtonText =
    step === maxStep ? t('plainText.resolve') : t('plainText.continue')
  const cancelButtonText =
    step === minStep ? t('plainText.cancel') : t('plainText.back')

  const reducedQueryStatus = reduceQueryStatuses([
    insightsQuery.status,
    ticketsQuery.status,
  ])

  return (
    <Container tw="w-[600px]" data-testid="resolve-insight-confirmation-modal">
      <div tw="flex justify-between">
        <ModalHeader
          css={`
            color: ${({ theme }) => theme.color.neutral.fg.default};
          `}
        >
          {titleCase({ text: t('plainText.resolveInsight'), language })}
        </ModalHeader>
        <StyledIcon
          onClick={handleConfirmationClose}
          tw="cursor-pointer"
          icon="close"
        />
      </div>

      <IndicatorContainer>
        {/*
          step is either 1, 2, or 3, when user completes a step and moves to next,
          the completed steps will show a checkmark, otherwise it will show the step number
        */}
        {stepIndicators.map(({ text, isLast = false }, zeroBasedIndex) => {
          const oneBasedIndex = zeroBasedIndex + 1

          return (
            <div key={text} tw="flex flex-grow">
              <Circle
                $selected={isTicketsReady ? step >= oneBasedIndex : false}
                tw="mr-[6px]"
              >
                {step > oneBasedIndex ? <Icon icon="check" /> : oneBasedIndex}
              </Circle>
              <span tw="whitespace-nowrap truncate">
                {titleCase({ text: t(text), language })}
              </span>
              {/* only show line in between steps */}
              {!isLast && <Line />}
            </div>
          )
        })}
      </IndicatorContainer>

      {reducedQueryStatus === 'loading' || !isTicketsReady ? (
        <Progress />
      ) : reducedQueryStatus === 'error' ? (
        <Message icon="error">{t('plainText.errorOccurred')}</Message>
      ) : step === 1 ? (
        <StepOneContent
          tickets={ticketsQuery.data ?? []}
          onTicketLinkClick={handleTicketLinkClick}
          onTicketStatusChange={setIsInitialLoad}
        />
      ) : step === 2 ? (
        <StepTwoContent
          insight={selectedInsight}
          reason={reason}
          onReasonChange={handleReasonChange}
        />
      ) : (
        <StepThreeContent
          insight={selectedInsight}
          tickets={ticketsQuery.data ?? []}
          onTicketLinkClick={handleTicketLinkClick}
          reason={reason}
        />
      )}

      <div tw="flex justify-end gap-[1rem]">
        <CancelButton
          disabled={resolveInsightMutation.isLoading}
          onClick={
            step === minStep
              ? handleConfirmationClose
              : () => handleStepChange(-1)
          }
        >
          {_.capitalize(cancelButtonText)}
        </CancelButton>
        <ConfirmButton
          prefix={
            (ticketsQuery.status === 'loading' ||
              resolveInsightMutation.status === 'loading') && (
              <LegacyIcon icon="progress" />
            )
          }
          disabled={isContinueDisabled(step)}
          onClick={() =>
            step === maxStep ? handleResolveInsight() : handleStepChange(1)
          }
        >
          {_.capitalize(continueButtonText)}
        </ConfirmButton>
      </div>
    </Container>
  )
}

export default ResolveInsightConfirmation
const minStep = 1
const maxStep = 3

const StepOneContent = ({
  tickets,
  onTicketLinkClick,
  onTicketStatusChange,
}: {
  tickets: TicketSimpleDto[]
  onTicketLinkClick: (ticket: TicketSimpleDto) => void
  onTicketStatusChange: (isTicketStatusChanged: boolean) => void
}) => {
  const {
    t,
    i18n: { language },
  } = useTranslation()

  return (
    <>
      <HeadingSmall>
        {titleCase({ text: t('plainText.closeTicketsToContinue'), language })}
      </HeadingSmall>
      <HeadingExtraSmall>
        {_.capitalize(t('plainText.closeTicketNotification'))}
      </HeadingExtraSmall>
      <StepContentContainer>
        {tickets.map((ticket) => (
          <SingleTicket
            key={ticket.id}
            ticket={ticket}
            onTicketLinkClick={onTicketLinkClick}
            onTicketStatusChange={onTicketStatusChange}
          />
        ))}
      </StepContentContainer>
    </>
  )
}

const SingleTicket = ({
  ticket,
  onTicketLinkClick,
  className,
  $isUpdateStatusAllowed = true,
  onTicketStatusChange,
}: {
  ticket: TicketSimpleDto
  onTicketLinkClick?: (ticket: TicketSimpleDto) => void
  className?: string
  $isUpdateStatusAllowed?: boolean
  onTicketStatusChange?: (isTicketStatusChanged: boolean) => void
}) => {
  const { t } = useTranslation()
  const snackbar = useSnackbar()
  const mutation = useUpdateTicketStatus(
    {
      ticket,
    },
    {
      onSuccess: () => {
        snackbar.show(_.upperFirst(t('plainText.ticketSaved')), {
          isToast: true,
          closeButtonLabel: t('plainText.dismiss'),
        })
      },
      onError: (e) => {
        console.error(e)
        snackbar.show(t('plainText.errorOccurred'))
      },
    }
  )

  return (
    <SingleTicketContainer
      className={className}
      key={ticket.id}
      $isUpdateStatusAllowed={$isUpdateStatusAllowed}
    >
      <TicketLink onClick={() => onTicketLinkClick?.(ticket)}>
        {ticket?.sequenceNumber}
      </TicketLink>
      <div tw="flex">
        {mutation.status === 'loading' && (
          <LegacyIcon
            tw="h-full flex flex-col justify-center mr-4"
            icon="progress"
          />
        )}
        <TicketStatusSelect
          statusCode={ticket.statusCode}
          onChange={(value) => {
            mutation.mutate(value)
            onTicketStatusChange?.(false)
          }}
          disabled={
            mutation.status === 'loading' || $isUpdateStatusAllowed === false
          }
        />
      </div>
    </SingleTicketContainer>
  )
}

const SingleTicketContainer = styled.div<{ $isUpdateStatusAllowed?: boolean }>(
  ({ $isUpdateStatusAllowed = true }) => ({
    display: 'flex',
    justifyContent: $isUpdateStatusAllowed ? 'space-between' : '',
    '&&& button': {
      backgroundColor: $isUpdateStatusAllowed ? '' : 'transparent',
    },
  })
)

const StepTwoContent = ({
  insight,
  reason,
  onReasonChange,
}: {
  insight?: Insight
  reason?: string
  onReasonChange: (nextReason?: string | ((prev: string) => string)) => void
}) => {
  const { t } = useTranslation()
  const hasRecommendation = insight?.recommendation != null

  return (
    <>
      {hasRecommendation && (
        <>
          <HeadingSmall>{t('labels.recommendation')}</HeadingSmall>
          <StepContentContainer>{insight.recommendation}</StepContentContainer>
        </>
      )}
      <HeadingSmall>{t('plainText.userCustomSolution')}</HeadingSmall>
      <div
        css={`
          display: flex;
          gap: ${({ theme }) => theme.spacing.s8};
        `}
      >
        {[
          ...(hasRecommendation ? ['plainText.recommendationWasHelpful'] : []),
          'plainText.insightWasNotCorrect',
          'plainText.insightWasHelpful',
        ].map((text) => (
          // append some predefined text to the reason
          <PurpleBorderButton
            key={text}
            onClick={() =>
              onReasonChange(
                reason && reason !== '' ? `${reason}\n${t(text)}` : t(text)
              )
            }
          >
            {t(text)}
          </PurpleBorderButton>
        ))}
      </div>
      <TextArea
        data-testid="userInputSolution"
        value={reason}
        onChange={onReasonChange}
      />
    </>
  )
}

const StepThreeContent = ({
  insight,
  tickets,
  onTicketLinkClick,
  reason,
}: {
  insight?: Insight
  tickets: TicketSimpleDto[]
  onTicketLinkClick?: (ticket: TicketSimpleDto) => void
  reason?: string
}) => {
  const {
    t,
    i18n: { language },
  } = useTranslation()

  return (
    <>
      <HeadingSmall>{t('labels.summary')}</HeadingSmall>
      <StepContentContainer tw="max-h-[304px]">
        <HeadingSmall>
          {`${t('headers.insight')} - ${
            insight?.ruleName ?? insight?.name ?? ''
          }`}
        </HeadingSmall>
        <HeadingExtraSmall>
          {titleCase({
            text: t('plainText.closedTickets'),
            language,
          })}
        </HeadingExtraSmall>
        {tickets.map((ticket) => (
          <SingleTicket
            $isUpdateStatusAllowed={false}
            key={ticket.id}
            ticket={ticket}
            onTicketLinkClick={onTicketLinkClick}
          />
        ))}
        {insight?.recommendation && (
          <>
            <HeadingExtraSmall>
              {titleCase({
                text: t('plainText.improveYourInsights'),
                language,
              })}
            </HeadingExtraSmall>
            {insight.recommendation}
          </>
        )}
        <HeadingExtraSmall>
          {titleCase({
            text: t('plainText.solutionComment'),
            language,
          })}
        </HeadingExtraSmall>
        <TextArea value={reason} readOnly />
      </StepContentContainer>
    </>
  )
}
