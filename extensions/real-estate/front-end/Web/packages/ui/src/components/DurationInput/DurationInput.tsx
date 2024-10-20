/* eslint-disable react/require-default-props */
import _ from 'lodash'
import { v4 as uuidv4 } from 'uuid'
import React, { CSSProperties, useState } from 'react'
import { useTranslation, TFunction } from 'react-i18next'
import { parse } from 'tinyduration'
import tw, { styled } from 'twin.macro'
import Button from '../Button/Button'
import Dropdown from '../Dropdown/Dropdown'
import Input from '../Input/Input'
import Icon from '../Icon/Icon'
import ToggleSwitch from '../ToggleSwitch/ToggleSwitch'

type RegularDuration<T> = {
  years: T
  months: T
  days: T
  hours: T
  minutes: T
  seconds: T
}

type WeeksDuration<T> = { weeks: T }

type Duration<T> = RegularDuration<T> | WeeksDuration<T>

const fields: (keyof RegularDuration<string>)[] = [
  'years',
  'months',
  'days',
  'hours',
  'minutes',
  'seconds',
]

export type InputDuration = Duration<number | string>

export type DurationState = Duration<string> | null

function zeroRegularDuration(): RegularDuration<string> {
  return {
    years: '0',
    months: '0',
    days: '0',
    hours: '0',
    minutes: '0',
    seconds: '0',
  }
}

function zeroWeeksDuration(): WeeksDuration<string> {
  return { weeks: '0' }
}

/**
 * A widget for editing a duration value, based on the ISO 8601 duration standard.
 *
 * If includeWeeksDuration is false (default), it contains inputs for years,
 * months, days, hours, minutes, and seconds.
 *
 * If includeWeeksDuration is true, it has two parts. The first
 * contains inputs for years, months, days, hours, minutes and seconds. The
 * second contains only a single input for weeks. The two parts are mutually
 * exclusive and the user can toggle between them via a switch located between
 * them.
 *
 * We designed this widget on the assumption that ADT's duration type
 * supported the ISO 8601 duration spec as it claims to in the docs [1],
 * however it does not actually support weeks due to issues which they plan
 * to fix. So we are hiding the weeks selector for now.
 *
 * Note the input `value` is of type `Duration<number | string>` meaning that
 * the component accepts either numbers or strings as input, but `onChange`
 * will only provide strings as output. This is so input is not lost if the
 * user enters non-numeric inputs, such as by clearing an input.
 *
 * A null duration is also supported. The Clear button sets the duration to a
 * null duration. If the duration is null, the input fields are empty. If the
 * duration is null and the user enters a value into any of the inputs, the
 * other inputs are set to zero to create a valid duration.
 *
 * [1] https://github.com/Azure/opendigitaltwins-dtdl/blob/master/DTDL/v2/dtdlv2.md#primitive-schemas
 */
export default function DurationInput({
  id,
  ariaLabelledBy,
  value,
  onChange,
  readOnly = false,
  includeWeeksSection = false,
  className,
  style,
}: {
  id: string
  ariaLabelledBy: string
  value: InputDuration
  onChange: (val: DurationState) => void
  readOnly?: boolean
  includeWeeksSection?: boolean
  className?: string
  style?: CSSProperties
}) {
  const { t } = useTranslation()

  // We keep the top & bottom values independently so the user can switch
  // between them without losing anything.
  const [{ top, bottom }, setValues] = useState(() => {
    if (value == null) {
      return {
        top: null,
        bottom: null,
      }
    } else if ('years' in value) {
      return {
        top: _.mapValues(value, (v) => v.toString()),
        bottom: zeroWeeksDuration(),
      }
    } else if ('weeks' in value) {
      return {
        top: zeroRegularDuration(),
        bottom: _.mapValues(value, (v) => v.toString()),
      }
    } else {
      throw new Error(`Invalid value ${value}`)
    }
  })
  const isTopActive = value == null || 'years' in value

  return (
    <StyledDropdown
      id={id}
      aria-labelledby={ariaLabelledBy}
      header={formatDuration(isTopActive ? top : bottom, t)}
      readOnly={readOnly}
      className={className}
      style={style}
    >
      <div tw="my-2 mx-4">
        <div>
          {fields.map((f) => (
            <Row
              key={f}
              label={t(`plainText.${f}`)}
              value={top?.[f].toString() ?? ''}
              disabled={!isTopActive}
              onChange={(val) => {
                const newTop = { ...(top ?? zeroRegularDuration()), [f]: val }
                onChange(newTop)
                setValues({
                  top: newTop,
                  bottom: bottom ?? zeroWeeksDuration(),
                })
              }}
            />
          ))}
        </div>
        {includeWeeksSection && (
          <>
            <div tw="text-right my-2">
              <ToggleSwitch
                checked={!isTopActive}
                onChange={(val: boolean) => {
                  onChange(val ? bottom : top)
                }}
              />
            </div>
            <Row
              label={t('plainText.week_other')}
              disabled={isTopActive}
              value={bottom?.weeks ?? ''}
              onChange={(val) => {
                const newBottom = {
                  ...(bottom ?? zeroWeeksDuration()),
                  weeks: val,
                }
                onChange(newBottom)
                setValues({
                  bottom: newBottom,
                  top: top ?? zeroRegularDuration(),
                })
              }}
            />
          </>
        )}
      </div>
      <hr />
      <div tw="mx-4 my-2 text-right">
        <ClearButton
          onClick={() => {
            const values = {
              top: null,
              bottom: null,
            }
            onChange(isTopActive ? values.top : values.bottom)
            setValues(values)
          }}
        >
          {t('plainText.clear')}
        </ClearButton>
      </div>
    </StyledDropdown>
  )
}

function Row({
  label,
  value,
  onChange,
  disabled = false,
}: {
  label: string
  value: string
  onChange: (val: string) => void
  disabled?: boolean
}) {
  const [inputId] = useState(uuidv4())

  function add(val: number) {
    let currentVal = parseFloat(value)
    if (Number.isNaN(currentVal)) {
      // If we have a blank or otherwise invalid number in the text box and the
      // user hits + or -, just pretend we started with zero.
      currentVal = 0
    }
    onChange((currentVal + val).toString())
  }

  return (
    <div tw="my-2 flex items-center">
      <label htmlFor={inputId} tw="flex-initial width[70px]">
        {label}
      </label>
      <div tw="flex-initial width[50px]">
        <NumberField
          id={inputId}
          value={value}
          onChange={onChange}
          disabled={disabled}
        />
      </div>
      <div tw="flex-initial justify-center flex items-center select-none width[40px]">
        <div tw="ml-4 flex-initial">
          <IconButton
            icon="remove"
            size="tiny"
            onClick={() => add(-1)}
            disabled={disabled}
          />
        </div>
        <div tw="ml-1 flex-initial">
          <IconButton
            icon="add"
            size="small"
            onClick={() => add(1)}
            disabled={disabled}
          />
        </div>
      </div>
    </div>
  )
}

/**
 * Give the dropdown the same styles as Selects get. These styles exist
 * in a few places and are derived from
 * `packages/ui/src/components/Select/Select/Select.css`.
 * We may factor this so that all Dropdowns have these styles.
 */
const StyledDropdown = styled(Dropdown)(({ theme }) => ({
  backgroundColor: theme.color.neutral.bg.panel.default,
  border: `1px solid ${theme.color.neutral.border.default}`,
  borderRadius: 'var(--border-radius)',

  '&:focus-within': {
    borderColor: 'var(--border-light)',
    boxShadow: '0 0 2px var(--border-light)',
  },
  '&:hover': {
    backgroundColor: theme.color.neutral.border.default,
  },

  height: 'var(--height-medium)',
  padding: '0 var(--padding)',
  position: 'relative',
}))

const NumberField = styled(Input)({
  width: '100%',
  input: {
    textAlign: 'right',
  },
})

const IconButton = styled(Icon)({
  cursor: 'pointer',
  ':hover': {
    color: 'white',
  },
})

const ClearButton = styled(Button)({
  fontWeight: 600,
})

/**
 * Parse an ISO 8601 duration (like "P1DT12H") and turn it into
 * `{ years: 0, months: 0, days: 1, hours: 12, minutes: 0, seconds: 0}`.
 */
export function parseIsoDuration(
  duration: string | null
): Duration<number> | null {
  if (duration == null) {
    return null
  }

  // `tinyduration.parse` does most of the work for us but it doesn't fill in
  // keys for values of zero, so we do that ourselves.
  const sparse = parse(duration)
  if (!('weeks' in sparse)) {
    for (const f of fields) {
      if (!(f in sparse)) {
        sparse[f] = 0
      }
    }
  }
  return sparse as Duration<number>
}

/**
 * Returns `true` if the duration is valid, false otherwise.
 */
export function isValid(duration: DurationState) {
  if (duration == null) {
    return false
  }
  if ('years' in duration) {
    const numbers = fields.map((f) => Number(duration[f]))
    if (numbers.some((n) => Number.isNaN(n) || n < 0)) {
      return false
    }
    // The standard allows the *smallest* value to have a decimal component.
    // Hence if we find a decimal anywhere, all the values after it must be
    // zero.
    const index = numbers.findIndex((v) => !Number.isInteger(v))
    if (index !== -1) {
      return numbers.slice(index + 1).every((v) => v === 0)
    } else {
      return true
    }
  } else {
    const number = Number(duration.weeks)
    return !Number.isNaN(number) && number >= 0
  }
}

/**
 * Create an ISO 8601 duration string from a DurationState, but omit
 * fields with values of zero where possible.
 */
export function toSimplifiedIsoDuration(
  duration: DurationState
): string | null {
  if (duration == null) {
    return null
  }

  function makeField(prop: keyof RegularDuration<string>) {
    return duration != null && parseFloat(duration[prop]) !== 0
      ? `${duration[prop]}${prop[0].toUpperCase()}`
      : []
  }

  if ('weeks' in duration) {
    return `P${duration.weeks}W`
  } else {
    const dateFields = ['years', 'months', 'days'].flatMap(makeField)
    const timeFields = ['hours', 'minutes', 'seconds'].flatMap(makeField)

    if (dateFields.length > 0 && timeFields.length > 0) {
      return `P${dateFields.join('')}T${timeFields.join('')}`
    } else if (dateFields.length > 0) {
      return `P${dateFields.join('')}`
    } else if (timeFields.length > 0) {
      return `PT${timeFields.join('')}`
    } else {
      return 'PT0S'
    }
  }
}

/**
 * Format a duration for display in the dropdown heading. Note this is not the
 * same as the ISO 8601 format (we could use tinyduration.serialize for that).
 */
function formatDuration(duration: DurationState, t: TFunction) {
  if (duration == null) {
    return ''
  }

  if ('years' in duration) {
    const { years, months, days, hours, minutes, seconds } = duration
    const parts: string[] = []
    if (parseFloat(years) !== 0) {
      parts.push(t('plainText.years_short', { num: years }))
    }
    if (parseFloat(months) !== 0) {
      parts.push(t('plainText.months_short', { num: months }))
    }
    if (parseFloat(days) !== 0) {
      parts.push(t('plainText.days_short', { num: days }))
    }

    if (
      parseFloat(hours) !== 0 ||
      parseFloat(minutes) !== 0 ||
      parseFloat(seconds) !== 0
    ) {
      parts.push(
        [hours, minutes, seconds].map((v) => _.padStart(v, 2, '0')).join(':')
      )
    }

    if (parts.length > 0) {
      return parts.join(' ')
    } else {
      return '0'
    }
  } else {
    return `${duration.weeks}W`
  }
}
