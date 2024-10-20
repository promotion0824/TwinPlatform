import { Modal, Tab, Tabs } from '@willow/ui'
import { styled } from 'twin.macro'
import Details from './Content/Details'
import Tickets from './Content/Tickets'
import Insights from './Content/Insights'
import Relationships from './Content/Relationships'
import styles from './AssetModal.css'

const BorderlessTabs = styled(Tabs)({
  border: 'none',
})

export enum AssetModalTab {
  details = 'details',
  tickets = 'tickets',
  insights = 'insights',
  relationships = 'relationships',
}

export default function AssetModal({
  t,
  assetId,
  selectedModalTab,
  onClose,
  onNextItem,
  onPreviousItem,
  siteId,
  insightId,
  header,
  onModalTabChange,
  onInsightIdChange,
  selectedTicketId,
  onSelectedTicketIdChange,
  insightTab,
  onInsightTabChange,
}) {
  return (
    <Modal
      size="large"
      onClose={onClose}
      showNavigationButtons
      onPreviousItem={onPreviousItem}
      onNextItem={onNextItem}
      header={header}
      className={styles.modal}
      isFormHeader
    >
      <BorderlessTabs>
        {[
          {
            tab: AssetModalTab.details,
            children: <Details assetId={assetId} siteId={siteId} />,
          },
          {
            tab: AssetModalTab.tickets,
            children: (
              <Tickets
                assetId={assetId}
                siteId={siteId}
                selectedTicketId={selectedTicketId}
                onSelectedTicketIdChange={onSelectedTicketIdChange}
              />
            ),
          },
          {
            tab: AssetModalTab.insights,
            children: (
              <Insights
                assetId={assetId}
                siteId={siteId}
                insightId={insightId}
                onInsightIdChange={onInsightIdChange}
                insightTab={insightTab}
                onInsightTabChange={onInsightTabChange}
              />
            ),
          },
          {
            tab: AssetModalTab.relationships,
            children: <Relationships assetId={assetId} siteId={siteId} />,
          },
        ].map(({ tab, children }) => (
          <Tab
            key={tab}
            header={t(`headers.${tab}`)}
            type="modal"
            selected={selectedModalTab === tab}
            onClick={() => onModalTabChange(tab)}
          >
            {children}
          </Tab>
        ))}
      </BorderlessTabs>
    </Modal>
  )
}
