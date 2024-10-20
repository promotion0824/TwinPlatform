import { getSyncStatus, titleCase } from '@willow/common'
import { Link, SyncStatusComponent, useFeatureFlag, useUser } from '@willow/ui'
import {
  Box,
  Group,
  Icon,
  Illustration,
  Panel,
  PanelContent,
  PanelGroup,
  Stack,
} from '@willowinc/ui'

import { SyncStatus } from '@willow/common/ticketStatus'
import { useTranslation } from 'react-i18next'
import { css, styled } from 'twin.macro'
import { TicketSimpleDto } from '../../../services/Tickets/TicketsService'
import TicketStatusPill from '../../TicketStatusPill/TicketStatusPill'
import { InsightActions } from '../ui/ActionsViewControl'
import NotFound from '../ui/NotFound'
import styles from './LeftPanel.css'

const Actions = ({
  tickets,
  onTicketClick,
}: {
  tickets: TicketSimpleDto[]
  onTicketClick: (action: InsightActions) => void
}) => {
  const {
    t,
    i18n: { language },
  } = useTranslation()

  const user = useUser()
  const featureFlags = useFeatureFlag()

  const isPolling =
    featureFlags.hasFeatureToggle('ticketSync') &&
    user?.customer?.name === 'Walmart' &&
    featureFlags.hasFeatureToggle('mappedEnabled')

  const isNoData = tickets.length === 0

  return (
    <StyledPanelGroup
      direction="vertical"
      className={styles.insightActionsPanelContainer}
    >
      <Panel
        title={
          !isNoData && (
            <Group>
              <Icon icon="assignment" />
              {titleCase({
                text: t('headers.tickets'),
                language,
              })}
            </Group>
          )
        }
        collapsible={!isNoData}
      >
        <StyledPanelContent $isFullHeight={isNoData}>
          {isNoData &&
            (isPolling ? (
              <Stack w="100%" h="100%" align="center" p="s16" justify="center">
                <Illustration illustration="no-data" w="108px" />
                <Box
                  c="neutral.fg.default"
                  w="207px"
                  css={{
                    whiteSpace: 'wrap',
                    textAlign: 'center',
                  }}
                >
                  {titleCase({
                    text: t('plainText.noTicketsAvailable'),
                    language,
                  })}
                </Box>
              </Stack>
            ) : (
              <NotFound size={20} message={t('plainText.noTicketsAvailable')} />
            ))}
          {tickets.map((ticket) => {
            const syncStatus = getSyncStatus(ticket)

            // we display each ticket's summary as a link as per design
            // so we add the ticketId and action as query params to current pathname
            // reference: https://www.figma.com/file/9CJYfmtjVvvEdRHBYs8EFF/Card%2FSkills-View-%5BSpec%5D?node-id=501%3A291095&mode=dev
            const currentUrl = new URL(window.location.href)
            for (const { param, value } of [
              { param: 'ticketId', value: ticket.id },
              { param: 'action', value: InsightActions.ticket },
            ]) {
              currentUrl.searchParams.set(param, value)
            }

            return (
              <div key={ticket.id} className={styles.actionContainer}>
                <Link
                  css={css(({ theme }) => ({
                    ...theme.font.body.md.regular,
                    textDecoration: 'underline',
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    minWidth: '320px',
                    maxWidth: '320px',
                    whiteSpace: 'nowrap',
                  }))}
                  to={`${currentUrl.pathname}${currentUrl.search}`}
                  onClick={() => onTicketClick(InsightActions.ticket)}
                >
                  {ticket.summary}
                </Link>
                <div className={styles.badge}>
                  <TicketStatusPill statusCode={ticket.statusCode} />
                </div>
                {isPolling && (
                  <Group ml="s16">
                    <SyncStatusComponent syncStatus={syncStatus} />
                  </Group>
                )}
              </div>
            )
          })}
        </StyledPanelContent>
      </Panel>
    </StyledPanelGroup>
  )
}

export default Actions

const StyledPanelContent = styled(PanelContent)<{ $isFullHeight: boolean }>(
  ({ theme, $isFullHeight }) => ({
    padding: $isFullHeight ? 0 : theme.spacing.s16,
    paddingTop: 0,
    display: 'flex',
    flexWrap: 'wrap',
    gap: theme.spacing.s16,
    height: $isFullHeight ? '100%' : 'auto',
  })
)

// remove border on outermost div of <Panel /> and <PanelHeader />
const StyledPanelGroup = styled(PanelGroup)({
  '&&& > div, &&& > div > div > div': {
    border: 'none',
  },

  'container-type': 'inline-size',
  'container-name': 'insightActionsPanelContainer',
})
