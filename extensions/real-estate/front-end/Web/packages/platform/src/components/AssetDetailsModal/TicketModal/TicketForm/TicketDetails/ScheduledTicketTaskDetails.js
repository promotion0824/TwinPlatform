/* eslint-disable complexity */
import { useEffect, useState } from 'react'
import cx from 'classnames'
import { Flex, Checkbox, Text, FormatedNumberInput } from '@willow/ui'
import { IconButton } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import styles from './ScheduledTicketTasks.css'

export default function ScheduledTicketTaskDetails({ task, props, i, form }) {
  const { t } = useTranslation()
  const numeric = 'numeric'
  const [showNumericalValues, setShowNumericalValues] = useState(true)
  const [error, setError] = useState(false)
  const [isDisabled, setIsDisabled] = useState(false)
  const [minError, setMinError] = useState('')
  const [maxError, setMaxError] = useState('')

  useEffect(() => {
    if (task.type.toLowerCase() === numeric) {
      setShowNumericalValues(true)
      setIsDisabled(true)
    } else {
      setShowNumericalValues(false)
      setIsDisabled(false)
    }
  }, [task])

  function handleDeleteClick(index) {
    form.setData((prevData) => ({
      ...prevData,
      tasks: prevData.tasks.filter((prevTasks, prevI) => prevI !== index),
    }))
  }

  function clearErrors() {
    setError(false)
    setMinError('')
    setMaxError('')
  }

  function handleMinValueError() {
    setError(true)
    setMinError(t('interpolation.belowThreshold', { value: task.minValue }))
    setMaxError('')
  }

  function handleMaxValueError() {
    setError(true)
    setMaxError(t('interpolation.aboveThreshold', { value: task.maxValue }))
    setMinError('')
  }

  function handleCheckboxChange(isCompleted) {
    form.setData((prevData) => ({
      ...prevData,
      tasks: prevData.tasks.map((prevTask, taskI) =>
        i === taskI
          ? {
              ...prevTask,
              isCompleted,
            }
          : prevTask
      ),
    }))
  }

  function setFormData(v, index) {
    form.setData((prevData) => ({
      ...prevData,
      tasks: prevData.tasks.map((prevTask, prevI) =>
        prevI === index
          ? {
              id: task.id,
              taskName: task.taskName,
              type: task.type,
              numberValue: v,
              unit: task.unit,
              decimalPlaces: task.decimalPlaces,
              minValue: task.minValue,
              maxValue: task.maxValue,
              isCompleted: v !== null,
            }
          : prevTask
      ),
    }))
  }

  function handleNumValueChange(nvString) {
    let nv = parseFloat(nvString)

    if (Number.isNaN(nv) || !Number.isFinite(nv)) {
      clearErrors()
      nv = null
    } else {
      if (
        task.minValue !== undefined &&
        task.minValue !== null &&
        (task.maxValue === undefined || task.maxValue === null)
      ) {
        if (nv < task.minValue) {
          handleMinValueError()
        } else {
          clearErrors()
        }
      }
      if (
        (task.maxValue !== undefined &&
          task.maxValue !== null &&
          task.minValue === undefined) ||
        task.minValue === null
      ) {
        if (nv > task.maxValue) {
          handleMaxValueError()
        } else {
          clearErrors()
        }
      }
      if (
        task.maxValue !== undefined &&
        task.maxValue !== null &&
        task.minValue !== undefined &&
        task.minValue !== null
      ) {
        if (nv > task.maxValue) {
          handleMaxValueError()
        } else if (nv < task.minValue) {
          handleMinValueError()
        } else if (nv <= task.maxValue || nv >= task.minValue) {
          clearErrors()
        }
      }
    }

    setFormData(nv, i)
  }

  function onNumericEntryBlur() {
    clearErrors()
  }

  return (
    <>
      <Flex>
        {error && minError ? (
          <Text className={styles.errorTxt}>{minError}</Text>
        ) : (
          [
            error && maxError ? (
              <Text className={styles.errorTxt}>{maxError}</Text>
            ) : null,
          ]
        )}
      </Flex>
      <Flex
        key={i} // eslint-disable-line
        horizontal
        fill="content"
        size="medium"
        align="middle"
      >
        <Checkbox
          value={task.isCompleted}
          onChange={(isCompleted) => handleCheckboxChange(isCompleted)}
          readOnly={props.readOnly}
          disabled={isDisabled}
        />
        <Text className={styles.taskName}>{task.taskName}</Text>
        {showNumericalValues && (
          <Flex horizontal size="medium">
            <div className={styles.numberValue}>
              <FormatedNumberInput
                name="numberValue"
                className={
                  !error
                    ? styles.numberValue
                    : cx(styles.error, styles.numberValue)
                }
                value={task.numberValue?.toString() ?? ''}
                placeholder={t('placeholder.entryDash')}
                required
                onChange={(numEntry) => handleNumValueChange(numEntry)}
                onBlur={(e) => onNumericEntryBlur(e, i)}
                fixedDecimalScale
                decimalScale={task.decimalPlaces}
                inputMode="decimal"
                step={1 / 10 ** task.decimalPlaces}
              />
              <span className={styles.unitValue}>{task.unit}</span>
            </div>
          </Flex>
        )}
        {!props.readOnly && !props.disabled && (
          <IconButton
            icon="delete"
            kind="secondary"
            background="transparent"
            tabIndex={-1}
            onClick={() => handleDeleteClick(i)}
          />
        )}
      </Flex>
    </>
  )
}
