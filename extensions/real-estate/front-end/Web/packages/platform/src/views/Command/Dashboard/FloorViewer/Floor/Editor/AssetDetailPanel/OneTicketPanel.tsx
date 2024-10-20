import { priorities, titleCase, useTicketStatuses } from '@willow/common'
import { PriorityBadge } from '@willow/common/insights/component/index'
import {
  Time,
  getTicketStatusTranslatedName,
  useAnalytics,
  useScopeSelector,
} from '@willow/ui'
import {
  Box,
  Card,
  Group,
  Icon,
  IconButton,
  Indicator,
  Stack,
  useTheme,
} from '@willowinc/ui'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'
import 'twin.macro'
import routes from '../../../../../../../routes'
import { TicketSimpleDto } from '../../../../../../../services/Tickets/TicketsService'
import {
  ButtonLink,
  GrayDot,
  MoreOrLessExpander,
  TICKET_CATEGORY,
  TICKET_NAME,
  TICKET_PRIORITY,
  TWIN_CATEGORY,
  TWIN_NAME,
} from './shared'

/**
 * A single ticket panel to be used in 3D viewer
 * that displays the ticket details
 */
export default function OneTicketPanel({
  ticket,
  siteId,
  twinName,
  twinCategory,
}: {
  ticket: TicketSimpleDto
  siteId: string
  twinName: string
  twinCategory: string
}) {
  const { scopeLookup } = useScopeSelector()
  const ticketStatuses = useTicketStatuses()
  const theme = useTheme()
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const analytics = useAnalytics()
  const [isOpen, setIsOpen] = useState(false)

  const ticketStatus = ticketStatuses.getByStatusCode(ticket.statusCode)

  return (
    <Card radius="r4" shadow="s2" mb="s12">
      <Box
        p="s8"
        bg="neutral.bg.accent.default"
        onClick={() => setIsOpen(!isOpen)}
        tw="cursor-pointer"
      >
        <div
          css={`
            display: flex;
            justify-content: space-between;
          `}
        >
          <Group>
            <Stack gap={0}>
              <span
                css={{
                  ...theme.font.heading.sm,
                  color: theme.color.neutral.fg.default,
                  whiteSpace: 'normal',
                }}
              >
                {ticket.summary}
              </span>
              <Group gap={2}>
                <Indicator
                  position="middle-center"
                  color={
                    ticket.priority === 1
                      ? theme.color.core.red.fg.default
                      : ticket.priority === 2
                      ? theme.color.core.orange.fg.default
                      : ticket.priority === 3
                      ? theme.color.core.yellow.fg.default
                      : theme.color.core.blue.fg.default
                  }
                >
                  <span
                    css={`
                      opacity: 0;
                    `}
                  >
                    {/* Content does not matter, it is used to position the indicator */}
                    xx
                  </span>
                </Indicator>
                <SmallText>
                  {titleCase({
                    text:
                      getTicketStatusTranslatedName(
                        t,
                        ticketStatus?.status ?? ''
                      ) ?? '-',
                    language,
                  })}
                </SmallText>
                <GrayDot />
                <Time
                  css={{
                    ...theme.font.body.sm.regular,
                    color: theme.color.neutral.fg.muted,
                  }}
                  value={ticket.createdDate ?? ''}
                  format="ago"
                />
                <GrayDot />
                <SmallText>
                  {titleCase({
                    text: ticket.category ?? '',
                    language,
                  })}
                </SmallText>
              </Group>
            </Stack>
          </Group>

          <PriorityBadge
            css={`
              align-self: center;
              min-width: fit-content;
            `}
            priority={priorities.find((p) => p.id === ticket.priority)!}
            size="sm"
          />

          <IconButton
            kind="secondary"
            background="transparent"
            css={`
              align-self: center;
              min-width: fit-content;
            `}
          >
            <Icon icon={isOpen ? 'keyboard_arrow_up' : 'keyboard_arrow_down'} />
          </IconButton>
        </div>
      </Box>
      {isOpen && (
        <Box p="s12" bg="neutral.bg.panel.default">
          <Stack
            css={{
              backgroundColor: theme.color.neutral.bg.panel.default,
            }}
          >
            <Stack gap={2}>
              <InnerHeader>{t('labels.description')}</InnerHeader>
              <ExpandableText ticket={ticket} />
            </Stack>

            <Stack gap={2}>
              <InnerHeader>{t('labels.assignee')}</InnerHeader>
              <StyledText>
                {ticket.assigneeName ?? t('plainText.unassigned')}
              </StyledText>
            </Stack>
            <Group w="100%" justify="flex-end">
              <ButtonLink
                to={`${
                  scopeLookup[siteId]?.twin?.id
                    ? routes.tickets_scope__scopeId_ticket__ticketId(
                        scopeLookup[siteId]?.twin?.id,
                        ticket.id
                      )
                    : routes.tickets_ticketId(ticket.id)
                }?noPresetFilter=1`}
                text={titleCase({
                  text: t('plainText.viewTicket'),
                  language,
                })}
                onClick={() =>
                  analytics.track('3D Viewer - View Ticket Clicked', {
                    [TWIN_NAME]: twinName,
                    [TWIN_CATEGORY]: twinCategory,
                    [TICKET_NAME]: ticket.summary,
                    [TICKET_CATEGORY]: ticket.category,
                    [TICKET_PRIORITY]: ticket.priority,
                  })
                }
              />
            </Group>
          </Stack>
        </Box>
      )}
    </Card>
  )
}

const ExpandableText = ({ ticket }: { ticket: TicketSimpleDto }) => {
  const [expanded, setExpanded] = useState(false)

  return (
    <Stack gap={2}>
      <StyledText
        css={{
          ...(!expanded
            ? {
                display: '-webkit-box',
                WebkitLineClamp: 3,
                WebkitBoxOrient: 'vertical',
                overflow: 'hidden',
                textOverflow: 'ellipsis',
              }
            : {}),
        }}
      >
        {ticket.description}
      </StyledText>
      <MoreOrLessExpander
        expanded={expanded}
        onClick={() => setExpanded((prev) => !prev)}
      />
    </Stack>
  )
}

const StyledText = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.default,
  textOverflow: 'ellipsis',
  overflow: 'hidden',
}))

const InnerHeader = styled(StyledText)(({ theme }) => ({
  color: theme.color.neutral.fg.muted,
}))

const SmallText = styled(StyledText)(({ theme }) => ({
  ...theme.font.body.sm.regular,
  color: theme.color.neutral.fg.muted,
}))
