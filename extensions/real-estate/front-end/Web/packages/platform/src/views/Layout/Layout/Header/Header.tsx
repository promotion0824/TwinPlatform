import {
  isWillowUser,
  Launcher,
  useGetMyPreferences,
  WillowLogoWhite,
} from '@willow/common'
import ChatApp from '@willow/common/copilot/ChatApp/ChatApp'
import { useChat } from '@willow/common/copilot/ChatApp/ChatContext'
import {
  lastDateTimeOpenNotificationBellKey,
  useGetNotificationsStats,
} from '@willow/common/notifications'
import {
  NotificationFilterOperator,
  NotificationStatus,
} from '@willow/common/notifications/types'
import {
  getContainmentHelper,
  getSiteIdFromUrl,
  Link,
  useConfig,
  useFeatureFlag,
  useUser,
} from '@willow/ui'
import { Drawer, IconButton, useDisclosure } from '@willowinc/ui'
import { useState } from 'react'
import { useLocation } from 'react-router'
import { css } from 'styled-components'
import { styled } from 'twin.macro'
import { useSites } from '../../../../providers'
import { useLayout } from '../LayoutContext'
import LayoutSidebar from '../LayoutSidebar'
import ContactUsForm from './ContactUs/ContactUsForm'
import MainMenu from './MainMenu'
import NotificationsMenu from './NotificationsMenu/NotificationsMenu'
import UserMenu from './UserMenu/UserMenu'
import { useHomeUrl, useSelectedSiteId } from './utils'

const { ContainmentWrapper, getContainerQuery } = getContainmentHelper()

const CommandTitleBar = styled.div(({ theme }) => ({
  alignItems: 'center',
  backgroundColor: theme.color.neutral.bg.panel.default,
  borderBottom: `1px solid ${theme.color.neutral.border.default}`,
  display: 'flex',
  gap: theme.spacing.s16,
  height: '52px',
  padding: `0 ${theme.spacing.s16}`,
}))

const CommandTitleBarContent = styled.div({
  marginRight: 'auto',
})

const CustomerName = styled.div(({ theme }) => ({
  ...theme.font.heading.md,
  color: theme.color.neutral.fg.default,
}))

const StyledLink = styled(Link)(({ theme }) => ({
  '&:focus-visible': {
    outline: `1px solid ${theme.color.state.focus.border}`,
  },
}))

const UtilitiesSection = styled.div(({ theme }) => ({
  alignItems: 'center',
  display: 'flex',
  gap: theme.spacing.s8,
}))

/**
 * Header component displays the common header on top of layout
 */
export default function Header() {
  const layout = useLayout()
  const user = useUser()
  const sites = useSites()
  const featureFlags = useFeatureFlag()
  const chatContext = useChat()
  const config = useConfig()
  const location = useLocation()
  const siteIdFromUrl = getSiteIdFromUrl(location.pathname)
  const selectedSiteId = siteIdFromUrl
  const [isContactFormOpen, setIsContactFormOpen] = useState(false)

  const userSelectedSiteId = useSelectedSiteId()
  const homeUrl = useHomeUrl(userSelectedSiteId)

  const [isMainMenuOpen, { close: closeMainMenu, open: openMainMenu }] =
    useDisclosure()

  const [
    isNotificationsMenuOpen,
    { close: closeNotificationsMenu, open: openNotificationsMenu },
  ] = useDisclosure()

  const [sidebarOpened, { close: closeSidebar, open: openSidebar }] =
    useDisclosure()

  const upToDateUserPreferencesQuery = useGetMyPreferences<string>()
  const lastDateTimeOpenNotificationBell =
    upToDateUserPreferencesQuery.data?.profile?.[
      lastDateTimeOpenNotificationBellKey
    ]
  const notificationsStats = useGetNotificationsStats(
    [
      {
        field: 'notification.userId',
        operator: NotificationFilterOperator.EqualsLiteral,
        value: user.id,
      },
      ...(lastDateTimeOpenNotificationBell
        ? [
            {
              field: 'notification.createdDateTime',
              operator: NotificationFilterOperator.GreaterThan,
              value: lastDateTimeOpenNotificationBell,
            },
          ]
        : []),
    ],
    {
      enabled: upToDateUserPreferencesQuery.status === 'success',
    }
  )

  return (
    <ContainmentWrapper css={{ height: 52 }}>
      <HeaderWrapper>
        {featureFlags.hasFeatureToggle('globalSidebar') ? (
          <Drawer
            header={<WillowLogoWhite height={18} />}
            onClose={closeSidebar}
            opened={sidebarOpened}
            position="left"
            size={257}
          >
            <LayoutSidebar onLinkClick={closeSidebar} withFooter={false} />
          </Drawer>
        ) : (
          <MainMenu
            isOpen={isMainMenuOpen}
            onClose={closeMainMenu}
            user={user}
            layout={layout}
            sites={sites}
            featureFlags={featureFlags}
            config={config}
          />
        )}

        <ContactUsForm
          isFormOpen={isContactFormOpen}
          onClose={() => setIsContactFormOpen(false)}
          siteId={selectedSiteId}
        />

        <CommandTitleBar>
          {featureFlags.hasFeatureToggle('globalSidebar') ? (
            <IconButton
              background="transparent"
              css={css(({ theme }) => ({
                [getContainerQuery(`min-width: ${theme.breakpoints.mobile}`)]: {
                  display: 'none',
                },
              }))}
              icon="menu"
              kind="secondary"
              onClick={openSidebar}
            />
          ) : (
            <IconButton
              background="transparent"
              data-testid="menu-title"
              data-segment="Hamburger Menu Clicked"
              icon="menu"
              kind="secondary"
              onClick={openMainMenu}
            />
          )}

          <StyledLink data-segment="Willow Home Button Clicked" to={homeUrl}>
            <WillowLogoWhite height={18} />
          </StyledLink>

          <CommandTitleBarContent ref={layout.headerRef} />

          {user.customer?.name && isWillowUser(user.email) && (
            <CustomerName data-testid="customer-name">
              {user.customer.name}
            </CustomerName>
          )}

          <UtilitiesSection>
            {featureFlags.hasFeatureToggle('copilot') && (
              <>
                <Launcher
                  isActive={chatContext.isActive}
                  copilotSessionId={chatContext.copilotSessionId}
                />
                {chatContext.isActive && <ChatApp {...chatContext} />}
              </>
            )}

            <IconButton
              background="transparent"
              icon="help"
              kind="secondary"
              onClick={() => setIsContactFormOpen(true)}
              css={css(({ theme }) => ({
                '& *': {
                  fontSize: theme.spacing.s24,
                },
              }))}
            />

            {featureFlags?.hasFeatureToggle('isNotificationEnabled') &&
              featureFlags?.hasFeatureToggle('scopeSelector') && (
                <NotificationsMenu
                  onOpen={() => {
                    user.saveOptions(
                      lastDateTimeOpenNotificationBellKey,
                      new Date()
                    )
                    upToDateUserPreferencesQuery.refetch()
                  }}
                  onChange={
                    isNotificationsMenuOpen
                      ? closeNotificationsMenu
                      : openNotificationsMenu
                  }
                  isOpened={isNotificationsMenuOpen}
                  newCountSinceLastOpen={
                    notificationsStats.data?.find(
                      ({ state }) => state === NotificationStatus.New
                    )?.count
                  }
                />
              )}
          </UtilitiesSection>

          <UserMenu user={user} />
        </CommandTitleBar>
      </HeaderWrapper>
    </ContainmentWrapper>
  )
}

const HeaderWrapper = styled.div(({ theme }) => ({
  backgroundColor: theme.color.neutral.bg.panel.default,
  overflow: 'hidden',
}))
