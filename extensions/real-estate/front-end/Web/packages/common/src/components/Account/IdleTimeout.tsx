import { titleCase, WillowLogoWhite } from '@willow/common'
import { Button, EmptyState, Stack } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { useHistory } from 'react-router'
import routes from '../../../../platform/src/routes'
import FullSizeContainer from '../FullSizeContainer'

export default function IdleTimeout() {
  const history = useHistory()
  const {
    i18n: { language },
    t,
  } = useTranslation()

  return (
    <FullSizeContainer css={{ height: '100vh', width: '100vw' }}>
      <Stack align="center">
        <WillowLogoWhite width={140} />
        <EmptyState
          description={t('plainText.idleTimeout')}
          icon="info"
          primaryActions={
            // This needs to redirect to / (home) rather than directly to the login route,
            // otherwise it pauses on a black screen in between while the B2C login page loads.
            <Button onClick={() => history.push(routes.home)}>
              {titleCase({ language, text: t('labels.login') })}
            </Button>
          }
        />
      </Stack>
    </FullSizeContainer>
  )
}
