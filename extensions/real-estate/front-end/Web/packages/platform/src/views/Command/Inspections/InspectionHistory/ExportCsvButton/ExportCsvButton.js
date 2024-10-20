import { qs } from '@willow/common'
import { downloadTextFile, useAnalytics, api, useSnackbar } from '@willow/ui'
import { Button, Icon } from '@willowinc/ui'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'

export default function ExportCsvButton({ times, inspection, check, ...rest }) {
  const analytics = useAnalytics()
  const params = useParams()
  const snackbar = useSnackbar()
  const { t } = useTranslation()

  const [isLoading, setIsLoading] = useState()

  async function handleClick() {
    analytics.track('Export inspection check history csv', {
      inspection: inspection.name,
      check: check?.name ?? 'all',
      times,
    })

    const exportUrl = qs.createUrl(
      `/inspections/${params.inspectionId}/checks/history/export`,
      {
        siteId: inspection.siteId,
        checkId: params.checkId !== 'all' ? params.checkId : undefined,
        startDate: times[0],
        endDate: times[1],
        timezoneOffset: new Date().getTimezoneOffset(),
      }
    )

    setIsLoading(true)

    try {
      const response = await api.get(exportUrl, {
        responseType: 'text',
      })

      downloadTextFile(
        response.data, // File Data
        response?.headers?.['x-file-name'] || 'InspectionCheckHistory.csv' // File Name
      )
      setIsLoading(false)
    } catch (err) {
      setIsLoading(false)
      snackbar.show(t('plainText.errorOccurred'))
    }
  }

  return (
    <Button
      kind="secondary"
      loading={isLoading}
      background="transparent"
      prefix={<Icon icon="download" size={24} />}
      onClick={handleClick}
      {...rest}
    />
  )
}
