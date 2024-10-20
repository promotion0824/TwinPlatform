import { useTranslation } from 'react-i18next'
import styled from 'styled-components'

const InvalidMessage = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  alignItems: 'center',
  color: theme.color.neutral.fg.default,
  display: 'flex',
  flexDirection: 'column',
  height: '100%',
  justifyContent: 'center',
  width: '100%',
}))

const InvalidMessageHeading = styled.div(({ theme }) => ({
  ...theme.font.heading.xl,
}))

export default function MarketplaceInvalid() {
  const { t } = useTranslation()

  return (
    <InvalidMessage>
      <InvalidMessageHeading>
        {t('plainText.connectorsNotAvailable')}
      </InvalidMessageHeading>
      <div>{t('plainText.selectBuildingToAccessConnectors')}</div>
    </InvalidMessage>
  )
}
