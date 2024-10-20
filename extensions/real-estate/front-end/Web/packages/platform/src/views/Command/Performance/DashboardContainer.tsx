import { styled, css } from 'twin.macro'
import { Panel, Message } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import { StyledDataPanel } from '../../Portfolio/Reports/Reports'
import { EmbedGroup } from '../../../components/Reports/ReportsLayout'

export default function DashboardContainer({
  isFetchOrAuthError,
  isFetchOrAuthLoading,
  isGetWidgetsSuccess,
  selectedReport,
  selectedDashboardReport,
  isAuthReportSuccess,
  children,
  ...rest
}: DashboardContainerParam) {
  const { t } = useTranslation()

  return (
    <Container overflow="hidden" {...rest}>
      {isFetchOrAuthError ? (
        <FullHeightMessage icon="error">
          {t('plainText.errorOccurred')}
        </FullHeightMessage>
      ) : (
        <StyledDataPanel isLoading={isFetchOrAuthLoading}>
          {isGetWidgetsSuccess &&
          !!selectedReport?.id &&
          !!selectedDashboardReport?.embedPath ? (
            isAuthReportSuccess && children
          ) : (
            <NoDashboardContainer>
              {t('plainText.NoDashboardsAvailableForThisLocation')}
            </NoDashboardContainer>
          )}
        </StyledDataPanel>
      )}
    </Container>
  )
}

const Container = styled(Panel)({
  height: '100%',
  border: 'none',
})

const FullHeightMessage = styled(Message)({
  height: '100%',
})

type DashboardContainerParam = {
  isFetchOrAuthError: boolean
  isFetchOrAuthLoading: boolean
  isGetWidgetsSuccess: boolean
  selectedReport?: { id: string }
  selectedDashboardReport?: EmbedGroup
  isAuthReportSuccess: boolean
  children: JSX.Element
}

const NoDashboardContainer = styled.div(
  ({ theme }) => css`
    display: flex;
    align-items: center;
    justify-content: center;
    height: 100%;
    color: ${theme.color.neutral.fg.default};
    background: ${theme.color.neutral.bg.base.default};
    ${theme.font.heading.xl}
  `
)
