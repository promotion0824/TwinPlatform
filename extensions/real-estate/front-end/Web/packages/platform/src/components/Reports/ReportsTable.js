import { useTranslation } from 'react-i18next'
import { Body, Cell, Row } from '@willow/ui'
import styles from './Reports.css'
import ReportsLayout from './ReportsLayout'

const HeadCells = () => {
  const { t } = useTranslation()

  return (
    <>
      <Cell>{t('plainText.reportName')}</Cell>
      <Cell>{t('labels.site')}</Cell>
      <Cell>{t('labels.category')}</Cell>
      <Cell>{t('labels.reportType')}</Cell>
      <Cell>{t('labels.embedPath')}</Cell>
      <Cell>{t('labels.groupId')}</Cell>
      <Cell>{t('labels.reportId')}</Cell>
    </>
  )
}
const BodyCells = ({ report, onRowClick, selectedReport }) => {
  const { metadata, type, positions = [] } = report

  return (
    <Body>
      <Row
        color={`var(--priority${report.priority})`}
        data-segment="Report Selected"
        page="Reports Page"
        selected={selectedReport?.id === report.id}
        onClick={() => onRowClick(report)}
      >
        <Cell className={styles.reportName}>{metadata.name}</Cell>
        <Cell>{positions?.map((site) => site.siteName).join(', ')}</Cell>
        <Cell>{metadata.category}</Cell>
        <Cell>{type}</Cell>
        <Cell>{metadata.embedPath}</Cell>
        <Cell>{metadata.groupId}</Cell>
        <Cell>{metadata.reportId}</Cell>
      </Row>
    </Body>
  )
}

function ReportsTable({ reports, selectedReport, onRowClick }) {
  return (
    <ReportsLayout
      reports={reports}
      selectedReport={selectedReport}
      onRowClick={onRowClick}
      HeadCells={HeadCells}
      BodyCells={BodyCells}
    />
  )
}

export default ReportsTable
