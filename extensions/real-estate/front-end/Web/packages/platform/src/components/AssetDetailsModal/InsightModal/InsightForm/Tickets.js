import { useTranslation } from 'react-i18next'
import { useModal, Fieldset, Flex, Link } from '@willow/ui'
import { useSite } from 'providers'
import TicketStatusPill from '../../../TicketStatusPill/TicketStatusPill.tsx'

export default function Tickets({ insight, setIsTicketUpdated }) {
  const modal = useModal()
  const site = useSite()
  const { t } = useTranslation()

  function handleClick(ticket) {
    modal.close({
      ...ticket,
      modalType: 'ticket',
    })
    setIsTicketUpdated(true)
  }

  if (site.features.isTicketingDisabled || insight.tickets.length === 0) {
    return null
  }

  return (
    <Fieldset icon="tickets" legend={t('headers.tickets')}>
      <Flex horizontal>
        <Flex size="large" flex={2}>
          {insight.tickets.map((ticket) => (
            <Flex
              key={ticket.id}
              horizontal
              fill="header"
              align="middle"
              size="medium"
            >
              <Flex align="left">
                <Link // eslint-disable-line
                  onClick={() => handleClick(ticket)}
                >
                  {ticket.sequenceNumber}
                </Link>
              </Flex>
              <TicketStatusPill statusCode={ticket.statusCode} />
            </Flex>
          ))}
        </Flex>
        <Flex flex={1} />
      </Flex>
    </Fieldset>
  )
}
