import { EmbedGroup } from '../../../components/Reports/ReportsLayout'
import { AuthenticatedReport } from '../../../services/Widgets/AuthWidgetService'
import { SigmaWidget } from '../../../services/Widgets/WidgetsService'
import ReportContent from '../../Portfolio/Reports/ReportContent'
import DashboardContainer from './DashboardContainer'

export default function DashboardContent({
  selectedReport,
  selectedDashboardReport,
  isFetchOrAuthError,
  isFetchOrAuthLoading,
  isGetWidgetsSuccess,
  isAuthReportSuccess,
  authenticatedReport,
  ...rest
}: {
  selectedReport?: SigmaWidget
  selectedDashboardReport?: EmbedGroup
  isFetchOrAuthError: boolean
  isFetchOrAuthLoading: boolean
  isGetWidgetsSuccess: boolean
  isAuthReportSuccess: boolean
  authenticatedReport?: AuthenticatedReport
}) {
  return (
    <DashboardContainer
      isFetchOrAuthError={isFetchOrAuthError}
      isFetchOrAuthLoading={isFetchOrAuthLoading}
      isGetWidgetsSuccess={isGetWidgetsSuccess}
      selectedReport={selectedReport}
      selectedDashboardReport={selectedDashboardReport}
      isAuthReportSuccess={isAuthReportSuccess}
      {...rest}
    >
      <ReportContent
        selectedReport={selectedReport}
        authenticatedReport={authenticatedReport}
      />
    </DashboardContainer>
  )
}
