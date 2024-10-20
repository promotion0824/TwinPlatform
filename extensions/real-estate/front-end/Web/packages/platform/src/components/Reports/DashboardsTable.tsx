import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { Body, Cell, PORTFOLIO, Row } from '@willow/ui'
import { styled } from 'twin.macro'
import { Fragment } from 'react'
import ReportsLayout, {
  DashboardConfig,
  Position,
  ReportConfig,
} from './ReportsLayout'

const HeadCells = () => {
  const { t } = useTranslation()
  return (
    <>
      <Cell>KPI{t('labels.category')}</Cell>
      <Cell>{t('labels.name')}</Cell>
      <Cell>{t('labels.reportType')}</Cell>
      <Cell>
        {t('headers.portfolio')}/{t('labels.site')}
      </Cell>
      <Cell>{t('labels.embedPath')}</Cell>
    </>
  )
}

const StyledGroupCell = styled(Cell)({
  background: '#252525',
})
const StyledGroupNameCell = styled(StyledGroupCell)({
  textDecoration: 'underline',
  color: '#fafafa',
})

const StyledSubgroupCell = styled(Cell)({
  background: '#2b2b2b',
  cursor: 'pointer',
})
const StyledSubgroupNameCell = styled(StyledGroupCell)({
  background: '#2b2b2b',
  paddingLeft: 'var(--padding-large)',
})

const SITE = 'Site'

function getPortfolioSiteType(positions: Position[]) {
  if (positions.length === 0) return ''

  const [position] = positions
  return position?.portfolioId ? PORTFOLIO : SITE
}

const BodyCells = ({
  report,
  selectedReport,
  onRowClick,
}: {
  report: DashboardConfig
  selectedReport: DashboardConfig
  onRowClick: (data: DashboardConfig) => void
}) => {
  const { metadata, type, positions = [] } = report
  const { category, embedGroup } = metadata
  const portfolioSiteType = getPortfolioSiteType(positions)
  const handleRowClick = () => {
    onRowClick(report)
  }
  const SubGroups = [...embedGroup]
    .sort((a, b) => a.order - b.order)
    .map(({ name, embedPath }) => (
      <Fragment key={name}>
        <StyledSubgroupNameCell onClick={handleRowClick}>
          {_.capitalize(category)}
        </StyledSubgroupNameCell>
        <StyledSubgroupCell onClick={handleRowClick}>{name}</StyledSubgroupCell>
        <StyledSubgroupCell onClick={handleRowClick}>{type}</StyledSubgroupCell>
        <StyledSubgroupCell onClick={handleRowClick}>
          {portfolioSiteType}
        </StyledSubgroupCell>
        <StyledSubgroupCell onClick={handleRowClick}>
          {embedPath}
        </StyledSubgroupCell>
      </Fragment>
    ))
  return (
    <Body>
      <Row
        data-segment="Report Selected"
        page="Reports Page"
        selected={selectedReport?.id === report.id}
        onClick={handleRowClick}
      >
        <StyledGroupNameCell>{_.capitalize(category)}</StyledGroupNameCell>
        <StyledGroupCell />
        <StyledGroupCell>{type}</StyledGroupCell>
        <StyledGroupCell>{portfolioSiteType}</StyledGroupCell>
        <StyledGroupCell />
      </Row>
      <Row>{SubGroups}</Row>
    </Body>
  )
}

export default function DashboardsTable({
  reports,
  selectedReport,
  onRowClick,
}: {
  reports: ReportConfig[]
  selectedReport: ReportConfig
  onRowClick: (report: ReportConfig) => void
}) {
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
