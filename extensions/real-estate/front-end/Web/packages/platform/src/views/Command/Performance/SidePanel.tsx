import { DashboardReportCategory } from '@willow/ui'
import { NavList, SidePanel } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { EmbedGroup } from '../../../components/Reports/ReportsLayout'
import { SigmaWidget } from '../../../services/Widgets/WidgetsService'

interface DashboardSidePanelProps {
  selectedDashboardReport?: EmbedGroup
  selectedCategory: DashboardReportCategory
  widgets: SigmaWidget[]
  onReportChange: (report: EmbedGroup, category: string | undefined) => void
}

export default function PerformanceSidePanel({
  selectedDashboardReport,
  selectedCategory,
  widgets,
  onReportChange,
  ...rest
}: DashboardSidePanelProps) {
  const { t } = useTranslation()

  return (
    <SidePanel css={{ width: 320 }} title={t('headers.dashboards')} {...rest}>
      <NavList>
        {widgets.map(({ metadata }) => (
          <NavList.Group title={metadata.category} key={metadata.category}>
            {metadata.embedGroup?.map((report) => (
              <NavList.Item
                label={report.name}
                key={report.name}
                active={
                  // report path could be duplicate, so need to match both category and report
                  selectedDashboardReport?.embedPath === report.embedPath &&
                  selectedCategory === metadata.category
                }
                onClick={() => {
                  onReportChange(report, metadata.category)
                }}
              />
            ))}
          </NavList.Group>
        ))}
      </NavList>
    </SidePanel>
  )
}
