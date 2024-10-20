import { useState } from 'react'
import cx from 'classnames'
import { useTranslation } from 'react-i18next'
import { Flex, NumberInput, Input, Text } from '@willow/ui'
import styles from './Tasks.css'

export default function NumericTaskFields({
  form,
  numericalValues,
  unitRequired,
  minMaxError,
  decimalRequired,
  handleNumericTask,
  setNumericalValues,
}) {
  const { t } = useTranslation()
  const [unitValueHasError, setUnitValueHasError] = useState(false)
  const [decimalValueHasError, setDecimalValueHasError] = useState(false)

  function emptyUnitFieldValidation(e) {
    if (e.target.value.length === 0) {
      setUnitValueHasError(true)
    } else {
      setUnitValueHasError(false)
    }
  }

  function emptyDecimalValidation(e) {
    if (e.target.value.length === 0) {
      setDecimalValueHasError(true)
    } else {
      setDecimalValueHasError(false)
    }
  }

  function addTask(e) {
    if (
      e.key === 'Enter' &&
      e?.target?.value.length !== 0 &&
      form.data.taskType
    ) {
      e.preventDefault()
      handleNumericTask()
    }
  }

  return (
    <div className={styles.numFieldsCtn}>
      {minMaxError && (
        <Text className={cx(styles.minMaxError, styles.minMaxErrorAdd)}>
          {minMaxError}
        </Text>
      )}
      <Flex horizontal size="medium" className={styles.numFieldsFlex}>
        <Input
          name="unit"
          value={numericalValues?.unit}
          placeholder={t('placeholder.unit')}
          required
          onChange={(unit) =>
            setNumericalValues((prevState) => ({
              ...prevState,
              unit,
            }))
          }
          onBlur={emptyUnitFieldValidation}
          className={
            unitValueHasError || unitRequired
              ? cx(styles.error, styles.numericValues)
              : styles.numericValues
          }
        />
        <NumberInput
          name="decimalPlaces"
          value={numericalValues?.decimalPlaces}
          placeholder={t('labels.decimals')}
          required
          onBlur={emptyDecimalValidation}
          onChange={(decimal) =>
            setNumericalValues((prevState) => ({
              ...prevState,
              decimalPlaces: decimal,
            }))
          }
          className={
            decimalValueHasError || decimalRequired
              ? cx(styles.error, styles.numericValues)
              : styles.numericValues
          }
          onKeyDown={addTask}
        />
        <NumberInput
          placeholder={t('labels.min')}
          value={numericalValues?.minValue}
          name="min"
          onChange={(minValue) =>
            setNumericalValues((prevState) => ({
              ...prevState,
              minValue,
            }))
          }
          className={
            !minMaxError
              ? styles.numericValues
              : cx(styles.error, styles.numericValues)
          }
        />
        <NumberInput
          placeholder={t('labels.max')}
          value={numericalValues?.maxValue}
          name="max"
          onChange={(maxValue) =>
            setNumericalValues((prevState) => ({
              ...prevState,
              maxValue,
            }))
          }
          onBlur={() => handleNumericTask()}
          className={
            !minMaxError
              ? styles.numericValues
              : cx(styles.error, styles.numericValues)
          }
        />
      </Flex>
    </div>
  )
}
