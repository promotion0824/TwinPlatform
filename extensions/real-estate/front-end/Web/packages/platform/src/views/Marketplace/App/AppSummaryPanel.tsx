import { titleCase } from '@willow/common'
import { PanelContent } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import styled from 'styled-components'
import MarketplaceAppLogo from '../MarketplaceAppLogo'
import MarketplaceCategoryBadges from '../MarketplaceCategoryBadges'
import { MarketplaceApp } from '../types'

const AppSummaryPanelContainer = styled(PanelContent)(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s16,
  padding: theme.spacing.s16,
}))

const HorizontalSection = styled.div({
  display: 'flex',
  justifyContent: 'space-between',
})

const VerticalSection = styled.div(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s4,
}))

const SummaryBody = styled.div(({ theme }) => ({
  display: 'flex',
  flexDirection: 'column',
  gap: theme.spacing.s8,
}))

const SectionContent = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.default,
}))

const SectionHeading = styled.div(({ theme }) => ({
  ...theme.font.body.md.regular,
  color: theme.color.neutral.fg.muted,
}))

export default function AppSummaryPanel({ app }: { app: MarketplaceApp }) {
  const {
    i18n: { language },
    t,
  } = useTranslation()

  return (
    <AppSummaryPanelContainer>
      <MarketplaceAppLogo app={app} />

      <SummaryBody>
        <HorizontalSection>
          <SectionHeading>
            {titleCase({ language, text: t('headers.version') })}
          </SectionHeading>
          <SectionContent>{app.version}</SectionContent>
        </HorizontalSection>
        <VerticalSection>
          <SectionHeading>{t('headers.overview')}</SectionHeading>
          <SectionContent>{app.description}</SectionContent>
        </VerticalSection>
      </SummaryBody>
      <MarketplaceCategoryBadges app={app} />
    </AppSummaryPanelContainer>
  )
}
