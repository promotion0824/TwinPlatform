import { titleCase } from '@willow/common'
import { Link } from '@willow/ui'
import { Button, Icon, Indicator, UnstyledButton } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { css, styled } from 'styled-components'

export function ButtonLink({
  to,
  text,
  onClick,
}: {
  to: string
  text: string
  onClick: () => void
}) {
  return (
    <Button
      kind="secondary"
      background="transparent"
      css={css(({ theme }) => ({
        border: `1px solid ${theme.color.neutral.border.default}`,
      }))}
      onClick={onClick}
    >
      <Link
        css={`
          &:hover {
            text-decoration: none;
          }
        `}
        to={to}
      >
        {text}
      </Link>
      <Icon icon="arrow_forward" />
    </Button>
  )
}

export const Flex = styled.div(({ theme }) => ({
  display: 'flex',
  gap: theme.spacing.s8,
}))

/**
 * A small gray dot done by using the Indicator component with a scale of 0.2
 */
export const GrayDot = () => (
  <Indicator
    css={`
      transform: scale(0.2);
    `}
    intent="secondary"
    position="middle-center"
  >
    x
  </Indicator>
)

export const MoreOrLessExpander = ({
  onClick,
  expanded = false,
}: {
  onClick: () => void
  expanded: boolean
}) => {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  return (
    <UnstyledButton
      css={css(({ theme }) => ({
        textDecoration: 'underline',
        color: theme.color.neutral.fg.subtle,
        '&:hover': {
          color: theme.color.neutral.fg.default,
        },
      }))}
      onClick={onClick}
    >
      {titleCase({
        text: expanded ? t('plainText.viewLess') : t('plainText.viewMore'),
        language,
      })}
    </UnstyledButton>
  )
}

// Constants for analytics tracking
export const TWIN_NAME = 'Twin Name'
export const TWIN_CATEGORY = 'Twin Category'
export const INSIGHT_NAME = 'Insight Name'
export const TICKET_NAME = 'Ticket Name'
export const TICKET_CATEGORY = 'Ticket Category'
export const INSIGHT_CATEGORY = 'Insight Category'
export const INSIGHT_PRIORITY = 'Insight Priority'
export const TICKET_PRIORITY = 'Ticket Priority'
export const OPEN_INSIGHTS = '# of Open Insights'
export const IN_PROGRESS_INSIGHTS = '# of In Progress Insights'
export const TICKETS_COUNT = '# of Tickets'

export const assetDetailPanelInsightsPanelKey = 'asset-detail-panel-insights'
export const assetDetailPanelTicketsPanelKey = 'asset-detail-panel-tickets'

/**
 * In order to have panels consistently collapsed on initial render,
 * as a business requirement, we need to override the default
 * localStorage behavior. This is only relevant for this page (Classic Viewer).
 * The logic goes as follows:
 * - only when there exists a key in storage that matches the key of
 *  `PanelGroup:active:${autoSaveId}`, and the value from storage is
 *   an empty array should the panel be collapsed.
 */
export const customLocalStorage: Storage = {
  ...localStorage,
  getItem: (key: string) =>
    [
      `PanelGroup:active:${assetDetailPanelInsightsPanelKey}`,
      `PanelGroup:active:${assetDetailPanelTicketsPanelKey}`,
    ].includes(key)
      ? '[]'
      : localStorage.getItem(key),
  setItem: (key: string, value: string) =>
    [
      `PanelGroup:active:${assetDetailPanelInsightsPanelKey}`,
      `PanelGroup:active:${assetDetailPanelTicketsPanelKey}`,
    ].includes(key)
      ? localStorage.setItem(key, '[]')
      : localStorage.setItem(key, value),
  length: localStorage.length,
  clear: localStorage.clear.bind(localStorage),
  key: localStorage.key.bind(localStorage),
  removeItem: localStorage.removeItem.bind(localStorage),
}
