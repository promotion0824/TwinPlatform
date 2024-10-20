import { Button, downloadTextFile, useApi, useSnackbar } from '@willow/ui'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import styles from './ExportConnectorLogButton.css'

export default function ExportConnectorLogButton({
  siteId,
  connectorId,
  logId,
}) {
  const api = useApi()
  const snackbar = useSnackbar()
  const { t } = useTranslation()

  const [isLoading, setIsLoading] = useState(false)

  async function handleClick() {
    setIsLoading(true)
    try {
      const text = await api.get(
        `/api/sites/${siteId}/connectors/${connectorId}/logs/${logId}/content`,
        { responseType: 'text' }
      )
      downloadTextFile(text, 'log.csv')
    } catch {
      snackbar.show(t('plainText.errorOccurred'))
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <Button
      icon={!isLoading ? 'exportIcon' : 'none'}
      disabled={isLoading}
      loading={isLoading}
      className={styles.exportButton}
      onClick={handleClick}
      data-segment="Export time series csv"
    />
  )
}
