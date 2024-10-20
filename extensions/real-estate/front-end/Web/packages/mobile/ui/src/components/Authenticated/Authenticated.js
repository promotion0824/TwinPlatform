import { Redirect } from 'react-router'
import { authService } from '@willow/common'
import { useUser } from 'providers'
import Message from 'components/Message/Message'
import styles from './Authenticated.css'

export default function Authenticated({
  authenticated = true,
  userRole,
  children,
}) {
  const user = useUser()

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
      <Message icon="error" className={styles.message}>
        You do not have permission to view this page
      </Message>
    )
  }

  return children ?? null
}
