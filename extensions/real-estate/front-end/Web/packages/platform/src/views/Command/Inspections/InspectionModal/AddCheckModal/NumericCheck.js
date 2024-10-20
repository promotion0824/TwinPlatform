import { useForm, Flex, Input, NumberInput } from '@willow/ui'
import { useTranslation } from 'react-i18next'

export default function NumericCheck() {
  const checkForm = useForm()
  const { t } = useTranslation()

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
          name="typeValue"
          label={t('placeholder.unit')}
          required
          onChange={handleChange}
        />
        <NumberInput
          name="decimalPlaces"
          label={t('labels.decimals')}
          required
        />
      </Flex>
      <Flex horizontal fill="equal" size="large">
        <NumberInput name="minValue" label={t('labels.min')} />
        <NumberInput name="maxValue" label={t('labels.max')} />
      </Flex>
    </>
  )
}
