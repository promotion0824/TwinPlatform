import axios from 'axios'
import { getUrl } from '@willow/ui'
import { WidgetsResponse } from '../Widgets/WidgetsService'

export function getReportsConfig(
  portfolioId: string
): Promise<WidgetsResponse> {
  const getReportsConfigUrl = getUrl(
    `/api/portfolios/${portfolioId}/dashboard?includeSiteWidgets=true`
  )
  return axios.get(getReportsConfigUrl).then(({ data }) => data)
}
