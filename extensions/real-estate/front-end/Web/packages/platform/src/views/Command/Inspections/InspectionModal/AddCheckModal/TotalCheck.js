import { useForm, Flex, Input, NumberInput } from '@willow/ui'
import { css } from 'twin.macro'
import { useTheme } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import { titleCase } from '@willow/common'

export default function TotalCheck() {
  const checkForm = useForm()
  const theme = useTheme()
  const {
    i18n: { language },
    t,
  } = useTranslation()

  const isMultiplierReadOnly = !!checkForm?.initialData?.name

  function handleChange(typeValue) {
    checkForm.setData((prevData) => ({
      ...prevData,
      typeValue,
    }))
  }

  return (
    <>
      <Flex horizontal fill="equal" size="large">
        <Input
          data-cy="inspection-check-totalValue"
          name="typeValue"
          label={t('placeholder.unit')}
          required
          onChange={handleChange}
        />
        <NumberInput
          data-cy="inspection-check-decimalPlaces"
          name="decimalPlaces"
          label={t('labels.decimals')}
          required
        />
      </Flex>
      <div
        css={css({
          display: 'flex',
          gap: theme.spacing.s16,
          flexWrap: 'wrap',
          '& > div': {
            flexGrow: 1,
          },
        })}
      >
        {[
          {
            name: 'minValue',
            label: 'labels.minimumPercentage',
            min: 0,
            value: checkForm.data.minValue,
          },
          {
            name: 'maxValue',
            label: 'labels.maximumPercentage',
            min: 0,
            value: checkForm.data.maxValue,
          },
          {
            name: 'multiplier',
            label: 'labels.multiplier',
            min: 1,
            readOnly: isMultiplierReadOnly,
            value: checkForm.data.multiplier ?? 1,
          },
        ].map(({ name, label, min, readOnly, value }) => (
          <NumberInput
            key={name}
            name={name}
            value={value}
            label={titleCase({
              text: t(label),
              language,
            })}
            min={min}
            readOnly={!!readOnly}
          />
        ))}
      </div>
    </>
  )
}
