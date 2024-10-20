import tw from 'twin.macro'
import styled from 'styled-components'
import { useTranslation } from 'react-i18next'
import { Link, useModal } from '@willow/ui'
import { Icon } from '@willowinc/ui'
import { InsightDetail, Container } from '@willow/common/insights/component'
import { Insight } from '@willow/common/insights/insights/types'
import TicketStatusPill from '../../../../TicketStatusPill/TicketStatusPill'

export default function Tickets({
  insight,
  setIsTicketUpdated,
}: {
  insight: Insight
  setIsTicketUpdated: (isUpdated: boolean) => void
}) {
  const { t } = useTranslation()
  const modal = useModal()

  function handleClick(ticket) {
    modal.close({
      ...ticket,
      modalType: 'ticket',
    })
    setIsTicketUpdated(true)
  }

  return (
    <Container $hidePaddingBottom>
      <InsightDetail
        headerIcon={<Icon icon="assignment" size={24} />}
        headerText={t('headers.tickets')}
      >
        {insight?.tickets != null && (insight.tickets?.length ?? 0) > 0
          ? insight.tickets.map((ticket) => (
              <div key={ticket.id} tw="flex">
                <StyledLink onClick={() => handleClick(ticket)}>
                  {ticket.summary}
                </StyledLink>
                <TicketStatusPill statusCode={ticket.statusCode} />
              </div>
            ))
          : t('plainText.noTicketCreated')}
      </InsightDetail>
    </Container>
  )
}

const StyledLink = styled(Link)(({ theme }) => ({
  lineHeight: theme.spacing.s24,
  marginRight: theme.spacing.s16,
  textDecoration: 'underline',
  width: '259px',
}))
