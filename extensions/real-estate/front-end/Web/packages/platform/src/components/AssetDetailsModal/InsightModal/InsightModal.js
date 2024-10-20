import { Fetch, Modal } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import InsightForm from './InsightForm/InsightForm'

/**
 * Legacy Modal to work with legacy insight status of open, acknowledged, inProgress and closed
 */
export default function InsightModal({
  siteId,
  insightId,
  onClose,
  showNavigationButtons,
  onPreviousItem,
  onNextItem,
  dataSegmentPropPage,
  onClearSelectedInsightIds,
  setIsTicketUpdated,
}) {
  const { t } = useTranslation()

  return (
    <Modal
      header={t('headers.insight')}
      size="large"
      onClose={onClose}
      showNavigationButtons={showNavigationButtons}
      onPreviousItem={onPreviousItem}
      onNextItem={onNextItem}
    >
      <Fetch
        name="insight"
        url={[
          `/api/sites/${siteId}/insights/${insightId}`,
          `/api/sites/${siteId}/insights/${insightId}/tickets`,
          `/api/sites/${siteId}/insights/${insightId}/commands`,
        ]}
      >
        {([insight, tickets, commands]) => (
          <InsightForm
            insight={{
              siteId,
              ...insight,
              tickets,
              commands,
            }}
            dataSegmentPropPage={dataSegmentPropPage}
            onClearSelectedInsightIds={onClearSelectedInsightIds}
            setIsTicketUpdated={setIsTicketUpdated}
          />
        )}
      </Fetch>
    </Modal>
  )
}
