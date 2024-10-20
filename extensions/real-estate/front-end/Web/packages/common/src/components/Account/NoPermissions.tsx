import { FullSizeContainer, isTouchDevice, WillowLogo } from '@willow/common'
import { Link, Text } from '@willow/ui'
import { Button, Stack } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'

import background from '../background_02.jpg'

const Background = styled.div({
  background: `url(${background})`,
  backgroundPosition: 'center',
  backgroundSize: 'cover',
  inset: 0,
  position: 'fixed',
})

const Rectangle = styled.div(({ theme }) => ({
  backgroundColor: 'rgb(0 0 0 / 60%)',
  borderRadius: theme.radius.r2,
  maxWidth: 400,
  padding: theme.spacing.s24,
}))

export default function NoPermissions({
  CountrySelect,
  handleLogout,
  isSingleTenant,
}: {
  // TODO: Deduplicate CountrySelect
  CountrySelect: () => React.ReactElement
  handleLogout: () => void
  isSingleTenant: boolean
}) {
  const isTouch = isTouchDevice()
  const { t } = useTranslation()

  return (
    <>
      <Background>
        <FullSizeContainer>
          <Rectangle>
            <Stack gap="s24">
              <WillowLogo />
              <Text size="large" color="white">
                {t('plainText.insufficientPrivilegesForPage')}
                {!isSingleTenant && ` ${t('plainText.currentRegionAccess')}`}
              </Text>
              {!isSingleTenant && <CountrySelect />}
              <Text size="large" color="white">
                {`${t('plainText.contactSysAdmin')} `}
                <Link
                  href="mailto:support@willowinc.com"
                  style={{ fontWeight: 'bold' }}
                >
                  support@willowinc.com
                </Link>
              </Text>
              <Button
                onClick={handleLogout}
                kind="negative"
                w={isTouch ? '100%' : 'fit-content'}
              >
                {t('plainText.logout')}
              </Button>
            </Stack>
          </Rectangle>
        </FullSizeContainer>
      </Background>
    </>
  )
}
