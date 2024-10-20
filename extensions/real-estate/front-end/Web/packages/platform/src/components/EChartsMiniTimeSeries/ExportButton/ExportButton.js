import {
  Button,
  downloadTextFile,
  Icon,
  useDateTime,
  useSnackbar,
  useUser,
} from '@willow/ui'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import exportCsv from 'services/LiveData/exportCsv.ts'
import { useTimeZoneInfo } from '../../TimeZoneSelect/useGetTimeZones'
import { useMiniTimeSeries } from '../MiniTimeSeriesContext'
import styles from './ExportButton.css'
import { useSelectedPoints } from '../../MiniTimeSeries/SelectedPointsContext'

export default function ExportButton({ timeZoneId }) {
  const dateTime = useDateTime()
  const snackbar = useSnackbar()
  const user = useUser()
  const miniTimeSeries = useMiniTimeSeries()
  const { t } = useTranslation()
  const { pointIds } = useSelectedPoints()
  const systemTimeZoneInfo = useTimeZoneInfo()

  const [isLoading, setIsLoading] = useState()

  if (!pointIds.length) {
    return null
  }

  const exportedTimeMachineCsvs = (
    user.options.exportedTimeMachineCsvs ?? []
  ).filter((time) => dateTime(dateTime.now()).differenceInHours(time) === 0)

  async function handleClick(e) {
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
        miniTimeSeries.times,
        miniTimeSeries.granularity,
        pointIds.map((sitePointIds) => {
          const [siteId, pointId] = sitePointIds.split('_')
          return { siteId, pointId }
        }),
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
    >
      <Icon size="large" icon="download" className={styles.icon} />
    </Button>
  )
}
