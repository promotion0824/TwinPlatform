import { titleCase } from '@willow/common'
import { api } from '@willow/ui'
import {
  Avatar,
  Icon,
  Menu,
  UnstyledButton,
  useDisclosure,
} from '@willowinc/ui'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useHistory } from 'react-router-dom'
import styled, { css } from 'styled-components'
import ResetProfileModal from './ResetProfileModal'
import type { User } from './types'
import UserPreferencesDrawer from './UserPreferencesDrawer'

function getInitials(user: User) {
  const firstName =
    user.name != null ? user.name.split(' ')[0] ?? '' : user.firstName ?? ''
  const lastName =
    user.name != null ? user.name.split(' ')[1] ?? '' : user.lastName ?? ''
  const firstInitial = firstName?.[0] ?? ''
  const lastInitial = lastName?.[0] ?? ''
  return `${firstInitial}${lastInitial}`
}

const StyledMenuTarget = styled(Menu.Target)({
  cursor: 'pointer',
})

export default function UserMenu({ user }: { user: User }) {
  const history = useHistory()
  const {
    i18n: { language },
    t,
  } = useTranslation()

  const [loggingOut, setLoggingOut] = useState(false)

  const [
    resetProfileModalOpened,
    { close: closeResetProfileModal, open: openResetProfileModal },
  ] = useDisclosure()

  const [
    userPreferencesDrawerOpened,
    { close: closeUserPreferencesDrawer, open: openUserPreferencesDrawer },
  ] = useDisclosure()

  return (
    <>
      <Menu position="bottom-end">
        <StyledMenuTarget>
          <UnstyledButton
            css={css(({ theme }) => ({
              '&:focus-visible': {
                outline: `1px solid ${theme.color.state.focus.border}`,
              },
            }))}
            data-testid="user-menu"
          >
            <Avatar color="purple">{getInitials(user)}</Avatar>
          </UnstyledButton>
        </StyledMenuTarget>
        <Menu.Dropdown>
          <Menu.Item
            onClick={() => window.open('https://support.willowinc.com')}
            prefix={<Icon icon="link" />}
          >
            {t('plainText.helpSupport')}
          </Menu.Item>
          <Menu.Item
            onClick={() =>
              window.open(
                'https://www.willowinc.com/posts/data-privacy-policy/'
              )
            }
            prefix={<Icon icon="link" />}
          >
            {titleCase({ language, text: t('plainText.privacyPolicy') })}
          </Menu.Item>
          <Menu.Divider />
          <Menu.Item
            onClick={openUserPreferencesDrawer}
            prefix={<Icon icon="person" />}
          >
            {titleCase({ language, text: t('labels.userPreferences') })}
          </Menu.Item>
          <Menu.Item
            intent="negative"
            onClick={() => openResetProfileModal()}
            prefix={<Icon icon="history" />}
          >
            {titleCase({ language, text: t('headers.resetProfile') })}
          </Menu.Item>
          <Menu.Divider />
          <Menu.Item
            closeMenuOnClick={false}
            disabled={loggingOut}
            onClick={async () => {
              setLoggingOut(true)
              await api.post('/me/signout')
              user.logout()
              sessionStorage.clear()
              history.push('/')
            }}
            prefix={<Icon icon="logout" />}
          >
            {t('plainText.logoutVerb')}
          </Menu.Item>
        </Menu.Dropdown>
      </Menu>

      <ResetProfileModal
        onClose={closeResetProfileModal}
        opened={resetProfileModalOpened}
        user={user}
      />

      <UserPreferencesDrawer
        onClose={closeUserPreferencesDrawer}
        opened={userPreferencesDrawerOpened}
        user={user}
      />
    </>
  )
}
