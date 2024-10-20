import { Controller, Control } from 'react-hook-form'
import _ from 'lodash'
import { useTranslation } from 'react-i18next'
import { styled } from 'twin.macro'
import { Label } from '@willow/ui'
import { FieldSet, FieldValidationText } from './shared'
import { PartialModelOfInterest } from '../../types'

/**
 * Input component for choosing TwinChip's icon.
 * TwinChip's icon can be 2 alphabet character (a-z), where the first character is capitalized and the second char is lowercase (eg. "Xy").
 */
export default function IconPreferencesSection({
  selectedText,
  onChange,
  control,
}: {
  selectedText: string
  onChange: (text: string) => void
  control: Control<PartialModelOfInterest>
}) {
  const { t } = useTranslation()

  const handleChange = (e) => {
    // Only accept alphabet characters
    const value = e.target.value.replace(/[^a-z]/gi, '')

    onChange(_.capitalize(value)) // Ensure text is in correct format, Uppercase first letter, followed by lowercase second letter. (eg."Xy").
  }

  return (
    <FieldSet label={t('plainText.iconPreferences')}>
      <StyledLabel
        label={t('plainText.enter2CharCustomIcon')}
        id="two-char-input"
      >
        <Controller
          name={'text'}
          control={control}
          rules={{ required: true, minLength: 2 }}
          render={({ fieldState }) => {
            const { error } = fieldState

            return (
              <>
                <TwoCharInput
                  id="two-char-input"
                  $hasError={!!error}
                  placeholder="Xy"
                  maxLength={2}
                  value={selectedText || ''} // Defaulting to empty string to prevent uncontrolled input warning.
                  $hasValue={selectedText?.length > 0}
                  onChange={handleChange}
                  type="text"
                  autoComplete="off"
                  spellCheck={false}
                />
                {!!error && (
                  <FieldValidationText>
                    {error.type === 'minLength'
                      ? t('plainText.require2Char')
                      : t('plainText.requiredField')}
                  </FieldValidationText>
                )}
              </>
            )
          }}
        />
      </StyledLabel>
    </FieldSet>
  )
}

const StyledLabel = styled(Label)({
  font: '400 11px/16px Poppins',
  color: '#959595',
})

const TwoCharInput = styled.input<{ $hasValue: boolean; $hasError: boolean }>(
  ({ $hasValue, $hasError, theme }) => ({
    backgroundColor: $hasValue
      ? 'var(--theme-color-neutral-bg-accent-default)'
      : theme.color.neutral.bg.panel.default,
    borderRadius: 'var(--border-radius)',
    display: 'inline-flex',
    height: 'var(--height-medium)',
    width: '51px',
    position: 'relative',
    transition: 'all 0.2s ease',
    padding: '0 var(--padding)',

    color: 'var(--light)',
    font: '400 12px Poppins',

    border: $hasError
      ? '1px solid var(--red)'
      : `1px solid ${theme.color.neutral.border.default}`,

    outline: '0',

    '::placeholder': { color: theme.color.neutral.fg.subtle },

    '&:focus-within': {
      borderColor: 'var(--border-light)',
      boxShadow: '0 0 2px var(--border-light)',
    },

    '&:focus': {
      color: 'var(--white)',
    },

    '&:hover': {
      backgroundColor: theme.color.neutral.border.default,
      '::placeholder': { color: 'var(--border-light)' },
    },
  })
)
