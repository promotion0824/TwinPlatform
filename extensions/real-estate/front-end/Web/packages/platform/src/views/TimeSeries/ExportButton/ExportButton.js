import {
  Button,
  downloadTextFile,
  Flex,
  Icon,
  useAnalytics,
  useDateTime,
  useSnackbar,
  useUser,
} from '@willow/ui'
import { useTimeSeries } from 'components/TimeSeries/TimeSeriesContext'
import { useTimeZoneInfo } from 'components/TimeZoneSelect/useGetTimeZones.ts'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import exportCsv from 'services/LiveData/exportCsv.ts'
import styles from './ExportButton.css'

export default function ExportButton({ timeZoneId }) {
  const dateTime = useDateTime()
  const snackbar = useSnackbar()
  const user = useUser()
  const timeSeries = useTimeSeries()
  const analytics = useAnalytics()
  const { t } = useTranslation()
  const systemTimeZoneInfo = useTimeZoneInfo()

  const [isLoading, setIsLoading] = useState()

  if (timeSeries.points.length === 0) {
    return null
  }

  const exportedTimeMachineCsvs = (
    user.options.exportedTimeMachineCsvs ?? []
  ).filter((time) => dateTime(dateTime.now()).differenceInHours(time) === 0)

  async function handleClick(e) {
    analytics.track('Export time series csv')
    const now = dateTime.now().format()

    const nextExportedTimeMachineCsvs = [
      ...exportedTimeMachineCsvs.filter(
        (time) => dateTime(now).differenceInHours(time) === 0
      ),
      now,
    ]

    if (nextExportedTimeMachineCsvs.length > 10) {
      snackbar.show(t('plainText.limitPerHour'))

      e.preventDefault()
      return
    }

    user.saveOptions('exportedTimeMachineCsvs', nextExportedTimeMachineCsvs)

    setIsLoading(true)

    try {
      const text = await exportCsv(
        timeSeries.times,
        timeSeries.granularity,
        timeSeries.points.map((point) => ({
          pointId: point.pointId,
          siteId: point.siteId,
        })),
        timeZoneId ?? systemTimeZoneInfo?.id
      )

      downloadTextFile(text, 'timeSeries.csv')
      setIsLoading(false)
    } catch (err) {
      setIsLoading(false)

      snackbar.show(t('plainText.errorOccurred'))
    }
  }

  return (
    <Button
      disabled={isLoading}
      loading={isLoading}
      className={styles.exportButton}
      onClick={handleClick}
      data-segment="Export time series csv"
      height="medium"
      padding="0"
    >
      <Flex horizontal fill="header" align="middle center" width="100%">
        <Icon
          icon={!isLoading ? 'download' : 'none'}
          size="large"
          className={styles.icon}
        />
      </Flex>
    </Button>
  )
}
