import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useApi, useSnackbar, useUserAgent, Button } from '@willow/ui'
import styles from './DownloadScanDataButton.css'

export default function DownloadScanDataButton({
  siteId,
  connectorId,
  scanId,
}) {
  const api = useApi()
  const snackbar = useSnackbar()
  const userAgent = useUserAgent()
  const { t } = useTranslation()

  const [isLoading, setIsLoading] = useState(false)

  async function handleClick() {
    setIsLoading(true)
    try {
      const blob = await api.get(
        `/api/sites/${siteId}/connectors/${connectorId}/scans/${scanId}/content`,
        { responseType: 'blob' }
      )
      const link = document.createElement('a')
      link.href = window.URL.createObjectURL(blob)
      if (userAgent.isIpad) {
        link.target = '_blank'
      } else {
        link.download = 'scan.zip'
      }
      link.click()
    } catch (err) {
      snackbar.show(`${t('plainText.errorOccurred')}${err}`)
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <Button
      icon={!isLoading ? 'exportIcon' : 'none'}
      disabled={isLoading}
      loading={isLoading}
      className={styles.downloadButton}
      onClick={handleClick}
    />
  )
}
