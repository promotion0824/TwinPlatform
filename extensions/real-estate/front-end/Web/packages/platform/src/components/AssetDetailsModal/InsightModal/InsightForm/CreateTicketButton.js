import { useModal, useUser } from '@willow/ui'
import { priorities } from '@willow/common'
import { Button } from '@willowinc/ui'
import insightStatuses from 'components/insightStatuses.json'
import { useTranslation } from 'react-i18next'

export default function CreateTicketButton({ insight, dataSegmentPropPage }) {
  const modal = useModal()
  const user = useUser()
  const { t } = useTranslation()

  function handleCreateTicketClick() {
    const nextItem = {
      id: null,
      modalType: 'ticket',
      siteId: insight.siteId,
      reporterId: user.id,
      reporterName: `${user.firstName} ${user.lastName}`,
      reporterPhone: user.mobile,
      reporterEmail: user.email,
      summary: insight.name,
      description: insight.description,
      floorCode: insight.floorCode,
      insightId: insight.id,
      insightName: insight.sequenceNumber,
      loadAttachments: true,
      insightStatus: insight.status,
    }

    if (insight.equipment) {
      nextItem.issueId = insight.equipment.id
      nextItem.issueName = insight.equipment.name
      nextItem.issueType = 'equipment'
    } else if (insight.asset) {
      nextItem.issueId = insight.asset.id
      nextItem.issueName = insight.asset.name
      nextItem.issueType = 'asset'
    }
    modal.close(nextItem)
  }

  return (
    <Button
      onClick={handleCreateTicketClick}
      data-segment="Insight Create Ticket Clicked"
      data-segment-props={JSON.stringify({
        type: insight.type,
        sourceName: insight.sourceName,
        priority: priorities.find(
          (priority) => priority.id === insight.priority
        )?.name,
        status: insightStatuses.find(
          (insightStatus) => insightStatus.id === insight.status
        )?.name,
        page: dataSegmentPropPage,
      })}
    >
      {t('plainText.createTicket')}
    </Button>
  )
}
