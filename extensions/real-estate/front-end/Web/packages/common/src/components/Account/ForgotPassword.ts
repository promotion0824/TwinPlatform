import { authService } from '@willow/common'

export default function ForgotPassword({
  useConfig,
}: {
  useConfig: () => unknown
}) {
  const config = useConfig()
  authService.resetPassword(config)
  return null
}
