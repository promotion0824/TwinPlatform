import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useApi, useFetchRefresh, useSnackbar, Button } from '@willow/ui'
import styles from './StopScanButton.css'

export default function StopScanButton({ siteId, connectorId, scanId }) {
  const api = useApi()
  const fetchRefresh = useFetchRefresh()
  const snackbar = useSnackbar()
  const { t } = useTranslation()

  const [isLoading, setIsLoading] = useState(false)

  async function handleClick() {
    setIsLoading(true)
    try {
      await api.post(
        `/api/sites/${siteId}/connectors/${connectorId}/scans/${scanId}/stop`
      )
      fetchRefresh('connectorScans')
    } catch (err) {
      snackbar.show(t('plainText.errorOccurred'))
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <Button
      icon={!isLoading ? 'close' : 'none'}
      disabled={isLoading}
      loading={isLoading}
      className={styles.stopButton}
      onClick={handleClick}
    />
  )
}
