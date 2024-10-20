/* eslint-disable complexity */
import tw, { styled } from 'twin.macro'
import { useEffect, useState } from 'react'
import {
  Modal,
  api,
  reduceQueryStatuses,
  Message,
  Progress,
  Icon,
  useModal,
} from '@willow/ui'
import { TextWithTooltip } from '@willow/common/insights/component'
import { getInsightPoints } from '@willow/common/insights/costImpacts/utils'
import { Button } from '@willowinc/ui'
import {
  Insight,
  InsightWorkflowStatus,
  Occurrence,
  InsightPointsDto,
} from '@willow/common/insights/insights/types'
import { useQueries } from 'react-query'
import { useTranslation, TFunction } from 'react-i18next'
import { TicketSimpleDto } from '../../../services/Tickets/TicketsService'
import InsightTabs from './InsightMetricsForm/Tab/InsightTabs'
import useUpdateInsightsStatuses from '../../../hooks/Insight/useUpdateInsightsStatuses'
import InsightWorkflowStatusPill from '../../InsightStatusPill/InsightWorkflowStatusPill'
import ActionsViewControl from '../../Insights/ui/ActionsViewControl'
import useGetInsightActivities from '../../../hooks/Insight/useGetInsightActivities'
import { selectOccurrences } from '@willow/common/utils/insightUtils'

/**
 * this is the Insight Modal to work with insight flow status values of open, new, inProgress, resolved, ignored
 * figma reference: https://www.figma.com/file/dUfwhUC42QG7UkxGTgjv7Q/Insights-to-Action-V2?type=design&node-id=4441-59932&t=Q0mHEnBIiBsQVf8f-0
 */
export default function InsightWorkFlowModal({
  siteId,
  insightId,
  name,
  onClose,
  lastStatus,
  showNavigationButtons,
  onPreviousItem,
  onNextItem,
  setIsTicketUpdated,
  insightTab,
  onInsightTabChange,
  canDeleteInsight,
}: {
  siteId: string
  insightId: string
  name?: string
  onClose: () => void
  lastStatus: InsightWorkflowStatus
  showNavigationButtons: boolean
  onPreviousItem: (index: number) => void
  onNextItem: (index: number) => void
  setIsTicketUpdated: (isUpdated: boolean) => void
  insightTab?: string
  onInsightTabChange?: (tab: string) => void
  canDeleteInsight: boolean
}) {
  const { t } = useTranslation()
  const [actionsViewControlOpen, setActionsViewControlOpen] = useState(false)

  const isUpdateStatusRequired = lastStatus === 'new'
  const { data: activitiesData } = useGetInsightActivities(siteId, insightId)

  // business requirement to update insight.lastStatus from "New" => "Open"
  // the first time when user open up the modal (insight drawer)
  // reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/78997
  const mutation = useUpdateInsightsStatuses({
    siteId,
    insightIds: [insightId],
    newStatus: 'open',
  })

  // Typically, it is common to place the mutation call inside a click handler.
  // However, in our situation, the click handler is defined at the InsightTable component,
  // and passing the mutation's status down multiple levels would be necessary
  // if we wanted to handle loading and error states.
  // Additionally, since we know for certain that the modal will be displayed as a result of the click,
  // it makes sense to include the mutation call directly in this location.
  useEffect(() => {
    if (isUpdateStatusRequired) {
      mutation.mutate()
    }
  }, [isUpdateStatusRequired])

  // ensure only to query the individual insight's detailed info after
  // mutation is successful and the insightId matches mutation's insightId
  // or we never need to mutate the insight status
  const enabledInsightInfoQueries =
    (isUpdateStatusRequired &&
      mutation.isSuccess &&
      mutation.data?.id === insightId) ||
    !isUpdateStatusRequired

  const insightInfoQueries = useQueries([
    {
      queryKey: ['insightInfo', siteId, insightId],
      queryFn: async (): Promise<Insight> => {
        const { data } = await api.get(`/sites/${siteId}/insights/${insightId}`)
        return data
      },
      enabled: enabledInsightInfoQueries,
    },
    {
      queryKey: ['insightTickets', siteId, insightId],
      queryFn: async (): Promise<TicketSimpleDto[]> => {
        const { data } = await api.get(
          `/sites/${siteId}/insights/${insightId}/tickets`
        )
        return data
      },
      enabled: enabledInsightInfoQueries,
    },
    {
      queryKey: ['insightOccurrences', siteId, insightId],
      queryFn: async (): Promise<Occurrence[]> => {
        const { data } = await api.get(
          `/sites/${siteId}/insights/${insightId}/occurrences`
        )
        return data
      },
      enabled: enabledInsightInfoQueries,
      select: selectOccurrences,
    },
    {
      queryKey: ['insightPoints', siteId, insightId],
      queryFn: async (): Promise<InsightPointsDto> => {
        const { data } = await api.get(
          `/sites/${siteId}/insights/${insightId}/points`
        )
        return data
      },
      enabled: !!siteId && !!insightId,
      select: getInsightPoints,
    },
  ])

  const reducedQueryStatus = reduceQueryStatuses(
    [...insightInfoQueries, ...(isUpdateStatusRequired ? [mutation] : [])].map(
      (query) => query.status
    )
  )

  const isActionsViewControlReady = !['error', 'loading'].includes(
    reducedQueryStatus
  )

  const insightDetail = insightInfoQueries[0].data
  const pointsQuery = insightInfoQueries[3]

  return (
    <Modal
      header={() => (
        <InsightWorkflowModalSubheader
          insight={insightDetail}
          // prefer to use status and name coming from props
          // as they are ready before insightInfoQueries finish running
          // however, we need to use the data from insightInfoQueries
          // as status/name from props are undefined when
          // user clicks on a ticket link on this modal, and then
          // click the insight link coming back to this modal
          insightName={
            // show insight's ruleName as its name just like in the insight table if it has one
            name || insightDetail?.ruleName || insightDetail?.name
          }
          lastStatus={lastStatus}
          t={t}
          actionsViewControlOpen={
            actionsViewControlOpen && isActionsViewControlReady
          }
          onClick={() => setActionsViewControlOpen(!actionsViewControlOpen)}
          canDeleteInsight={canDeleteInsight}
          onClose={onClose}
        />
      )}
      size="large"
      onClose={onClose}
      showNavigationButtons={showNavigationButtons}
      onPreviousItem={onPreviousItem}
      onNextItem={onNextItem}
    >
      {reducedQueryStatus === 'error' ? (
        <Message tw="h-full" icon="error">
          {t('plainText.errorOccurred')}
        </Message>
      ) : reducedQueryStatus === 'loading' ? (
        <Progress />
      ) : (
        reducedQueryStatus === 'success' &&
        insightDetail != null && (
          <InsightTabs
            insight={{
              ...insightDetail,
              tickets: insightInfoQueries[1].data ?? [],
            }}
            setIsTicketUpdated={setIsTicketUpdated}
            occurrences={insightInfoQueries[2].data ?? []}
            activities={activitiesData}
            insightTab={insightTab}
            onInsightTabChange={onInsightTabChange}
            twinInfo={{
              twinName: insightDetail?.equipmentName,
              twinId: insightDetail?.twinId,
              isInsightPointsLoading: pointsQuery.isLoading,
              insightPoints: pointsQuery.data?.insightPoints ?? [],
              impactScorePoints: pointsQuery.data?.impactScorePoints ?? [],
              isAnyDiagnosticSelected: false,
            }}
          />
        )
      )}
    </Modal>
  )
}

function InsightWorkflowModalSubheader({
  insight,
  insightName,
  lastStatus,
  actionsViewControlOpen,
  onClick,
  onClose,
  t,
  canDeleteInsight,
}: {
  insight?: Insight
  insightName?: string
  lastStatus: InsightWorkflowStatus
  actionsViewControlOpen: boolean
  onClick: () => void
  onClose: () => void
  t: TFunction
  canDeleteInsight: boolean
}) {
  const {
    id: insightId,
    siteId,
    lastStatus: mostUpdateToDateLastStatus,
    asset,
    floorId,
    sequenceNumber,
  } = insight ?? {}
  const modal = useModal()

  // show "Insight - Name of Insight" when insightName is defined
  // otherwise, just show "Insight"
  const insightHeaderText = `${t('headers.insight')}${`${
    insightName && insightName !== '' ? ` - ${insightName}` : ''
  }`}`

  return (
    <Container>
      <FlexRow>
        <div tw="flex gap-[12px]">
          <InsightWorkflowStatusPill
            size="lg"
            lastStatus={mostUpdateToDateLastStatus ?? lastStatus}
            tw="min-w-min"
          />
          <TextWithTooltip tw="max-w-[600px]" text={insightHeaderText} />
        </div>
        {siteId && insight && (
          <ActionsViewControl
            selectedInsight={insight}
            siteId={siteId}
            lastStatus={lastStatus}
            assetId={asset?.id}
            floorId={floorId}
            canDeleteInsight={canDeleteInsight}
            onCreateTicketClick={() =>
              modal.close({
                insightId,
                insightName: sequenceNumber,
                siteId,
                modalType: 'newTicket',
              })
            }
            onDeleteClick={() =>
              modal.close({
                modalType: 'deleteInsightsConfirmation',
              })
            }
            onResolveClick={() =>
              modal.close({
                modalType: 'resolveInsightConfirmation',
              })
            }
            onReportClick={() =>
              modal.close({
                modalType: 'report',
              })
            }
            onModalClose={onClose}
            opened={actionsViewControlOpen}
            onToggleActionsView={onClick}
          >
            <StyledButton onClick={onClick} className="insightActionIcon">
              <span tw="pointer-events-none">{t('plainText.actions')}</span>
              <ChevronIcon
                tw="pointer-events-none"
                icon="chevron"
                $isExpanded={actionsViewControlOpen}
              />
            </StyledButton>
          </ActionsViewControl>
        )}
      </FlexRow>
    </Container>
  )
}

const FlexRow = styled.div(({ theme }) => ({
  display: 'flex',
  justifyContent: 'space-between',
  ...theme.font.heading.xl,
  marginLeft: theme.spacing.s24,
  color: theme.color.neutral.fg.default,
}))

const ChevronIcon = styled(Icon)<{ $isExpanded: boolean }>(
  ({ $isExpanded }) => ({
    transform: $isExpanded ? 'rotate(-180deg)' : undefined,
    transition: 'var(--transition-out)',
  })
)

const Container = styled.div({
  display: 'flex',
  flexDirection: 'column',
  height: '100%',
  justifyContent: 'center',
})

// click event should only be triggered on the Button, not the inner span
const StyledButton = styled(Button)({
  '& .mantine-Button-inner': {
    pointerEvents: 'none',
  },
})
