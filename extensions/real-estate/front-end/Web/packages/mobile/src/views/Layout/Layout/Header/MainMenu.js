import { WillowLogoWhite } from '@willow/common'
import { Tab as TicketTab } from '@willow/common/ticketStatus'
import { Icon, ModalNew as Modal, Spacing, Text } from '@willow/mobile-ui'
import { Link } from 'react-router-dom'
import { useLayout } from '../LayoutContext'
import styles from './MainMenu.css'
import MainMenuButton from './MainMenuButton'

export default function MainMenu({ onClose }) {
  const layout = useLayout()
  const { site } = layout

  return (
    <Modal
      className={styles.modal}
      header={
        <Link
          data-segment="Willow Home Button Clicked"
          onClick={onClose}
          to="/"
        >
          <WillowLogoWhite height={18} />
        </Link>
      }
      type="left"
      onClose={onClose}
    >
      <div className={styles.menu}>
        <Spacing padding="large large small">
          <Text type="h3" className={styles.menuTitle}>
            Menu
          </Text>
        </Spacing>
        <MainMenuButton
          key="tickets"
          tile="T"
          header={
            site.features.isScheduledTicketsEnabled
              ? 'Standard Tickets'
              : 'Tickets'
          }
          to={`/tickets/sites/${site.id}/${TicketTab.open}`}
        />
        {site.features.isScheduledTicketsEnabled && (
          <MainMenuButton
            tile="S"
            key="scheduled-tickets"
            header="Scheduled Tickets"
            to={`/sites/${site.id}/scheduled-tickets/${TicketTab.open}`}
          />
        )}
        {site.features.isInspectionEnabled && (
          <MainMenuButton
            key="inspections"
            header="Inspections"
            to={`/sites/${site.id}/inspectionZones`}
          />
        )}
        <MainMenuButton
          key="assets"
          tile={<Icon icon="search" />}
          header="Asset Search"
          to={`/sites/${site.id}/floors`}
        />
        <hr />
      </div>
    </Modal>
  )
}
