import axios from 'axios'
import { getUrl } from '@willow/ui'
import { qs } from '@willow/common'

export type TwinDataQualitiesResponse = {
  attributePropertiesScore: number
  sensorsDefinedScore: number
  staticScore: number
  sensorsReadingDataScore: number
  connectivityScore: number
  overallScore: number
}

export type LocationDataQualitiesResponse = Array<{
  locationId: string // either a floorId or siteId
  dataQuality: {
    staticScore: number
    connectivityScore: number
    overallScore: number
  }
}>

export function getTwinDataQualities({
  siteId,
  twinId,
}): Promise<TwinDataQualitiesResponse> {
  const getTwinDataQualitiesUrl = getUrl(
    `/api/sites/${siteId}/twins/${twinId}/dataquality`
  )
  return axios.get(getTwinDataQualitiesUrl).then(({ data }) => data)
}

export function getSitesDataQualities({
  customerId,
  portfolioId,
}): Promise<LocationDataQualitiesResponse> {
  const getSitesDataQualitiesUrl = getUrl(
    `/api/customers/${customerId}/portfolios/${portfolioId}/sites/dataquality`
  )
  return axios.get(getSitesDataQualitiesUrl).then(({ data }) => data)
}

export function getFloorsDataQualities({
  siteId,
  systemId,
}: {
  siteId: string
  systemId?: string
}): Promise<LocationDataQualitiesResponse> {
  const getFloorsDataQualitiesUrl = getUrl(
    qs.createUrl(`/api/sites/${siteId}/floors/dataquality`, { systemId })
  )
  return axios.get(getFloorsDataQualitiesUrl).then(({ data }) => data)
}
