import { useQuery, UseQueryOptions } from 'react-query'
import { qs } from '@willow/common'
import { api } from '@willow/ui'

type CheckHistoryResponse = Array<{
  id: string
  attachments: string[]
  checkId: string
  checkName: string
  checkType: string
  effectiveDate: string
  enteredBy: string
  floorCode: string
  inspectionId: string
  inspectionName: string
  inspectionRecordId: string
  notes: string
  numberValue: number
  status: string
  submittedDate: string
  submittedSiteLocalDate: string
  submittedUserId: string
  twinName: string
  typeValue: string
  zoneId: string
  zoneName: string
}>

export default function useGetCheckHistory(
  {
    startDate,
    checkId,
    endDate,
    siteId,
    inspectionId,
  }: {
    startDate: string
    checkId: string
    endDate: string
    siteId: string
    inspectionId: string
  },
  options: UseQueryOptions<CheckHistoryResponse>
) {
  return useQuery(
    ['checkHistory', checkId, startDate, endDate],
    async () => {
      const { data } = await api.get(
        qs.createUrl(`/inspections/${inspectionId}/checks/history`, {
          startDate,
          endDate,
          siteId,
          checkId,
        })
      )

      return data
    },
    options
  )
}
