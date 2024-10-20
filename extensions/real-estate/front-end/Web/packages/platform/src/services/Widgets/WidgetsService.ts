import { getUrl } from '@willow/ui'
import axios from 'axios'
import { DashboardConfigForm } from '../../components/Reports/DashboardModal/DashboardConfigForm'
import {
  EmbedGroup,
  EmbedLocation,
} from '../../components/Reports/ReportsLayout'

type SigmaMetadata = {
  embedPath?: string
  name?: string
  allowExport?: string
  embedLocation?: EmbedLocation
  category?: string
  embedGroup?: EmbedGroup[]
}

type PowerBiMetadata = {
  groupId: string
  reportId: string
  name: string
  allowExport?: string
  [key: string]: string | undefined
}

export type SigmaReportType = 'sigmaReport'
export type PowerBiReportType = 'powerBIReport'

/**
 * please refer to schema of GET /sites/{siteId}/dashboard on
 * https://wil-uat-plt-aue1-portalxl.azurewebsites.net/swagger/index.html
 * for most update-to-date Widget type structure
 */
interface BasicWidget {
  id: string
  positions?: Array<{
    siteId: string
    siteName: string
    portfolioId?: string
    position: number
  }>
}

export interface SigmaWidget extends BasicWidget {
  type: SigmaReportType
  metadata: SigmaMetadata
}

interface PowerBiWidget extends BasicWidget {
  type: PowerBiReportType
  metadata: PowerBiMetadata
}

export type Widget = SigmaWidget | PowerBiWidget

export type WidgetsResponse = {
  widgets: Array<Widget>
}

export function getWidgets(
  baseUrl: string,
  id: string
): Promise<WidgetsResponse> {
  const getWidgetsUrl = getUrl(`${baseUrl}/${id}/dashboard`)
  return axios.get(getWidgetsUrl).then(({ data }) => data)
}

export function postWidget(formData: DashboardConfigForm) {
  const postWidgetUrl = getUrl('/api/dashboard')
  return axios.post(postWidgetUrl, formData).then(({ data }) => data)
}

export function putWidget(id: string, formData: DashboardConfigForm) {
  const putWidgetUrl = getUrl(`/api/dashboard/${id}`)
  return axios.put(putWidgetUrl, formData).then(({ data }) => data)
}

export function deleteWidget(id: string) {
  const deleteWidgetUrl = getUrl(`/api/dashboard/${id}?resetLinked=true`)
  return axios.delete(deleteWidgetUrl).then(({ data }) => data)
}
