import { NotFound, Panel, PowerBIReport } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function ReportContent({ selectedReport }) {
  const { t } = useTranslation()
  return (
    <Panel fill="header">
      {selectedReport != null && (
        <PowerBIReport
          groupId={selectedReport.metadata.groupId}
          reportId={selectedReport.metadata.reportId}
        />
      )}
      {selectedReport == null && <NotFound>{t('plainText.noReport')}</NotFound>}
    </Panel>
  )
}
