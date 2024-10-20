import { useAnalytics, useUser } from '@willow/ui'
import { Button, Icon } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { css } from 'twin.macro'
import { useChat } from '../ChatApp/ChatContext'

export default function Launcher({
  isActive,
  copilotSessionId,
}: {
  isActive: boolean
  copilotSessionId: string
}) {
  const { t } = useTranslation()
  const { onToggle } = useChat()
  const analytics = useAnalytics()
  const user = useUser()

  return (
    <Button
      kind="secondary"
      prefix={
        <Icon
          icon="forum"
          css={css(({ theme }) => ({
            color: isActive
              ? theme.color.intent.primary.fg.activated
              : 'inherit',
          }))}
        />
      }
      onClick={() => {
        // Only track opening action.
        if (!isActive) {
          analytics.track('Copilot - Button Clicked', {
            sessionID: copilotSessionId,
            userEmail: user.email,
          })
        }

        onToggle?.(!isActive)
      }}
    >
      {t('plainText.copilot')}
    </Button>
  )
}
