import { forwardRef, useState } from 'react'
import cx from 'classnames'
import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import {
  useDateTime,
  useForwardedRef,
  useFeatureFlag,
  DatePickerBusinessRangeOptions,
  DatePickerDayRangeOptions,
} from '@willow/ui'
import Dropdown from 'components/Dropdown/Dropdown'
import Text from 'components/Text/Text'
import { useLanguage } from '../../../providers/LanguageProvider/LanguageContext.ts'
import { DatePickerContext } from './DatePickerContext'
import Calendar from './Calendar'
import KeyboardHandler from './KeyboardHandler'
import styles from './DatePicker.css'
import QuickRangeOptions from './QuickRangeOptions.tsx'

const StyledQuickOptions = styled(QuickRangeOptions)({
  marginRight: 'var(--padding)',
  flexShrink: '1',
  flexWrap: 'wrap',
  overflow: 'hidden',
})

export default forwardRef(function DatePicker(
  {
    type = 'date',
    timezone,
    timezoneSelector,
    value,
    readOnly,
    disabled,
    error,
    min,
    max,
    placeholder,
    helper,
    position,
    height,
    className,
    onChange,

    /* Props for day range */
    onDayRangeChange,
    selectedDayRange,

    /* Props for business hour range */
    onBusinessHourRangeChange,
    selectedBusinessHourRange,
    hideBusinessHourRange,

    /* Props for reset button click */
    onResetClick,

    /* Props for quick range */
    isOuterQuickRangeEnabled = false,
    quickRangeOptions,
    selectedQuickRange,
    onSelectQuickRange,
    /**
     * If specified, prevent the user from selecting a date range that is longer than
     * this many days.
     */
    maxDays,
    'data-segment': dataSegment,
    children,
    zIndex,
    ...rest
  },
  forwardedRef
) {
  const dateTime = useDateTime()
  const featureFlags = useFeatureFlag()
  const { language } = useLanguage()
  const { t } = useTranslation()
  const [isOpen, setIsOpen] = useState(false)

  const datePickerMap = [
    {
      k: DatePickerDayRangeOptions.ALL_DAYS_KEY,
      v: _.capitalize(t('plainText.allDays')),
    },
    {
      k: DatePickerDayRangeOptions.WEEK_ENDS_KEY,
      v: _.capitalize(t('plainText.weekends')),
    },
    {
      k: DatePickerDayRangeOptions.WEEK_DAYS_KEY,
      v: _.capitalize(t('plainText.weekdays')),
    },
    {
      k: DatePickerBusinessRangeOptions.ALL_HOURS_KEY,
      v: _.capitalize(t('plainText.allHours')),
    },
    {
      k: DatePickerBusinessRangeOptions.IN_BUSINESS_HOURS_KEY,
      v: _.capitalize(t('plainText.duringBusinessHours')),
    },
    {
      k: DatePickerBusinessRangeOptions.OUT_BUSINESS_HOURS_KEY,
      v: _.capitalize(t('plainText.outsideBusinessHours')),
    },
  ].reduce((map, { k, v }) => {
    map.set(k, v)
    return map
  }, new Map())

  const dropdownRef = useForwardedRef(forwardedRef)

  let derivedValue = value
  if (type === 'date' || type === 'date-time') {
    derivedValue = [value]
  }

  function getFormattedValue(nextValue) {
    let format = 'date'
    if (type === 'date-time' || type === 'date-time-range') format = 'date-time'

    let formattedValue = dateTime(nextValue?.[0], timezone).format(
      format,
      timezone,
      language
    )
    if (formattedValue == null) {
      return ''
    }
    if (
      (type === 'date-range' || type === 'date-time-range') &&
      nextValue.length === 2
    ) {
      formattedValue = `${formattedValue} - ${dateTime(
        nextValue[1],
        timezone
      ).format(format, timezone, language)}`
    }

    if (type === 'date-business-range' && nextValue.length === 2) {
      if (featureFlags.hasFeatureToggle('businessHourRange')) {
        formattedValue = `${formattedValue} - ${dateTime(
          nextValue[1],
          timezone
        ).format(format, timezone, language)}, ${datePickerMap.get(
          selectedDayRange ?? DatePickerDayRangeOptions.ALL_DAYS_KEY
        )}, ${datePickerMap.get(
          selectedBusinessHourRange ??
            DatePickerBusinessRangeOptions.ALL_HOURS_KEY
        )}`
      } else {
        formattedValue = `${formattedValue} - ${dateTime(
          nextValue[1],
          timezone
        ).format(format, timezone, language)}`
      }
    }

    return formattedValue ?? ''
  }

  const formattedValue = getFormattedValue(derivedValue)
  const hasValue = formattedValue !== ''
  const nextPlaceholder = placeholder != null ? `- ${placeholder} -` : undefined
  const hasPlaceholder = !hasValue && nextPlaceholder != null

  const context = {
    type,
    value: derivedValue,
    min,
    max,
    helper,
    timezone,
    timezoneSelector,
    maxDays,
    quickRangeOptions,
    selectedQuickRange,
    selectQuickRange: onSelectQuickRange,
    selectedDayRange,
    onDayRangeChange,
    onBusinessHourRangeChange,
    selectedBusinessHourRange,
    onResetClick,
    zIndex,

    select(date, isCustomRange) {
      onChange(date, isCustomRange)
      dropdownRef.current.focus()
    },
  }

  return (
    <DatePickerContext.Provider value={context}>
      {quickRangeOptions?.length > 0 && isOuterQuickRangeEnabled && (
        <StyledQuickOptions
          options={quickRangeOptions}
          onSelect={(option, dateRange) => {
            onSelectQuickRange(option)
            onChange(dateRange, option)
          }}
          selected={selectedQuickRange}
          data-segment="Quick Time Option"
          timezone={timezone}
        />
      )}
      <Dropdown
        icon="calendar"
        {...rest}
        zIndex={zIndex}
        ref={dropdownRef}
        header={
          <Text className={styles.text}>
            {hasPlaceholder ? nextPlaceholder : formattedValue}
          </Text>
        }
        readOnly={readOnly}
        disabled={disabled}
        className={cx(
          styles.datePicker,
          {
            [styles.readOnly]: readOnly,
            [styles.disabled]: disabled,
            [styles.dateRange]: type === 'date-range',
            [styles.dateTimeRange]: type === 'date-time-range',
            [styles.hasValue]: hasValue,
            [styles.hasError]: error != null,
            [styles.hasPlaceholder]: hasPlaceholder,
            [styles.open]: isOpen,
            [styles.heightLarge]: height === 'large',
          },
          className
        )}
        iconClassName={styles.icon}
        position={position}
        data-segment={dataSegment}
        onIsOpenChange={setIsOpen}
      >
        <Calendar
          featureFlags={featureFlags}
          hideBusinessHourRange={hideBusinessHourRange}
        >
          {children}
        </Calendar>
        <KeyboardHandler />
      </Dropdown>
    </DatePickerContext.Provider>
  )
})
