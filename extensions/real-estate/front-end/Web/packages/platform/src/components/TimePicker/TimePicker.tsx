/* eslint-disable react/require-default-props */
import { v4 as uuidv4 } from 'uuid'
import { styled } from 'twin.macro'
import { Button, Dropdown, Text } from '@willow/ui'
import { ChangeEvent, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'

const ButtonContainer = styled.div(({ theme }) => ({
  padding: '0.5rem 1rem',
  borderTop: `1px solid ${theme.color.neutral.border.default}`,
  textAlign: 'right',
}))

const NumberInput = ({ label, ...props }) => {
  const id = useRef(uuidv4()).current
  return (
    <>
      <label
        css={{
          position: 'absolute',
          top: '1rem',
          width: '2rem',
          textAlign: 'center',
        }}
        htmlFor={id}
      >
        {label}
      </label>
      <StyledInput id={id} type="number" {...props} />
    </>
  )
}

const Colon = () => <span css={{ padding: '0 0.5rem' }}>:</span>

const initTime = () => ({
  hours: '0',
  minutes: '0',
  seconds: '0',
  offset: '+00:00',
})

export function parse(timeString: string) {
  if (timeString.length < 8 || timeString[2] !== ':' || timeString[5] !== ':') {
    throw new Error(`Malformed time: ${timeString}`)
  }
  // Slice manually rather than splitting by colon because there may be a colon
  // in the time zone offset.
  const hours = timeString.slice(0, 2)
  const minutes = timeString.slice(3, 5)
  const seconds = timeString.slice(6, 8)
  const offset = timeString.slice(8)
  return { hours, minutes, seconds, offset }
}

type TimeObj = {
  hours: string
  minutes: string
  seconds: string
  offset: string
}
const stringify = ({ hours, minutes, seconds, offset }: TimeObj) =>
  [
    hours.padStart(2, '0'),
    minutes.padStart(2, '0'),
    seconds.padStart(2, '0'),
  ].join(':') + offset

/**
 * A time input widget, with numeric inputs for hours, minutes and seconds.
 *
 * Accepts time in the following formats:
 * - 09:00:00Z
 * - 09:00:00+10:00
 * - 09:00:00-10:00
 *
 * A null time is also supported. The Clear button sets the time to a null
 * time. If the time is null, the input fields are empty. If the time is null
 * and the user enters a value into any of the inputs, the other inputs are set
 * to zero to create a valid time.
 */
export default ({
  id,
  ariaLabelledBy,
  value,
  onChange,
  readOnly = false,
  className,
}: {
  id: string
  ariaLabelledBy: string
  value: string | null
  onChange: (val: string | null) => void
  readOnly?: boolean
  className?: string
}) => {
  const { t } = useTranslation()
  const [time, setTime] = useState(value ? parse(value) : null)

  const handleChangeTimeField = (e: ChangeEvent<HTMLInputElement>) => {
    const currentTime = time ?? initTime()
    const newTime = { ...currentTime, [e.target.name]: e.target.value }
    setTime(newTime)
    onChange(stringify(newTime))
  }

  const handleClear = () => {
    setTime(null)
    onChange(null)
  }

  return (
    <StyledDropdown
      id={id}
      aria-labelledby={ariaLabelledBy}
      className={className}
      icon="clock"
      disabled={readOnly}
      header={<Text>{time != null ? stringify(time) : ''}</Text>}
    >
      <div css={{ minWidth: '198px', padding: '1rem', textAlign: 'center' }}>
        <NumberInput
          name="hours"
          label={t('plainText.hr')}
          min={0}
          max={23}
          value={time?.hours ?? ''}
          onChange={handleChangeTimeField}
          disabled={readOnly}
        />
        <Colon />
        <NumberInput
          name="minutes"
          label={t('plainText.min')}
          min={0}
          max={59}
          value={time?.minutes ?? ''}
          onChange={handleChangeTimeField}
          disabled={readOnly}
        />
        <Colon />
        <NumberInput
          name="seconds"
          label={t('plainText.sec')}
          min={0}
          max={59}
          value={time?.seconds ?? ''}
          onChange={handleChangeTimeField}
          disabled={readOnly}
        />
      </div>
      <ButtonContainer>
        <Button css={{ fontWeight: '600' }} onClick={handleClear}>
          {t('plainText.clear')}
        </Button>
      </ButtonContainer>
    </StyledDropdown>
  )
}

const StyledInput = styled.input(({ theme }) => ({
  marginTop: '1rem',
  width: '2rem',

  backgroundColor: theme.color.neutral.bg.panel.default,
  border: `1px solid ${theme.color.neutral.border.default}`,
  borderRadius: 'var(--border-radius)',

  color: '#D9D9D9',
  textAlign: 'center',

  '&::-webkit-outer-spin-button, &::-webkit-inner-spin-button': {
    appearance: 'none',
  },
}))

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
  fontSize: 'var(--font-small)',
  fontWeight: 'var(--font-weight-500)',
}))
