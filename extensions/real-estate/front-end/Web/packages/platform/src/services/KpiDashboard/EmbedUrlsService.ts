import {
  DatePickerBusinessRangeOptions,
  DatePickerDayRangeOptions,
  getUrl,
} from '@willow/ui'
import axios from 'axios'

export type EmbedUrlsResponse = Array<{
  name: string
  url: string
  reportId?: string
}>

export type GetEmbedUrlsParams = {
  start: string
  end: string
  scopeId?: string
  customerId: string
  siteIds: string[]
  url: string
  reportId?: string
  reportName?: string
  selectedDayRange: DatePickerDayRangeOptions
  selectedBusinessHourRange: DatePickerBusinessRangeOptions
}

export function getEmbedUrls({
  start,
  end,
  scopeId,
  customerId,
  selectedDayRange,
  selectedBusinessHourRange,
  siteIds,
  url,
  reportId,
  reportName,
}: GetEmbedUrlsParams): Promise<EmbedUrlsResponse> {
  const getPerformanceUrl = getUrl(url)
  // this is a post request because we need to send a site id array with possibly long length,
  // hence, Backend choose to set this up as a post request to include site id array in the body.
  // (Note Body will be ignored by Get request)
  return axios
    .post(getPerformanceUrl, {
      siteIds,
      start,
      end,
      scopeId,
      customerId,
      selectedDayRange: [selectedDayRange],
      selectedBusinessHourRange: [selectedBusinessHourRange],
      // optional to include reportId and reportName in body
      ...(reportId ? { reportId } : {}),
      ...(reportName ? { reportName } : {}),
    })
    .then(({ data }) => data)
}
