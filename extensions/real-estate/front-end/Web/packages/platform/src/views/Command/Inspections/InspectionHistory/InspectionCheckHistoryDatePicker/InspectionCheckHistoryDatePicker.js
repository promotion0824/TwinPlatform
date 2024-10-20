import { useDateTime, DatePicker, DatePickerButton } from '@willow/ui'
import { useTranslation } from 'react-i18next'
import styles from './InspectionCheckHistoryDatePicker.css'

export default function InspectionCheckHistoryDatePicker({
  times,
  onTimesChange,
  ...rest
}) {
  const dateTime = useDateTime()
  const { t } = useTranslation()

  function handleClick(fn) {
    const now = dateTime.now()

    onTimesChange([fn(now).format(), now.format()])
  }

  return (
    <DatePicker
      type="date-time-range"
      height="large"
      helper={
        <>
          <DatePickerButton
            onClick={() => handleClick((now) => now.addHours(-1))}
            data-segment="Inspection Check History Quick Time Option"
            data-segment-props={JSON.stringify({
              option: 'Last Hour',
            })}
          >
            {t('plainText.lastHour')}
          </DatePickerButton>
          <DatePickerButton
            onClick={() => handleClick((now) => now.addHours(-4))}
            data-segment="Inspection Check History Quick Time Option"
            data-segment-props={JSON.stringify({
              option: 'Last 4 Hours',
            })}
          >
            {t('plainText.last4Hours')}
          </DatePickerButton>
          <DatePickerButton
            onClick={() => handleClick((now) => now.addHours(-24))}
            data-segment="Inspection Check History Quick Time Option"
            data-segment-props={JSON.stringify({
              option: 'Last 24 Hours',
            })}
          >
            {t('plainText.last24Hours')}
          </DatePickerButton>
          <DatePickerButton
            onClick={() => handleClick((now) => now.addHours(-48))}
            data-segment="Inspection Check History Quick Time Option"
            data-segment-props={JSON.stringify({
              option: 'Last 48 Hours',
            })}
          >
            {t('plainText.last48Hours')}
          </DatePickerButton>
          <DatePickerButton
            onClick={() => handleClick((now) => now.addDays(-7))}
            data-segment="Inspection Check History Quick Time Option"
            data-segment-props={JSON.stringify({
              option: 'Last 7 Days',
            })}
          >
            {t('plainText.last7Days')}
          </DatePickerButton>
          <DatePickerButton
            onClick={() => handleClick((now) => now.addDays(-30))}
            data-segment="Inspection Check History Quick Time Option"
            data-segment-props={JSON.stringify({
              option: 'Last 30 Days',
            })}
          >
            {t('plainText.last30Days')}
          </DatePickerButton>
          <DatePickerButton
            onClick={() => handleClick((now) => now.addDays(-90))}
            data-segment="Inspection Check History Quick Time Option"
            data-segment-props={JSON.stringify({
              option: 'Last 90 Days',
            })}
          >
            {t('plainText.last90Days')}
          </DatePickerButton>
          <DatePickerButton
            onClick={() => handleClick((now) => now.addMonths(-12))}
            data-segment="Inspection Check History Quick Time Option"
            data-segment-props={JSON.stringify({
              option: 'Last 12 Months',
            })}
          >
            {t('plainText.last12Months')}
          </DatePickerButton>
        </>
      }
      value={times}
      className={styles.inspectionCheckHistoryDatePicker}
      onChange={onTimesChange}
      data-segment="Inspection Check History Calendar Expanded"
      {...rest}
    />
  )
}
