import { Redirect } from 'react-router'
import { useUser } from '@willow/ui'
import Flex from 'components/Flex/Flex'
import Message from 'components/Message/Message'
import { useTranslation } from 'react-i18next'
import { authService } from '@willow/common'

export default function Authenticated({
  authenticated = true,
  userRole,
  children,
}) {
  const user = useUser()
  const { t } = useTranslation()

  if (!authenticated) {
    if (user.isAuthenticated) {
      return <Redirect to="/" />
    }

    return children ?? null
  }

  if (!user.isAuthenticated) {
    return <Redirect to={authService.getLoginPath()} />
  }

  if (userRole != null && !user.roles.includes(userRole)) {
    return (
      <Flex position="fixed">
        <Message icon="error">{t('plainText.noPermission')}</Message>
      </Flex>
    )
  }

  return children ?? null
}
