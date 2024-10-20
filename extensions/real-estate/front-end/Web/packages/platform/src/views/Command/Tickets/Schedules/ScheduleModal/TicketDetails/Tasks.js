/* eslint-disable no-empty */
import { useRef, useState } from 'react'
import {
  useForm,
  Text,
  Flex,
  Input,
  Select,
  Option,
  useNumericTaskFormValidation,
} from '@willow/ui'
import { IconButton } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import TaskDetails from './TaskDetails'
import NumericTaskFields from './NumericTaskFields'
import styles from './Tasks.css'

export default function Tasks({ isReadOnly }) {
  const numeric = 'numeric'
  const form = useForm()
  const newTaskRef = useRef()
  const containerRef = useRef()
  const { t } = useTranslation()
  const emptyString = ''
  const errors = form.errors.find((formError) => formError.name === 'tasks')

  const [numericalValues, setNumericalValues] = useState({
    unit: emptyString,
    decimalPlaces: emptyString,
    minValue: emptyString,
    maxValue: emptyString,
  })
  const [newTask, setNewTask] = useState(emptyString)
  const [type, setType] = useState(emptyString)
  const [showNumericalFields, setShowNumericalFields] = useState(false)
  const [minMaxError, setMinMaxError] = useState(emptyString)

  const {
    unitRequired,
    decimalRequired,
    showAllRequiredErrors,
    setShowAllRequiredErrors,
    showUnitError,
    showDecimalError,
  } = useNumericTaskFormValidation(
    newTask,
    numericalValues.unit,
    numericalValues.decimalPlaces
  )

  function clearForm() {
    setNewTask(emptyString)
    setNumericalValues({})
    setMinMaxError(emptyString)
    setShowAllRequiredErrors(false)
  }

  function handleAddTaskOnBlur(e) {
    if (e.target.value.length > 0 && type) {
      form.setData((prevData) => ({
        ...prevData,
        tasks: [
          ...prevData.tasks,
          {
            description: e.target.value,
            type,
          },
        ],
      }))
    }
    setNewTask(emptyString)
  }

  function handleNoMaxTask() {
    form.setData((prevData) => ({
      ...prevData,
      tasks: [
        ...prevData.tasks,
        {
          description: newTask,
          type,
          unit: numericalValues.unit,
          decimalPlaces: numericalValues.decimalPlaces,
          minValue: numericalValues.minValue,
        },
      ],
    }))
  }

  function handleNoMinTask() {
    form.setData((prevData) => ({
      ...prevData,
      tasks: [
        ...prevData.tasks,
        {
          description: newTask,
          type,
          unit: numericalValues.unit,
          decimalPlaces: numericalValues.decimalPlaces,
          maxValue: numericalValues.maxValue,
        },
      ],
    }))
  }

  function handleNoMinNoMaxTask() {
    form.setData((prevData) => ({
      ...prevData,
      tasks: [
        ...prevData.tasks,
        {
          description: newTask,
          type,
          unit: numericalValues.unit,
          decimalPlaces: numericalValues.decimalPlaces,
        },
      ],
    }))
  }

  function handleMinMaxTask() {
    form.setData((prevData) => ({
      ...prevData,
      tasks: [
        ...prevData.tasks,
        {
          description: newTask,
          type,
          unit: numericalValues.unit,
          decimalPlaces: numericalValues.decimalPlaces,
          minValue: numericalValues.minValue,
          maxValue: numericalValues.maxValue,
        },
      ],
    }))
  }

  // eslint-disable-next-line complexity
  function handleNumericTask() {
    if (newTask) {
      if ((unitRequired || decimalRequired) && !showAllRequiredErrors) {
        setShowAllRequiredErrors(true)
        return
      }

      if (
        numericalValues.unit &&
        (numericalValues.decimalPlaces ||
          numericalValues.decimalPlaces === 0) &&
        (numericalValues.minValue || numericalValues.minValue === 0) &&
        (numericalValues.maxValue === null ||
          numericalValues.maxValue === undefined ||
          numericalValues.maxValue === '')
      ) {
        handleNoMaxTask()
        newTaskRef.current.focus()
        clearForm()
      } else if (
        numericalValues.unit &&
        (numericalValues.decimalPlaces ||
          numericalValues.decimalPlaces === 0) &&
        (numericalValues.maxValue || numericalValues.maxValue === 0) &&
        (numericalValues.minValue === null ||
          numericalValues.minValue === undefined ||
          numericalValues.minValue === '')
      ) {
        handleNoMinTask()
        newTaskRef.current.focus()
        clearForm()
      } else if (
        numericalValues.unit &&
        (numericalValues.decimalPlaces ||
          numericalValues.decimalPlaces === 0) &&
        (numericalValues.minValue || numericalValues.minValue === 0) &&
        (numericalValues.maxValue || numericalValues.maxValue === 0)
      ) {
        if (numericalValues.minValue > numericalValues.maxValue) {
          setMinMaxError('Min cannot be more than max')
          return
        }
        setMinMaxError(emptyString)
        handleMinMaxTask()
        newTaskRef.current.focus()
        clearForm()
      } else if (
        numericalValues.unit &&
        (numericalValues.decimalPlaces ||
          numericalValues.decimalPlaces === 0) &&
        (numericalValues.maxValue === null ||
          numericalValues.maxValue === undefined ||
          numericalValues.maxValue === '') &&
        (numericalValues.minValue === null ||
          numericalValues.minValue === undefined ||
          numericalValues.minValue === '')
      ) {
        handleNoMinNoMaxTask()
        newTaskRef.current.focus()
        clearForm()
      }
    }
  }

  function handleTaskTypeChange(taskForm, taskType) {
    if (taskType?.toLowerCase() === numeric) {
      setShowNumericalFields(true)
    } else {
      setShowAllRequiredErrors(false)
      setMinMaxError(emptyString)
      setShowNumericalFields(false)
    }
    taskForm?.setData((prevData) => {
      return {
        ...prevData,
        taskType,
      }
    })
    setType(taskType)
  }

  function addTask(e) {
    if (
      e.key === 'Enter' &&
      e?.target?.value.length !== 0 &&
      form.data.taskType?.toLowerCase() === numeric
    ) {
      if ((unitRequired || decimalRequired) && !showAllRequiredErrors) {
        setShowAllRequiredErrors(true)
      }
    } else if (
      e.key === 'Enter' &&
      e?.target?.value.length !== 0 &&
      form.data.taskType?.toLowerCase() !== numeric
    ) {
      e.preventDefault()
      newTaskRef.current.blur()
    }
  }

  return (
    <>
      <Text>{t('labels.tasks')}</Text>
      <Flex ref={containerRef} size="medium">
        {form.data.tasks.map((task, i) => {
          const currentError = errors?.nestedErrors?.find(
            (taskError) => taskError.index === i
          )
          return (
            <TaskDetails
              task={task}
              isReadOnly={isReadOnly}
              // eslint-disable-next-line react/no-array-index-key
              key={task.description + task.type + i}
              i={i}
              form={form}
              dataIndex={i}
              {...(currentError && {
                error: currentError.errors,
              })}
            />
          )
        })}

        <Flex
          horizontal
          fill="content"
          size="medium"
          className={styles.addTask}
        >
          <Select
            name="taskType"
            required
            placeholder={t('labels.type')}
            style={{ minWidth: '110px' }}
            onChange={(taskType) => handleTaskTypeChange(form, taskType)}
          >
            <Option value="Checkbox">{t('plainText.checkbox')}</Option>
            <Option value="Numeric">{t('plainText.numeric')}</Option>
          </Select>
          <Input
            name="taskDesc"
            value={newTask}
            ref={newTaskRef}
            placeholder={t('placeholder.taskDescription')}
            onChange={(nextNewTask) => setNewTask(nextNewTask)}
            onBlur={!showNumericalFields ? handleAddTaskOnBlur : null}
            onKeyDown={addTask}
          />
          {!isReadOnly && (
            <IconButton
              icon="add"
              kind="secondary"
              background="transparent"
              tabIndex={-1}
              onClick={handleNumericTask}
            />
          )}
        </Flex>
        {showNumericalFields && (
          <NumericTaskFields
            form={form}
            numericalValues={numericalValues}
            unitRequired={showUnitError}
            decimalRequired={showDecimalError}
            minMaxError={minMaxError}
            handleNumericTask={handleNumericTask}
            setNumericalValues={setNumericalValues}
          />
        )}
      </Flex>
    </>
  )
}
