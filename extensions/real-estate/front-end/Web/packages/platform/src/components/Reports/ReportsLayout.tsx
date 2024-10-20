import { Table, Row, Head, Flex, DashboardReportCategory } from '@willow/ui'
import {
  PowerBiReportType,
  SigmaReportType,
} from '../../services/Widgets/WidgetsService'

export interface Position {
  position: number
  portfolioId?: string
  siteId?: string
  siteName?: string
}

export type ReportCategory = 'Comfort' | 'Energy' | 'N/A' | 'Twin Summary'

export type EmbedLocation = 'reportsTab' | 'dashboardsTab' | undefined

export type ReportType = SigmaReportType | PowerBiReportType

export type EmbedInfo = {
  name: string
  embedPath: string
  tenantFilter?: boolean
  disableDatePicker?: boolean
}
export interface EmbedGroup extends EmbedInfo {
  order: number
  widgetId?: string
}

interface BaseConfig {
  id: string
  positions: Position[]
  type: ReportType
}

export interface ReportConfig extends BaseConfig {
  metadata: {
    category: ReportCategory
    embedLocation: EmbedLocation
    embedPath: string
    name: string
  }
}

export interface DashboardConfig extends BaseConfig {
  metadata: {
    category: DashboardReportCategory
    embedLocation: EmbedLocation
    embedGroup: EmbedGroup[]
  }
}

export default function ReportsLayout({
  reports,
  selectedReport,
  HeadCells,
  BodyCells,
  onRowClick,
}: {
  reports: ReportConfig[] | DashboardConfig[]
  selectedReport: ReportConfig | DashboardConfig[]
  HeadCells: React.ElementType
  BodyCells: React.ElementType
  onRowClick: (report: ReportConfig) => void
}) {
  return (
    <Flex fill="content">
      <Table
        items={reports}
        defaultSort={['priority asc', 'occurredDate desc']}
      >
        <Head>
          <Row>
            <HeadCells />
          </Row>
        </Head>
        {reports?.map((report) => (
          <BodyCells
            key={report.id}
            report={report}
            selectedReport={selectedReport}
            onRowClick={onRowClick}
          />
        ))}
      </Table>
    </Flex>
  )
}
