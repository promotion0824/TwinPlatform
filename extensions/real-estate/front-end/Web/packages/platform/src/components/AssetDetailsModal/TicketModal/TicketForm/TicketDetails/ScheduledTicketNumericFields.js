import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Flex, NumberInput, Input } from '@willow/ui'
import styles from './ScheduledTicketTasks.css'

export default function ScheduledTicketNumericFields({
  form,
  unitRequired,
  decimalRequired,
  minMaxError,
  handleNumericTask,
  stNumericalValues,
  setStNumericalValues,
}) {
  const numeric = 'numeric'
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
      e?.target?.value?.length !== 0 &&
      form.data.taskType.toLowerCase() === numeric
    ) {
      handleNumericTask()
    } else if (
      e.key === 'Enter' &&
      e?.target?.value.length !== 0 &&
      form.data.taskType.toLowerCase() !== numeric
    ) {
      e.preventDefault()
    }
  }

  return (
    <Flex horizontal size="medium" className={styles.numFieldsFlex}>
      <Input
        name="unit"
        placeholder={t('placeholder.unit')}
        required
        value={stNumericalValues.unit}
        onBlur={emptyUnitFieldValidation}
        onChange={(unit) =>
          setStNumericalValues((prevState) => ({
            ...prevState,
            unit,
          }))
        }
        className={
          unitValueHasError || unitRequired
            ? [styles.error, styles.numericValues].join(' ')
            : styles.numericValues
        }
      />
      <NumberInput
        name="decimalPlaces"
        placeholder={t('labels.decimals')}
        required
        value={stNumericalValues.decimalPlaces}
        onBlur={emptyDecimalValidation}
        onChange={(decimal) =>
          setStNumericalValues((prevState) => ({
            ...prevState,
            decimalPlaces: decimal,
          }))
        }
        className={
          decimalValueHasError || decimalRequired
            ? [styles.error, styles.numericValues].join(' ')
            : styles.numericValues
        }
        onKeyDown={addTask}
      />
      <NumberInput
        placeholder={t('labels.min')}
        name="min"
        value={stNumericalValues.minValue}
        onChange={(minValue) =>
          setStNumericalValues((prevState) => ({
            ...prevState,
            minValue,
          }))
        }
        className={
          !minMaxError
            ? styles.numericValues
            : [styles.error, styles.numericValues].join(' ')
        }
      />
      <NumberInput
        placeholder={t('labels.max')}
        name="max"
        value={stNumericalValues.maxValue}
        onBlur={handleNumericTask}
        onChange={(maxValue) =>
          setStNumericalValues((prevState) => ({
            ...prevState,
            maxValue,
          }))
        }
        className={
          !minMaxError
            ? styles.numericValues
            : [styles.error, styles.numericValues].join(' ')
        }
      />
    </Flex>
  )
}
