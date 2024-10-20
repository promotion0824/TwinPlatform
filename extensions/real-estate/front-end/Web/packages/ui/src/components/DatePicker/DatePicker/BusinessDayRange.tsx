/* eslint-disable @typescript-eslint/no-empty-function */
import React from 'react'
import _ from 'lodash'
import tw, { styled } from 'twin.macro'
import { useTranslation } from 'react-i18next'
import {
  Checkbox,
  DatePickerBusinessRangeOptions,
  DatePickerDayRangeOptions,
} from '@willow/ui'
import Text from '../../Text/Text'

export default function BusinessDayRange({
  hideBusinessHourRange,
  onDayRangeChange,
  selectedDayRange = DatePickerDayRangeOptions.ALL_DAYS_KEY,
  onBusinessHourRangeChange,
  selectedBusinessHourRange = DatePickerBusinessRangeOptions.ALL_HOURS_KEY,
}: {
  hideBusinessHourRange: boolean
  onDayRangeChange: (dayRange: DatePickerDayRangeOptions) => void
  onBusinessHourRangeChange: (
    selectedBusinessHourRange: DatePickerBusinessRangeOptions
  ) => void
  selectedDayRange?: DatePickerDayRangeOptions
  selectedBusinessHourRange?: DatePickerBusinessRangeOptions
}) {
  const { t } = useTranslation()

  const handleChange = (name, value: boolean) => {
    // business logic to ensure, one checkbox from each group is always checked
    if (value === false) {
      return
    }

    if (
      [
        DatePickerDayRangeOptions.ALL_DAYS_KEY,
        DatePickerDayRangeOptions.WEEK_DAYS_KEY,
        DatePickerDayRangeOptions.WEEK_ENDS_KEY,
      ].includes(name)
    ) {
      onDayRangeChange(name)
    } else {
      onBusinessHourRangeChange(name)
    }
  }

  const dayRangeJson = [
    {
      key: DatePickerDayRangeOptions.ALL_DAYS_KEY,
      label: t('plainText.allDays'),
    },
    {
      key: DatePickerDayRangeOptions.WEEK_DAYS_KEY,
      label: t('plainText.weekdays'),
    },
    {
      key: DatePickerDayRangeOptions.WEEK_ENDS_KEY,
      label: t('plainText.weekends'),
    },
  ]

  const businessRangeJson = [
    {
      key: DatePickerBusinessRangeOptions.ALL_HOURS_KEY,
      label: t('plainText.allHours'),
    },
    {
      key: DatePickerBusinessRangeOptions.IN_BUSINESS_HOURS_KEY,
      label: t('plainText.duringBusinessHours'),
    },
    {
      key: DatePickerBusinessRangeOptions.OUT_BUSINESS_HOURS_KEY,
      label: t('plainText.outsideBusinessHours'),
    },
  ]

  return (
    <Container tw="flex" $floatLeft={hideBusinessHourRange}>
      <div>
        <Heading tw="flex">{_.toUpper(t('headers.dayRange'))}</Heading>
        {dayRangeJson.map((item) => (
          <CheckboxContainer key={item.key}>
            <Checkbox
              value={selectedDayRange === item.key}
              onChange={() =>
                handleChange(item.key, !(selectedDayRange === item.key))
              }
            >
              {_.capitalize(item.label)}
            </Checkbox>
          </CheckboxContainer>
        ))}
      </div>

      <div>
        {!hideBusinessHourRange && (
          <>
            <Heading tw="flex">
              {_.toUpper(t('headers.businessHourRange'))}
            </Heading>
            {businessRangeJson.map((item) => (
              <CheckboxContainer key={item.key}>
                <Checkbox
                  value={selectedBusinessHourRange === item.key}
                  onChange={() =>
                    handleChange(
                      item.key,
                      !(selectedBusinessHourRange === item.key)
                    )
                  }
                >
                  {_.capitalize(item.label)}
                </Checkbox>
              </CheckboxContainer>
            ))}
          </>
        )}
      </div>
    </Container>
  )
}

const Container = styled.div<{ $floatLeft: boolean }>(
  ({ $floatLeft, theme }) => ({
    justifyContent: $floatLeft ? 'flex-start' : 'space-evenly',
    marginLeft: $floatLeft ? theme.spacing.s16 : '0px',
    marginTop: theme.spacing.s24,
  })
)

const CheckboxContainer = styled.div({
  display: 'flex',

  '&&& div': {
    justifyContent: 'flex-start',
  },
})

const Heading = styled(Text)(({ theme }) => ({
  ...theme.font.heading.group,
}))
